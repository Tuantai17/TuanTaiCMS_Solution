using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using CMS.Data;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var data = _context.Categories.ToList();
        return View(data);
    }
}
