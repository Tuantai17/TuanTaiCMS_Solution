/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 22/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ chi tiáº¿t Ä‘Æ¡n hÃ ng, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, thÃªm, sá»­a vÃ  xÃ³a chi tiáº¿t Ä‘Æ¡n hÃ ng.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // OrderDetailController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /OrderDetail.
    [Authorize(Roles = "Admin,Staff")]
    public class OrderDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch táº¥t cáº£ chi tiáº¿t Ä‘Æ¡n hÃ ng kÃ¨m tÃªn Ä‘Æ¡n hÃ ng vÃ  sáº£n pháº©m.
        public IActionResult Index(int? orderId)
        {
            var query = _context.OrderDetails
                .Include(od => od.Order)
                    .ThenInclude(o => o!.Customer)
                .Include(od => od.Product)
                .AsQueryable();

            // Náº¿u cÃ³ orderId thÃ¬ lá»c theo Ä‘Æ¡n hÃ ng Ä‘Ã³.
            if (orderId.HasValue)
            {
                query = query.Where(od => od.OrderId == orderId.Value);
                ViewBag.FilterOrderId = orderId.Value;
            }

            var orderDetails = query
                .OrderBy(od => od.OrderId)
                .ToList();

            return View(orderDetails);
        }

        // Action GET Create má»Ÿ form thÃªm chi tiáº¿t Ä‘Æ¡n hÃ ng.
        [HttpGet]
        public IActionResult Create(int? orderId)
        {
            LoadDropdowns(orderId);
            var model = new OrderDetail();
            if (orderId.HasValue)
            {
                model.OrderId = orderId.Value;
            }
            return View(model);
        }

        // Action POST Create lÆ°u chi tiáº¿t Ä‘Æ¡n hÃ ng má»›i vÃ o database.
        [HttpPost]
        public IActionResult Create(OrderDetail model)
        {
            ModelState.Remove(nameof(OrderDetail.Order));
            ModelState.Remove(nameof(OrderDetail.Product));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.OrderId);
                return View(model);
            }

            _context.OrderDetails.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Chi tiáº¿t Ä‘Æ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c thÃªm thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit má»Ÿ form chá»‰nh sá»­a chi tiáº¿t Ä‘Æ¡n hÃ ng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var orderDetail = _context.OrderDetails.Find(id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            LoadDropdowns(orderDetail.OrderId, orderDetail.ProductId);
            return View(orderDetail);
        }

        // Action POST Edit cáº­p nháº­t chi tiáº¿t Ä‘Æ¡n hÃ ng vÃ o database.
        [HttpPost]
        public IActionResult Edit(OrderDetail model)
        {
            ModelState.Remove(nameof(OrderDetail.Order));
            ModelState.Remove(nameof(OrderDetail.Product));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.OrderId, model.ProductId);
                return View(model);
            }

            _context.OrderDetails.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Chi tiáº¿t Ä‘Æ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a chi tiáº¿t Ä‘Æ¡n hÃ ng theo id.
        public IActionResult Delete(int id)
        {
            var orderDetail = _context.OrderDetails.Find(id);

            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Chi tiáº¿t Ä‘Æ¡n hÃ ng Ä‘Ã£ Ä‘Æ°á»£c xÃ³a thÃ nh cÃ´ng!";
            }

            return RedirectToAction("Index");
        }

        // HÃ m dÃ¹ng chung Ä‘á»ƒ náº¡p dropdown Ä‘Æ¡n hÃ ng vÃ  sáº£n pháº©m cho form Create/Edit.
        private void LoadDropdowns(int? selectedOrderId = null, int? selectedProductId = null)
        {
            // Hiá»ƒn thá»‹ Ä‘Æ¡n hÃ ng kÃ¨m tÃªn khÃ¡ch hÃ ng cho dá»… nháº­n biáº¿t.
            var orders = _context.Orders
                .Include(o => o.Customer)
                .ToList()
                .Select(o => new { o.Id, Display = $"ÄH#{o.Id} - {o.Customer?.FullName ?? "?"} ({o.OrderDate:dd/MM/yyyy})" });

            ViewBag.OrderList = new SelectList(orders, "Id", "Display", selectedOrderId);
            ViewBag.ProductList = new SelectList(_context.Products.ToList(), "Id", "Name", selectedProductId);
        }
    }
}

