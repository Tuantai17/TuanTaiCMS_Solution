/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 22/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ khÃ¡ch hÃ ng, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, thÃªm, sá»­a vÃ  xÃ³a khÃ¡ch hÃ ng.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // CustomerController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /Customer.
    [Authorize(Roles = "Admin,Staff")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch táº¥t cáº£ khÃ¡ch hÃ ng.
        public IActionResult Index()
        {
            var customers = _context.Customers
                .OrderBy(c => c.FullName)
                .ToList();

            return View(customers);
        }

        // Action Details hiá»ƒn thá»‹ chi tiáº¿t má»™t khÃ¡ch hÃ ng vÃ  danh sÃ¡ch Ä‘Æ¡n hÃ ng cá»§a há».
        public IActionResult Details(int id)
        {
            var customer = _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Action GET Create má»Ÿ form thÃªm khÃ¡ch hÃ ng má»›i.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create lÆ°u khÃ¡ch hÃ ng má»›i vÃ o database.
        [HttpPost]
        public IActionResult Create(Customer model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Customers.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "KhÃ¡ch hÃ ng Ä‘Ã£ Ä‘Æ°á»£c thÃªm thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit má»Ÿ form chá»‰nh sá»­a khÃ¡ch hÃ ng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Action POST Edit cáº­p nháº­t thÃ´ng tin khÃ¡ch hÃ ng vÃ o database.
        [HttpPost]
        public IActionResult Edit(Customer model)
        {
            var existingCustomer = _context.Customers.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Customer.Orders));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Customers.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "ThÃ´ng tin khÃ¡ch hÃ ng Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a khÃ¡ch hÃ ng theo id.
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "KhÃ¡ch hÃ ng Ä‘Ã£ Ä‘Æ°á»£c xÃ³a thÃ nh cÃ´ng!";
            }

            return RedirectToAction("Index");
        }
    }
}

