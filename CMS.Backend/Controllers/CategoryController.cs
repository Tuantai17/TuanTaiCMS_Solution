/*
Sinh ViÃªn: Nguyá»…n Tuáº¥n TÃ i
MÃ£ Sinh ViÃªn: 2123110166
Lá»›p: CCQ2311E
NgÃ y Táº¡o: 15/5/2026
MÃ´ táº£: Controller quáº£n lÃ½ danh má»¥c bÃ i viáº¿t, dÃ¹ng Ä‘á»ƒ thÃªm, sá»­a, xÃ³a vÃ  hiá»ƒn thá»‹ danh má»¥c trong há»‡ thá»‘ng.
*/

using Microsoft.AspNetCore.Authorization; // Buá»•i 5: Namespace cáº§n thiáº¿t Ä‘á»ƒ dÃ¹ng [Authorize]
using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;

// CategoryController nháº­n request liÃªn quan Ä‘áº¿n Ä‘Æ°á»ng dáº«n /Category.
// Káº¿ thá»«a Controller Ä‘á»ƒ cÃ³ thá»ƒ tráº£ vá» View, Redirect, NotFound...
// Buá»•i 5: [Authorize] báº¯t buá»™c pháº£i Ä‘Äƒng nháº­p má»›i Ä‘Æ°á»£c truy cáº­p cÃ¡c action bÃªn dÆ°á»›i.
[Authorize(Roles = "Admin,Staff")]
public class CategoryController : Controller
{
    // _context lÃ  biáº¿n dÃ¹ng chung trong controller Ä‘á»ƒ thao tÃ¡c vá»›i database.
    // readonly giÃºp Ä‘áº£m báº£o biáº¿n chá»‰ Ä‘Æ°á»£c gÃ¡n má»™t láº§n trong constructor.
    private readonly ApplicationDbContext _context;

    // Constructor nháº­n ApplicationDbContext tá»« Dependency Injection.
    // ASP.NET Core tá»± táº¡o vÃ  truyá»n context vÃ o khi controller Ä‘Æ°á»£c gá»i.
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Action Index dÃ¹ng Ä‘á»ƒ hiá»ƒn thá»‹ danh sÃ¡ch táº¥t cáº£ danh má»¥c.
    // ToList() thá»±c thi truy váº¥n vÃ  láº¥y dá»¯ liá»‡u tá»« báº£ng Categories vá» bá»™ nhá»›.
    // return View(data) truyá»n danh sÃ¡ch danh má»¥c sang Views/Category/Index.cshtml.
    public IActionResult Index()
    {
        var data = _context.Categories.ToList();
        return View(data);
    }

    // Action GET Create dÃ¹ng Ä‘á»ƒ má»Ÿ form thÃªm danh má»¥c má»›i.
    // HÃ m nÃ y chá»‰ tráº£ vá» giao diá»‡n nháº­p liá»‡u, chÆ°a lÆ°u dá»¯ liá»‡u vÃ o database.
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Action POST Create nháº­n dá»¯ liá»‡u ngÆ°á»i dÃ¹ng gá»­i tá»« form thÃªm má»›i.
    // model chá»©a thÃ´ng tin danh má»¥c Ä‘Æ°á»£c bind tá»« cÃ¡c input asp-for trong View.
    // Add(model) Ä‘Æ°a danh má»¥c vÃ o hÃ ng chá» thÃªm má»›i cá»§a Entity Framework.
    // SaveChanges() ghi thay Ä‘á»•i tháº­t sá»± xuá»‘ng SQL Server.
    [HttpPost]
    public IActionResult Create(Category model)
    {
        _context.Categories.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // Action Delete nháº­n id danh má»¥c cáº§n xÃ³a tá»« route.
    // Find(id) tÃ¬m danh má»¥c theo khÃ³a chÃ­nh trong database.
    // Kiá»ƒm tra null Ä‘á»ƒ trÃ¡nh lá»—i khi id khÃ´ng tá»“n táº¡i.
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.Find(id);

        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // Action GET Edit dÃ¹ng Ä‘á»ƒ má»Ÿ form chá»‰nh sá»­a danh má»¥c.
    // Náº¿u khÃ´ng tÃ¬m tháº¥y dá»¯ liá»‡u theo id thÃ¬ tráº£ vá» lá»—i 404 báº±ng NotFound().
    // Náº¿u tÃ¬m tháº¥y thÃ¬ truyá»n category sang View Ä‘á»ƒ Ä‘á»• dá»¯ liá»‡u cÅ© lÃªn form.
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // Action POST Edit nháº­n dá»¯ liá»‡u sau khi ngÆ°á»i dÃ¹ng báº¥m nÃºt cáº­p nháº­t.
    // Update(model) Ä‘Ã¡nh dáº¥u báº£n ghi cáº§n sá»­a trong Entity Framework.
    // SaveChanges() lÆ°u thay Ä‘á»•i xuá»‘ng database.
    [HttpPost]
    public IActionResult Edit(Category model)
    {
        _context.Categories.Update(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}

