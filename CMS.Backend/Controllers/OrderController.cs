/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 22/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ Ä‘Æ¡n hÃ ng, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, thÃªm, sá»­a, xÃ³a vÃ  thay Ä‘á»•i tráº¡ng thÃ¡i Ä‘Æ¡n hÃ ng.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // OrderController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /Order.
    [Authorize(Roles = "Admin,Staff")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch táº¥t cáº£ Ä‘Æ¡n hÃ ng kÃ¨m tÃªn khÃ¡ch hÃ ng.
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // Action Details hiá»ƒn thá»‹ chi tiáº¿t má»™t Ä‘Æ¡n hÃ ng kÃ¨m danh sÃ¡ch sáº£n pháº©m.
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Action GET Create má»Ÿ form thÃªm Ä‘Æ¡n hÃ ng má»›i.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCustomerList();
            return View();
        }

        // Action POST Create lÆ°u Ä‘Æ¡n hÃ ng má»›i vÃ o database.
        [HttpPost]
        public IActionResult Create(Order model)
        {
            ModelState.Remove(nameof(Order.Customer));
            ModelState.Remove(nameof(Order.OrderDetails));

            if (!ModelState.IsValid)
            {
                LoadCustomerList(model.CustomerId);
                return View(model);
            }

            model.OrderDate = model.OrderDate == default ? DateTime.Now : model.OrderDate;

            _context.Orders.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "ÄÆ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c táº¡o thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit má»Ÿ form chá»‰nh sá»­a Ä‘Æ¡n hÃ ng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            LoadCustomerList(order.CustomerId);
            return View(order);
        }

        // Action POST Edit cáº­p nháº­t Ä‘Æ¡n hÃ ng vÃ o database.
        [HttpPost]
        public IActionResult Edit(Order model)
        {
            var existingOrder = _context.Orders.AsNoTracking().FirstOrDefault(o => o.Id == model.Id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Order.Customer));
            ModelState.Remove(nameof(Order.OrderDetails));

            if (!ModelState.IsValid)
            {
                LoadCustomerList(model.CustomerId);
                return View(model);
            }

            model.OrderDate = model.OrderDate == default ? existingOrder.OrderDate : model.OrderDate;

            _context.Orders.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "ÄÆ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a Ä‘Æ¡n hÃ ng theo id (bao gá»“m cáº£ chi tiáº¿t Ä‘Æ¡n hÃ ng).
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }
                _context.Orders.Remove(order);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "ÄÆ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c xÃ³a thÃ nh cÃ´ng!";
            }

            return RedirectToAction("Index");
        }

        // HÃ m dÃ¹ng chung Ä‘á»ƒ náº¡p dropdown khÃ¡ch hÃ ng cho form Create/Edit.
        private void LoadCustomerList(int? selectedId = null)
        {
            ViewBag.CustomerList = new SelectList(
                _context.Customers.ToList(),
                "Id",
                "FullName",
                selectedId
            );
        }
    }
}

