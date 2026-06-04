/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 22/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ loáº¡i sáº£n pháº©m, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, thÃªm, sá»­a vÃ  xÃ³a loáº¡i sáº£n pháº©m.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // CategoryProductController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /CategoryProduct.
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch loáº¡i sáº£n pháº©m kÃ¨m sá»‘ lÆ°á»£ng sáº£n pháº©m thuá»™c má»—i loáº¡i.
        public IActionResult Index()
        {
            var categoriesProducts = _context.CategoriesProducts
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToList();

            return View(categoriesProducts);
        }

        // Action GET Create má»Ÿ form thÃªm loáº¡i sáº£n pháº©m má»›i.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create lÆ°u loáº¡i sáº£n pháº©m má»›i vÃ o database.
        [HttpPost]
        public IActionResult Create(CategoryProduct model)
        {
            ModelState.Remove(nameof(CategoryProduct.Products));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "TÃªn loáº¡i sáº£n pháº©m Ä‘Ã£ tá»“n táº¡i.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.CategoriesProducts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Loáº¡i sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c thÃªm thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit má»Ÿ form chá»‰nh sá»­a loáº¡i sáº£n pháº©m theo id.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.CategoriesProducts.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Action POST Edit cáº­p nháº­t loáº¡i sáº£n pháº©m vÃ o database.
        [HttpPost]
        public IActionResult Edit(CategoryProduct model)
        {
            var existingCategory = _context.CategoriesProducts.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CategoryProduct.Products));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Id != model.Id && c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "TÃªn loáº¡i sáº£n pháº©m Ä‘Ã£ tá»“n táº¡i.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.CategoriesProducts.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Loáº¡i sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a loáº¡i sáº£n pháº©m theo id.
        public IActionResult Delete(int id)
        {
            var category = _context.CategoriesProducts
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (category != null)
            {
                if (category.Products != null && category.Products.Any())
                {
                    TempData["SuccessMessage"] = "KhÃ´ng thá»ƒ xÃ³a loáº¡i sáº£n pháº©m Ä‘ang cÃ³ sáº£n pháº©m. Vui lÃ²ng chuyá»ƒn/xÃ³a sáº£n pháº©m trÆ°á»›c.";
                    return RedirectToAction("Index");
                }

                _context.CategoriesProducts.Remove(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Loáº¡i sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c xÃ³a thÃ nh cÃ´ng!";
            }

            return RedirectToAction("Index");
        }
    }
}

