/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 22/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ sáº£n pháº©m, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, thÃªm, sá»­a, xÃ³a vÃ  upload áº£nh sáº£n pháº©m.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // ProductController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /Product.
    [Authorize(Roles = "Admin,Staff")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch táº¥t cáº£ sáº£n pháº©m kÃ¨m loáº¡i sáº£n pháº©m.
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.CategoryProduct)
                .OrderBy(p => p.Name)
                .ToList();

            return View(products);
        }

        // Action Details hiá»ƒn thá»‹ chi tiáº¿t má»™t sáº£n pháº©m theo id.
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.CategoryProduct)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Action GET Create má»Ÿ form thÃªm sáº£n pháº©m má»›i.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCategoryProductList();
            return View();
        }

        // Action POST Create lÆ°u sáº£n pháº©m má»›i, xá»­ lÃ½ upload áº£nh náº¿u cÃ³.
        [HttpPost]
        public IActionResult Create(Product model, IFormFile? uploadImage)
        {
            ModelState.Remove(nameof(Product.CategoryProduct));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? model.ImageUrl;

            _context.Products.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c thÃªm thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit má»Ÿ form chá»‰nh sá»­a sáº£n pháº©m.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            LoadCategoryProductList(product.CategoryProductId);
            return View(product);
        }

        // Action POST Edit cáº­p nháº­t sáº£n pháº©m, giá»¯ nguyÃªn áº£nh cÅ© náº¿u khÃ´ng upload áº£nh má»›i.
        [HttpPost]
        public IActionResult Edit(Product model, IFormFile? uploadImage)
        {
            var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Product.CategoryProduct));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                model.ImageUrl = existingProduct.ImageUrl;
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? existingProduct.ImageUrl;

            _context.Products.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a sáº£n pháº©m theo id.
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Sáº£n pháº©m Ä‘Ã£ Ä‘Æ°á»£c xÃ³a thÃ nh cÃ´ng!";
            }

            return RedirectToAction("Index");
        }

        // HÃ m dÃ¹ng chung Ä‘á»ƒ náº¡p dropdown loáº¡i sáº£n pháº©m cho form Create/Edit.
        private void LoadCategoryProductList(int? selectedId = null)
        {
            ViewBag.CategoryProductList = new SelectList(
                _context.CategoriesProducts.ToList(),
                "Id",
                "Name",
                selectedId
            );
        }

        // HÃ m lÆ°u áº£nh upload vÃ o wwwroot/uploads vÃ  tráº£ vá» Ä‘Æ°á»ng dáº«n tÆ°Æ¡ng Ä‘á»‘i.
        private string? SaveUploadImage(IFormFile? uploadImage)
        {
            if (uploadImage == null || uploadImage.Length == 0)
            {
                return null;
            }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(uploadImage.FileName);
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadImage.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }
    }
}

