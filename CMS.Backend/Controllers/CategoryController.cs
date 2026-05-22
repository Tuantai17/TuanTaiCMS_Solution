/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý danh mục bài viết, dùng để thêm, sửa, xóa và hiển thị danh mục trong hệ thống.
*/

using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;

// CategoryController nhận request liên quan đến đường dẫn /Category.
// Kế thừa Controller để có thể trả về View, Redirect, NotFound...
public class CategoryController : Controller
{
    // _context là biến dùng chung trong controller để thao tác với database.
    // readonly giúp đảm bảo biến chỉ được gán một lần trong constructor.
    private readonly ApplicationDbContext _context;

    // Constructor nhận ApplicationDbContext từ Dependency Injection.
    // ASP.NET Core tự tạo và truyền context vào khi controller được gọi.
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Action Index dùng để hiển thị danh sách tất cả danh mục.
    // ToList() thực thi truy vấn và lấy dữ liệu từ bảng Categories về bộ nhớ.
    // return View(data) truyền danh sách danh mục sang Views/Category/Index.cshtml.
    public IActionResult Index()
    {
        var data = _context.Categories.ToList();
        return View(data);
    }

    // Action GET Create dùng để mở form thêm danh mục mới.
    // Hàm này chỉ trả về giao diện nhập liệu, chưa lưu dữ liệu vào database.
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Action POST Create nhận dữ liệu người dùng gửi từ form thêm mới.
    // model chứa thông tin danh mục được bind từ các input asp-for trong View.
    // Add(model) đưa danh mục vào hàng chờ thêm mới của Entity Framework.
    // SaveChanges() ghi thay đổi thật sự xuống SQL Server.
    [HttpPost]
    public IActionResult Create(Category model)
    {
        _context.Categories.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // Action Delete nhận id danh mục cần xóa từ route.
    // Find(id) tìm danh mục theo khóa chính trong database.
    // Kiểm tra null để tránh lỗi khi id không tồn tại.
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

    // Action GET Edit dùng để mở form chỉnh sửa danh mục.
    // Nếu không tìm thấy dữ liệu theo id thì trả về lỗi 404 bằng NotFound().
    // Nếu tìm thấy thì truyền category sang View để đổ dữ liệu cũ lên form.
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

    // Action POST Edit nhận dữ liệu sau khi người dùng bấm nút cập nhật.
    // Update(model) đánh dấu bản ghi cần sửa trong Entity Framework.
    // SaveChanges() lưu thay đổi xuống database.
    [HttpPost]
    public IActionResult Edit(Category model)
    {
        _context.Categories.Update(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}
