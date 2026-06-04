using CMS.Data;
using CMS.Data.Entities;
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
app.UseStaticFiles();

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
