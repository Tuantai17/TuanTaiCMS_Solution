using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Menus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            return await _context.Menus
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToListAsync();
        }

        // GET: api/Menus/Hierarchy
        [HttpGet("hierarchy")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenuHierarchy()
        {
            var allMenus = await _context.Menus
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToListAsync();

            var rootMenus = allMenus.Where(m => m.ParentId == null).ToList();
            return rootMenus;
        }
    }
}
