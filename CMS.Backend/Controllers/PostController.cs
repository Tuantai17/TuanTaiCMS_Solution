/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 15/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ bÃ i viáº¿t, gá»“m hiá»ƒn thá»‹ danh sÃ¡ch, xem chi tiáº¿t, thÃªm, sá»­a vÃ  xÃ³a bÃ i viáº¿t.
*/

// NhÃ³m thÆ° viá»‡n phá»¥c vá»¥ truy váº¥n database, xá»­ lÃ½ MVC, upload file vÃ  táº¡o dropdown danh má»¥c.
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization; // Buá»•i 5: Namespace cáº§n thiáº¿t Ä‘á»ƒ dÃ¹ng [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // PostController xá»­ lÃ½ cÃ¡c request báº¯t Ä‘áº§u báº±ng /Post.
    // Buá»•i 5: [Authorize(Roles = "Admin,Staff")] báº¯t buá»™c pháº£i Ä‘Äƒng nháº­p vá»›i quyá»n Admin má»›i Ä‘Æ°á»£c vÃ o cÃ¡c action bÃªn dÆ°á»›i.
    [Authorize(Roles = "Admin,Staff")]
    public class PostController : Controller
    {
        // DbContext dÃ¹ng Ä‘á»ƒ truy váº¥n báº£ng Posts vÃ  Categories.
        private readonly ApplicationDbContext _context;

        // Constructor nháº­n context tá»« há»‡ thá»‘ng Dependency Injection.
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiá»ƒn thá»‹ danh sÃ¡ch bÃ i viáº¿t.
        // Tham sá»‘ id lÃ  mÃ£ danh má»¥c, cÃ³ thá»ƒ null náº¿u ngÆ°á»i dÃ¹ng khÃ´ng lá»c danh má»¥c.
        // Include láº¥y kÃ¨m Category Ä‘á»ƒ hiá»ƒn thá»‹ tÃªn danh má»¥c ngoÃ i View.
        // OrderByDescending Ä‘Æ°a bÃ i viáº¿t má»›i nháº¥t lÃªn Ä‘áº§u danh sÃ¡ch.
        public IActionResult Index(int? id)
        {
            var posts = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            // Náº¿u id cÃ³ giÃ¡ trá»‹, chá»‰ giá»¯ láº¡i bÃ i viáº¿t thuá»™c danh má»¥c Ä‘Ã³.
            // Where lá»c danh sÃ¡ch theo CategoryId trÃ¹ng vá»›i id Ä‘Æ°á»£c truyá»n vÃ o URL.
            if (id != null)
            {
                posts = posts
                    .Where(p => p.CategoryId == id)
                    .ToList();
            }

            return View(posts);
        }

        // Action Details hiá»ƒn thá»‹ chi tiáº¿t má»™t bÃ i viáº¿t theo id.
        // FirstOrDefault tráº£ vá» bÃ i viáº¿t Ä‘áº§u tiÃªn khá»›p Ä‘iá»u kiá»‡n hoáº·c null náº¿u khÃ´ng cÃ³.
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            // Náº¿u khÃ´ng tÃ¬m tháº¥y bÃ i viáº¿t thÃ¬ tráº£ vá» trang lá»—i 404.
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // Action GET Create má»Ÿ form thÃªm bÃ i viáº¿t.
        // ViewBag.CategoryList chá»©a danh sÃ¡ch danh má»¥c Ä‘á»ƒ hiá»ƒn thá»‹ dropdown trong View.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCategoryList();
            return View();
        }

        // Action POST Create nháº­n dá»¯ liá»‡u bÃ i viáº¿t tá»« form vÃ  xá»­ lÃ½ upload áº£nh náº¿u cÃ³.
        // áº¢nh Ä‘Æ°á»£c lÆ°u trong wwwroot/uploads, database chá»‰ lÆ°u Ä‘Æ°á»ng dáº«n tÆ°Æ¡ng Ä‘á»‘i.
        [HttpPost]
        public IActionResult Create(Post model, IFormFile? uploadImage)
        {
            // Bá» qua validation cá»§a navigation property vÃ¬ Category khÃ´ng Ä‘Æ°á»£c gá»­i tá»« form,
            // chá»‰ cáº§n CategoryId lÃ  Ä‘á»§ Ä‘á»ƒ EF Core táº¡o liÃªn káº¿t.
            ModelState.Remove(nameof(Post.Category));

            if (!ModelState.IsValid)
            {
                LoadCategoryList(model.CategoryId);
                return View(model);
            }

            model.CreatedDate = model.CreatedDate == default ? DateTime.Now : model.CreatedDate;
            model.ImageUrl = SaveUploadImage(uploadImage) ?? model.ImageUrl;

            _context.Posts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "BÃ i viáº¿t Ä‘Ã£ Ä‘Æ°á»£c lÆ°u vÃ  Ä‘Äƒng thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action GET Edit dÃ¹ng Ä‘á»ƒ má»Ÿ form chá»‰nh sá»­a bÃ i viáº¿t.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }

            LoadCategoryList(post.CategoryId);
            return View(post);
        }

        // Action POST Edit cáº­p nháº­t bÃ i viáº¿t.
        // Náº¿u khÃ´ng upload áº£nh má»›i thÃ¬ giá»¯ nguyÃªn ImageUrl cÅ©.
        [HttpPost]
        public IActionResult Edit(Post model, IFormFile? uploadImage)
        {
            var existingPost = _context.Posts.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);

            if (existingPost == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Post.Category));

            if (!ModelState.IsValid)
            {
                LoadCategoryList(model.CategoryId);
                model.ImageUrl = existingPost.ImageUrl;
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? existingPost.ImageUrl;
            model.CreatedDate = model.CreatedDate == default ? existingPost.CreatedDate : model.CreatedDate;

            _context.Posts.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "BÃ i viáº¿t Ä‘Ã£ Ä‘Æ°á»£c cáº­p nháº­t thÃ nh cÃ´ng!";
            return RedirectToAction("Index");
        }

        // Action Delete xÃ³a bÃ i viáº¿t theo id nháº­n Ä‘Æ°á»£c tá»« giao diá»‡n.
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);

            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // HÃ m dÃ¹ng chung Ä‘á»ƒ náº¡p dropdown danh má»¥c cho form Create/Edit.
        private void LoadCategoryList(int? selectedCategoryId = null)
        {
            ViewBag.CategoryList = new SelectList(
                _context.Categories.ToList(),
                "Id",
                "Name",
                selectedCategoryId
            );
        }

        // HÃ m lÆ°u áº£nh upload vÃ o wwwroot/uploads vÃ  tráº£ vá» Ä‘Æ°á»ng dáº«n tÆ°Æ¡ng Ä‘á»‘i.
        // Náº¿u ngÆ°á»i dÃ¹ng khÃ´ng chá»n file thÃ¬ tráº£ vá» null Ä‘á»ƒ controller giá»¯ dá»¯ liá»‡u cÅ©.
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

