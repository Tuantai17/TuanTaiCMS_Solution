using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using CMS.Backend.Models;
using CMS.Backend.Services;
using CMS.Backend.Services.Favorite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Buổi 5 - Bước 1: Khai báo dịch vụ xác thực Cookie
// LoginPath: Khi chưa đăng nhập sẽ bị điều hướng về trang này
// AccessDeniedPath: Khi đăng nhập nhưng không đủ quyền sẽ bị điều hướng về trang này
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Dang ky dich vu gui email moi (IEmailService) va giu lai EmailHelper cu
builder.Services.AddTransient<EmailHelper>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IProductFavoriteService, ProductFavoriteService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderIssueService, OrderIssueService>();

// 1. Đăng ký các dịch vụ bổ trợ khám phá Endpoint phục vụ Web API
builder.Services.AddEndpointsApiExplorer();
// 2. Kích hoạt bộ sinh tài liệu API tự động Swagger UI
builder.Services.AddSwaggerGen();

// 3. Cấu hình chính sách chia sẻ tài nguyên CORS cho phép ReactJS kết nối rút dữ liệu
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()   // Cho phép tất cả các nguồn gửi yêu cầu đến
              .AllowAnyMethod()   // Cho phép tất cả phương thức HTTP (GET, POST, PUT, DELETE...)
              .AllowAnyHeader();  // Cho phép tất cả các thuộc tính tiêu đề (Header)
    });
});

var app = builder.Build();

// ===== TỰ ĐỘNG MÃ HÓA MẬT KHẨU PLAIN TEXT TRONG DATABASE =====
// Chạy một lần khi ứng dụng khởi động.
// Kiểm tra mật khẩu chưa hash (không bắt đầu bằng "$2") và hash bằng BCrypt.
// An toàn khi chạy nhiều lần vì chỉ xử lý mật khẩu chưa hash.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Tự động seed dữ liệu mẫu nếu database trống
    DbInitializer.Initialize(context);

    // --- Xóa các tài khoản Users có Role không phải Admin/Staff ---
    // Hệ thống quản trị chỉ cần Admin và Staff, các Role khác (User, Editor) là dữ liệu dư thừa.
    var invalidUsers = context.Users
        .Where(u => u.Role != "Admin" && u.Role != "Staff")
        .ToList();

    if (invalidUsers.Any())
    {
        context.Users.RemoveRange(invalidUsers);
        Console.WriteLine($"==> Đã xóa {invalidUsers.Count} tài khoản không hợp lệ (Role khác Admin/Staff).");
    }

    // Hash mật khẩu bảng Users (Admin/Staff)
    var users = context.Users.ToList();
    bool hasChanges = invalidUsers.Any();
    foreach (var user in users)
    {
        if (!string.IsNullOrEmpty(user.PasswordHash) && !user.PasswordHash.StartsWith("$2"))
        {
            user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);
            hasChanges = true;
        }
    }

    // Hash mật khẩu bảng Customers (Khách hàng)
    var customers = context.Customers.ToList();
    foreach (var customer in customers)
    {
        if (!string.IsNullOrEmpty(customer.Password) && !customer.Password.StartsWith("$2"))
        {
            customer.Password = PasswordHelper.HashPassword(customer.Password);
            hasChanges = true;
        }
    }

    if (hasChanges)
    {
        context.SaveChanges();
        Console.WriteLine("==> Đã mã hóa BCrypt toàn bộ mật khẩu plain text trong database.");
    }
}
// ================================================================

// Kích hoạt bộ sinh giao diện thử nghiệm API Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Cấu hình đường dẫn file đặc tả JSON của API
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuanTaiCMS Web API v1");
    // Đường dẫn truy cập giao diện kiểm thử mặc định sẽ là http://localhost:xxxx/swagger
    c.RoutePrefix = "swagger";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cấu hình StaticFiles hỗ trợ thêm các MIME type cho ảnh hiện đại (.avif, .webp)
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

// [VỊ TRÍ ĐẶT CORS]: Kích hoạt CORS ngay sau UseRouting và trước Authentication để mở cổng cho ReactJS kết nối
app.UseCors("AllowAll");

// Buổi 5 - Thứ tự quan trọng: Authentication phải đứng TRƯỚC Authorization
// BƯỚC A: Xác nhận "Anh là ai?" (Kiểm tra thẻ bài / Cookie)
app.UseAuthentication();
// BƯỚC B: Xác nhận "Anh được làm gì?" (Kiểm tra quyền)
app.UseAuthorization();

// Phân luồng A: Ánh xạ và kích hoạt các Endpoint API của các API Controller
app.MapControllers();

// Phân luồng B: Giữ lại bản đồ định tuyến mặc định cho trang giao diện Web MVC cũ (.cshtml)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

