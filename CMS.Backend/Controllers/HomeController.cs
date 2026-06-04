/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 15/5/2026
MÃ´ táº£: Controller xá»­ lÃ½ trang chá»§, trang riÃªng tÆ° vÃ  trang lá»—i cá»§a website CMS.
*/

// NhÃ³m using: khai bÃ¡o cÃ¡c thÆ° viá»‡n vÃ  namespace cáº§n dÃ¹ng.
// CMS.Backend.Models chá»©a ErrorViewModel dÃ¹ng cho trang lá»—i.
// CMS.Data chá»©a ApplicationDbContext Ä‘á»ƒ truy váº¥n database.
// EntityFrameworkCore cung cáº¥p Include Ä‘á»ƒ join báº£ng Category khi láº¥y bÃ i viáº¿t.
using CMS.Backend.Models;
using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CMS.Backend.Controllers
{
    // HomeController quáº£n lÃ½ cÃ¡c trang chung nhÆ° trang chá»§, privacy vÃ  error.
    public class HomeController : Controller
    {
        // _context dÃ¹ng Ä‘á»ƒ truy váº¥n dá»¯ liá»‡u bÃ i viáº¿t tá»« database.
        // _logger dÃ¹ng Ä‘á»ƒ ghi log khi cáº§n theo dÃµi hoáº¡t Ä‘á»™ng hoáº·c lá»—i.
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // Constructor nháº­n cÃ¡c dependency do ASP.NET Core tá»± Ä‘á»™ng tiÃªm vÃ o.
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Action Index hiá»ƒn thá»‹ trang Dashboard.
        // Include láº¥y kÃ¨m thÃ´ng tin Category Ä‘á»ƒ View Ä‘á»c Ä‘Æ°á»£c item.Category.Name.
        // OrderByDescending sáº¯p xáº¿p bÃ i viáº¿t má»›i nháº¥t theo CreatedDate lÃªn Ä‘áº§u.
        // KhÃ´ng dÃ¹ng Take(3) Ä‘á»ƒ Dashboard hiá»ƒn thá»‹ Ä‘áº§y Ä‘á»§ dá»¯ liá»‡u bÃ i viáº¿t theo yÃªu cáº§u.
        // ToList() thá»±c thi truy váº¥n vÃ  chuyá»ƒn káº¿t quáº£ thÃ nh danh sÃ¡ch.
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Index()
        {
            // Láº¥y Ä‘Ãºng dá»¯ liá»‡u cÃ³ sáºµn trong database Ä‘á»ƒ hiá»ƒn thá»‹ á»Ÿ cÃ¡c Ã´ thá»‘ng kÃª Dashboard.
            ViewBag.TotalCategories = _context.Categories.Count();
            ViewBag.TotalPosts = _context.Posts.Count();
            ViewBag.TotalUsers = _context.Users.Count();

            var posts = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View(posts);
        }

        // Action Privacy tráº£ vá» trang thÃ´ng tin riÃªng tÆ°.
        public IActionResult Privacy()
        {
            return View();
        }

        // Cáº¥u hÃ¬nh khÃ´ng cache trang lá»—i Ä‘á»ƒ luÃ´n hiá»ƒn thá»‹ thÃ´ng tin lá»—i má»›i nháº¥t.
        // ErrorViewModel chá»©a RequestId giÃºp tra cá»©u lá»—i trong quÃ¡ trÃ¬nh debug.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

