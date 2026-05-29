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

// Khối Seed dữ liệu đồ chơi MyKingdom thông minh (Chỉ thực hiện xóa cũ và cập nhật mới MỘT LẦN DUY NHẤT)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Kiểm tra xem database đã được chuyển đổi sang dữ liệu đồ chơi chưa
        if (!context.CategoriesProducts.Any(c => c.Name == "Đồ chơi lắp ghép"))
        {
            // 1. Xóa sạch dữ liệu sản phẩm cũ (Các sản phẩm điện tử, công nghệ)
            if (context.Products.Any())
            {
                context.Products.RemoveRange(context.Products);
                context.SaveChanges();
            }
            
            // 2. Xóa sạch dữ liệu danh mục sản phẩm cũ
            if (context.CategoriesProducts.Any())
            {
                context.CategoriesProducts.RemoveRange(context.CategoriesProducts);
                context.SaveChanges();
            }

            // 3. Khởi tạo 4 danh mục đồ chơi MyKingdom chuẩn
            var catLapGhep = new CategoryProduct { Name = "Đồ chơi lắp ghép", Description = "Các bộ đồ chơi lắp ghép Lego phát triển trí thông minh." };
            var catSangTao = new CategoryProduct { Name = "Đồ chơi sáng tạo", Description = "Con quay Beyblade, Yoyo rèn luyện tính khéo léo cho trẻ." };
            var catThoiTrang = new CategoryProduct { Name = "Đồ thời trang", Description = "Ba lô học đường Clever Hippo chống gù cao cấp." };
            var catDongVat = new CategoryProduct { Name = "Thế giới động vật", Description = "Mô hình động vật Schleich cao cấp của Đức." };

            context.CategoriesProducts.AddRange(catLapGhep, catSangTao, catThoiTrang, catDongVat);
            context.SaveChanges(); // Lưu để phát sinh ID khóa chính tự động

            // 4. Khởi tạo 12 sản phẩm đồ chơi tương ứng với các danh mục
            var products = new List<Product>
            {
                // Nhóm 1: Đồ chơi lắp ghép (3 sản phẩm LEGO)
                new Product
                {
                    Name = "LEGO Classic 10696 - Thùng Gạch Trung Sáng Tạo Cao Cấp",
                    Price = 799000,
                    StockQuantity = 50,
                    Description = "Bộ gạch lắp ráp Lego Classic 484 chi tiết gạch nhiều màu sắc khơi dậy nguồn cảm hứng sáng tạo vô tận cho bé.",
                    ImageUrl = "https://images.unsplash.com/photo-1587654780291-39c9404d746b?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catLapGhep.Id
                },
                new Product
                {
                    Name = "LEGO City 60312 - Xe Cảnh Sát Đuổi Bắt Tốc Độ Cao",
                    Price = 279000,
                    StockQuantity = 30,
                    Description = "Bộ mô hình đồ chơi xe cảnh sát Lego City sinh động dành cho các bé yêu thích chủ đề phiêu lưu và hành động.",
                    ImageUrl = "https://images.unsplash.com/photo-1560169897-fc0cdbdfa4d5?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catLapGhep.Id
                },
                new Product
                {
                    Name = "LEGO Creator 31058 - Khủng Long Gầm Vang 3 Trong 1",
                    Price = 399000,
                    StockQuantity = 20,
                    Description = "Lắp ráp và chơi cùng khủng long bạo chúa T-Rex dũng mãnh, có thể hoán đổi thành khủng long ba sừng Triceratops hoặc thằn lằn bay Pterodactyl.",
                    ImageUrl = "https://images.unsplash.com/photo-1558060370-d644479cb6f7?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catLapGhep.Id
                },

                // Nhóm 2: Đồ chơi sáng tạo (3 sản phẩm Yoyo, Beyblade)
                new Product
                {
                    Name = "Con Quay B-180 Booster Dynamite Belial.Nx.Vn-2 BEYBLADE 6173670",
                    Price = 189500,
                    StockQuantity = 45,
                    Description = "Con quay Beyblade Burst DB thế hệ mới với trục xoay kim loại và chốt trợ lực tấn công siêu tốc độ cực đỉnh.",
                    ImageUrl = "https://images.unsplash.com/photo-1596461404969-9ae70f2830c1?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catSangTao.Id
                },
                new Product
                {
                    Name = "Con Quay B-192 Booster Greatest Raphael.Ov.HXt+ BEYBLADE 6173779",
                    Price = 229500,
                    StockQuantity = 25,
                    Description = "Bộ con quay thế hệ cải tiến Greatest Raphael tối ưu hóa khả năng phòng thủ và giữ thăng bằng xuất sắc trên đấu trường.",
                    ImageUrl = "https://images.unsplash.com/photo-1515488042361-404e9250afef?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catSangTao.Id
                },
                new Product
                {
                    Name = "Yoyo Chiến Binh Huyền Thoại YOYO 22 EU677118R Cực Đỉnh",
                    Price = 47800,
                    StockQuantity = 100,
                    Description = "Đồ chơi Yoyo kim loại bền bỉ với ổ bi chịu lực tốc độ cao, hỗ trợ các bé thực hiện các kỹ thuật biểu diễn từ cơ bản đến nâng cao cực đỉnh.",
                    ImageUrl = "https://images.unsplash.com/photo-1531256379416-9f000e90aacc?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catSangTao.Id
                },

                // Nhóm 3: Đồ thời trang (3 sản phẩm ba lô Clever Hippo)
                new Product
                {
                    Name = "Ba Lô Chống Gù Clever Hippo Easy Go Dino - Khủng Long Xanh",
                    Price = 499000,
                    StockQuantity = 15,
                    Description = "Thiết kế ba lô học sinh chống gù siêu nhẹ độc quyền với chất liệu vải chống thấm nước, họa tiết khủng long Dino cực ngầu.",
                    ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catThoiTrang.Id
                },
                new Product
                {
                    Name = "Ba Lô Học Đường Siêu Nhẹ Clever Hippo Fancy Unicorn Hồng",
                    Price = 599000,
                    StockQuantity = 8,
                    Description = "Ba lô bé gái họa tiết kỳ lân dễ thương, có đai ngực trợ lực giúp phân phối lực đều, bảo vệ xương cột sống cho trẻ em tiểu học.",
                    ImageUrl = "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catThoiTrang.Id
                },
                new Product
                {
                    Name = "Bình Nước Thể Thao Clever Hippo Active Cách Nhiệt Tốt",
                    Price = 149000,
                    StockQuantity = 40,
                    Description = "Bình nước cá nhân bằng nhựa Tritan cao cấp không chứa BPA, khả năng chống tràn và chịu nhiệt tuyệt vời cho bé hoạt động thể thao.",
                    ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catThoiTrang.Id
                },

                // Nhóm 4: Thế giới động vật (3 sản phẩm mô hình Schleich Đức)
                new Product
                {
                    Name = "Mô Hình Khủng Long Bạo Chúa T-Rex Schleich Độc Quyền Đức",
                    Price = 299000,
                    StockQuantity = 12,
                    Description = "Mô hình đồ chơi khủng long bạo chúa T-Rex chế tác vô cùng tinh xảo từ thương hiệu Schleich Đức, các khớp hàm cử động linh hoạt.",
                    ImageUrl = "https://images.unsplash.com/photo-1525869916826-972885c91c1e?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catDongVat.Id
                },
                new Product
                {
                    Name = "Mô Hình Voi Châu Á Trưởng Thành Schleich Sống Động",
                    Price = 199000,
                    StockQuantity = 20,
                    Description = "Mô hình voi châu Á đúc đặc bền vững, sơn tay chi tiết mô phỏng chính xác cấu trúc da voi và ngà voi giống hệt ngoài đời thực.",
                    ImageUrl = "https://images.unsplash.com/photo-1581888227599-779811939961?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catDongVat.Id
                },
                new Product
                {
                    Name = "Mô Hình Sư Tử Đực Dũng Mãnh Schleich Chi Tiết Sắc Nét",
                    Price = 149000,
                    StockQuantity = 18,
                    Description = "Đồ chơi mô hình sư tử đực dũng mãnh Schleich Đức, chi tiết sắc nét giáo dục nhận biết sinh học tự nhiên cực tốt cho bé.",
                    ImageUrl = "https://images.unsplash.com/photo-1614027164847-1b2809eb7b9b?w=500&auto=format&fit=crop&q=80",
                    CategoryProductId = catDongVat.Id
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
            Console.WriteLine(">>> Đã dọn dẹp dữ liệu cũ & Seed thành công 12 sản phẩm đồ chơi MyKingdom mới!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(">>> Lỗi trong quá trình Seed dữ liệu đồ chơi: " + ex.Message);
    }
}

app.Run();
