using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách menu phân cấp dạng cây
        public IActionResult Index()
        {
            var sortedMenus = GetSortedMenus();
            return View(sortedMenus);
        }

        // Action GET Create mở form thêm menu mới
        [HttpGet]
        public IActionResult Create()
        {
            LoadParentMenuList();
            return View();
        }

        // Action POST Create lưu menu mới
        [HttpPost]
        public IActionResult Create(Menu model)
        {
            ModelState.Remove(nameof(Menu.Parent));
            ModelState.Remove(nameof(Menu.Children));

            if (!ModelState.IsValid)
            {
                LoadParentMenuList(null, model.ParentId);
                return View(model);
            }

            _context.Menus.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Menu đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form sửa menu
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return NotFound();
            }

            LoadParentMenuList(id, menu.ParentId);
            return View(menu);
        }

        // Action POST Edit cập nhật menu
        [HttpPost]
        public IActionResult Edit(Menu model)
        {
            ModelState.Remove(nameof(Menu.Parent));
            ModelState.Remove(nameof(Menu.Children));

            if (!ModelState.IsValid)
            {
                LoadParentMenuList(model.Id, model.ParentId);
                return View(model);
            }

            _context.Menus.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Menu đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa menu
        public IActionResult Delete(int id)
        {
            var menu = _context.Menus
                .Include(m => m.Children)
                .FirstOrDefault(m => m.Id == id);

            if (menu != null)
            {
                // Chuyển danh mục con về cha của danh mục bị xóa
                foreach (var child in menu.Children)
                {
                    child.ParentId = menu.ParentId;
                }

                _context.Menus.Remove(menu);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Menu đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Action AJAX: Bật/tắt trạng thái hiển thị của Menu
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy menu." });
            }

            menu.IsActive = !menu.IsActive;
            _context.SaveChanges();

            return Json(new { success = true, isActive = menu.IsActive });
        }

        // Helper nạp dropdown menu cha (loại trừ chính nó tránh đệ quy vòng)
        private void LoadParentMenuList(int? currentMenuId = null, int? selectedParentId = null)
        {
            var query = _context.Menus.AsQueryable();
            if (currentMenuId.HasValue)
            {
                query = query.Where(m => m.Id != currentMenuId.Value);
            }

            var parentList = query.OrderBy(m => m.Title).ToList();

            ViewBag.ParentId = new SelectList(
                parentList,
                "Id",
                "Title",
                selectedParentId
            );
        }

        // Đệ quy lấy danh sách menu dạng cây
        private List<Menu> GetSortedMenus()
        {
            var allMenus = _context.Menus.ToList();
            var sortedList = new List<Menu>();

            var rootMenus = allMenus
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Order)
                .ToList();

            foreach (var root in rootMenus)
            {
                AddChildrenToList(root, allMenus, sortedList, 0);
            }

            return sortedList;
        }

        private void AddChildrenToList(Menu current, List<Menu> all, List<Menu> sorted, int depth)
        {
            current.Depth = depth;
            sorted.Add(current);

            var children = all
                .Where(m => m.ParentId == current.Id)
                .OrderBy(m => m.Order)
                .ToList();

            foreach (var child in children)
            {
                AddChildrenToList(child, all, sorted, depth + 1);
            }
        }
    }
}
