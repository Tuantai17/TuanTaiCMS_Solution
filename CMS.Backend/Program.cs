using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

var app = builder.Build();

// Seed dữ liệu ảo cho 5 thực thể mới nếu chưa có dữ liệu.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Seed CategoriesProducts (Loại sản phẩm)
    if (!db.CategoriesProducts.Any())
    {
        db.CategoriesProducts.AddRange(
            new CategoryProduct { Name = "Điện thoại", Description = "Điện thoại thông minh các loại thương hiệu nổi tiếng" },
            new CategoryProduct { Name = "Laptop", Description = "Máy tính xách tay phục vụ học tập và làm việc" },
            new CategoryProduct { Name = "Phụ kiện", Description = "Tai nghe, sạc, bao da, ốp lưng và các phụ kiện điện tử" },
            new CategoryProduct { Name = "Màn hình", Description = "Màn hình máy tính các kích cỡ từ 22 đến 34 inch" },
            new CategoryProduct { Name = "Bàn phím & Chuột", Description = "Thiết bị đầu vào cơ học và không dây" }
        );
        db.SaveChanges();
    }

    // Seed Products (Sản phẩm)
    // Seed / cập nhật Products (Sản phẩm kèm ảnh thực)
    var catDienThoai = db.CategoriesProducts.FirstOrDefault(c => c.Name == "Điện thoại");
    var catLaptop    = db.CategoriesProducts.FirstOrDefault(c => c.Name == "Laptop");
    var catPhuKien   = db.CategoriesProducts.FirstOrDefault(c => c.Name == "Phụ kiện");
    var catManHinh   = db.CategoriesProducts.FirstOrDefault(c => c.Name == "Màn hình");
    var catBanPhim   = db.CategoriesProducts.FirstOrDefault(c => c.Name == "Bàn phím & Chuột");

    // Danh sách sản phẩm mẫu với URL ảnh từ Unsplash / ảnh sản phẩm thực
    var seedProducts = new List<(string Name, string Desc, decimal Price, int Stock, int CatId, string ImageUrl)>
    {
        // --- Điện thoại ---
        ("iPhone 15 Pro Max",
         "iPhone 15 Pro Max 256GB, chip A17 Pro, camera 48MP, màn hình 6.7 inch Super Retina XDR",
         32990000, 25, catDienThoai!.Id,
         "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=400&q=80"),

        ("Samsung Galaxy S24 Ultra",
         "Galaxy S24 Ultra 512GB, Snapdragon 8 Gen 3, bút S Pen, camera 200MP",
         28490000, 18, catDienThoai.Id,
         "https://images.unsplash.com/photo-1706542762862-37a2c6cf9c3e?w=400&q=80"),

        ("Xiaomi 14 Ultra",
         "Xiaomi 14 Ultra 512GB, camera Leica, Snapdragon 8 Gen 3, sạc nhanh 90W",
         19990000, 12, catDienThoai.Id,
         "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400&q=80"),

        ("OPPO Find X7 Ultra",
         "OPPO Find X7 Ultra 512GB, camera Hasselblad, Dimensity 9300, sạc không dây 80W",
         22990000, 8, catDienThoai.Id,
         "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400&q=80"),

        ("Google Pixel 8 Pro",
         "Google Pixel 8 Pro 256GB, chip Tensor G3, camera 50MP, AI tích hợp Android 14",
         20990000, 10, catDienThoai.Id,
         "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=400&q=80"),

        // --- Laptop ---
        ("MacBook Pro M3 14\"",
         "MacBook Pro 14 inch chip M3 Pro, 18GB RAM, 512GB SSD, màn hình Liquid Retina XDR",
         45990000, 8, catLaptop!.Id,
         "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&q=80"),

        ("Dell XPS 15 2024",
         "Dell XPS 15 Intel Core Ultra 7, 32GB RAM, 1TB SSD, màn hình OLED 3.5K cảm ứng",
         38500000, 6, catLaptop.Id,
         "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=400&q=80"),

        ("ASUS ROG Zephyrus G14",
         "ASUS ROG G14, AMD Ryzen 9 8945HS, RTX 4070 8GB, 16GB RAM, 1TB SSD, 165Hz",
         42000000, 5, catLaptop.Id,
         "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=400&q=80"),

        ("Lenovo ThinkPad X1 Carbon",
         "ThinkPad X1 Carbon Gen 12, Intel Core Ultra 7, 16GB LPDDR5, 512GB SSD, 14\" 2.8K OLED",
         36990000, 7, catLaptop.Id,
         "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400&q=80"),

        ("HP Spectre x360 14",
         "HP Spectre x360 14 inch 2-in-1, Core Ultra 5, 16GB RAM, 1TB SSD, OLED cảm ứng gập 360°",
         31000000, 9, catLaptop.Id,
         "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=400&q=80"),

        // --- Phụ kiện ---
        ("Tai nghe AirPods Pro 2",
         "AirPods Pro thế hệ 2, chống ồn ANC chủ động, âm thanh Spatial Audio, chip H2",
         5990000, 45, catPhuKien!.Id,
         "https://images.unsplash.com/photo-1603351154351-5e2d0600bb77?w=400&q=80"),

        ("Cáp sạc USB-C 100W",
         "Cáp sạc nhanh PD 100W, dài 2m, dây bện chống đứt, tương thích đa thiết bị",
         290000, 200, catPhuKien.Id,
         "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&q=80"),

        ("Sạc dự phòng Anker 20000mAh",
         "Anker PowerCore 20000mAh PD 22.5W, 2 cổng USB-A + 1 USB-C, công nghệ PowerIQ 3.0",
         1290000, 60, catPhuKien.Id,
         "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?w=400&q=80"),

        ("Ốp lưng MagSafe iPhone 15",
         "Ốp lưng silicon MagSafe cho iPhone 15/15 Pro, chống trầy, hỗ trợ sạc không dây",
         590000, 120, catPhuKien.Id,
         "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=400&q=80"),

        ("Tai nghe Sony WH-1000XM5",
         "Sony WH-1000XM5, chống ồn hàng đầu thế giới, 30h pin, kết nối multipoint đến 2 thiết bị",
         8490000, 22, catPhuKien.Id,
         "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&q=80"),

        // --- Màn hình ---
        ("Màn hình LG UltraWide 34\"",
         "LG 34\" 1440p IPS, 144Hz, HDR400, AMD FreeSync Premium, USB-C 96W, hỗ trợ PBP",
         12500000, 15, catManHinh!.Id,
         "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400&q=80"),

        ("Màn hình Dell UltraSharp 27\"",
         "Dell UltraSharp U2723QE 27\" 4K IPS, 100% sRGB, USB-C 90W, DisplayPort, HDMI 2.0",
         14800000, 10, catManHinh.Id,
         "https://images.unsplash.com/photo-1593640408182-31c228f52a10?w=400&q=80"),

        ("Màn hình Samsung OLED 32\"",
         "Samsung Odyssey OLED G8 32\" 4K 240Hz, 0.03ms GtG, HDR400, DisplayHDR True Black 400",
         19900000, 6, catManHinh.Id,
         "https://images.unsplash.com/photo-1585792180666-f7347c490ee2?w=400&q=80"),

        // --- Bàn phím & Chuột ---
        ("Bàn phím cơ Keychron K2 Pro",
         "Keychron K2 Pro 75%, Switch Gateron Pro Red, đèn RGB, Bluetooth 5.1 + USB-C, nhôm anodized",
         2290000, 30, catBanPhim!.Id,
         "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400&q=80"),

        ("Chuột Logitech MX Master 3S",
         "Logitech MX Master 3S, 8000 DPI, kết nối Logi Bolt + Bluetooth, sạc USB-C, bấm siêu yên tĩnh",
         2090000, 40, catBanPhim.Id,
         "https://images.unsplash.com/photo-1527814050087-3793815479db?w=400&q=80"),

        ("Bàn phím cơ Ducky One 3",
         "Ducky One 3 TKL, Switch Cherry MX Blue, PBT Dye-sub keycap, RGB, hỗ trợ hot-swap",
         2890000, 18, catBanPhim.Id,
         "https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=400&q=80"),

        ("Chuột gaming Razer DeathAdder V3",
         "Razer DeathAdder V3, 59g siêu nhẹ, sensor Focus Pro 30K DPI, Optical Switch thế hệ 3",
         1990000, 25, catBanPhim.Id,
         "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=400&q=80"),
    };

    foreach (var p in seedProducts)
    {
        var existing = db.Products.FirstOrDefault(x => x.Name == p.Name);
        if (existing == null)
        {
            // Thêm mới nếu chưa tồn tại
            db.Products.Add(new Product
            {
                Name = p.Name,
                Description = p.Desc,
                Price = p.Price,
                StockQuantity = p.Stock,
                CategoryProductId = p.CatId,
                ImageUrl = p.ImageUrl
            });
        }
        else if (string.IsNullOrWhiteSpace(existing.ImageUrl))
        {
            // Chỉ cập nhật ảnh nếu sản phẩm đã có nhưng chưa có ảnh
            existing.ImageUrl = p.ImageUrl;
            db.Products.Update(existing);
        }
    }
    db.SaveChanges();


 
    // Seed Customers (Khách hàng)
    if (!db.Customers.Any())
    {
        db.Customers.AddRange(
            new Customer { FullName = "Nguyễn Văn An", Email = "nguyenvanan@gmail.com", Phone = "0912 345 678", Address = "123 Nguyễn Trãi, Quận 1, TP.HCM", Password = "123456" },
            new Customer { FullName = "Trần Thị Bình", Email = "tranthibinh@gmail.com", Phone = "0908 765 432", Address = "456 Lê Lợi, Quận 3, TP.HCM", Password = "123456" },
            new Customer { FullName = "Lê Minh Cường", Email = "leminhcuong@gmail.com", Phone = "0933 111 222", Address = "789 Hai Bà Trưng, Quận Bình Thạnh, TP.HCM", Password = "123456" },
            new Customer { FullName = "Phạm Thị Dung", Email = "phamthidung@gmail.com", Phone = "0977 888 999", Address = "321 Đinh Tiên Hoàng, Quận Bình Thạnh, TP.HCM", Password = "123456" },
            new Customer { FullName = "Hoàng Văn Em", Email = "hoangvanem@gmail.com", Phone = "0944 666 777", Address = "654 Phạm Văn Đồng, Quận Gò Vấp, TP.HCM", Password = "123456" },
            new Customer { FullName = "Võ Thị Hoa", Email = "vothihoa@gmail.com", Phone = "0901 234 567", Address = "111 Cách Mạng Tháng 8, Quận 10, TP.HCM", Password = "123456" },
            new Customer { FullName = "Đặng Quốc Khải", Email = "dangquockhai@gmail.com", Phone = "0985 432 109", Address = "88 Nguyễn Huệ, Quận 1, TP.HCM", Password = "123456" }
        );
        db.SaveChanges();
    }

    // Seed Orders (Đơn hàng)
    if (!db.Orders.Any())
    {
        var customers = db.Customers.ToList();
        db.Orders.AddRange(
            new Order { CustomerId = customers[0].Id, OrderDate = DateTime.Now.AddDays(-10), Status = 2, Notes = "Giao hàng trước 18h" },
            new Order { CustomerId = customers[1].Id, OrderDate = DateTime.Now.AddDays(-7), Status = 2, Notes = "Gọi trước khi giao" },
            new Order { CustomerId = customers[2].Id, OrderDate = DateTime.Now.AddDays(-5), Status = 1, Notes = "Để hàng trước cửa nếu không có nhà" },
            new Order { CustomerId = customers[0].Id, OrderDate = DateTime.Now.AddDays(-3), Status = 1, Notes = null },
            new Order { CustomerId = customers[3].Id, OrderDate = DateTime.Now.AddDays(-2), Status = 0, Notes = "Cần đóng gói cẩn thận" },
            new Order { CustomerId = customers[4].Id, OrderDate = DateTime.Now.AddDays(-1), Status = 0, Notes = null },
            new Order { CustomerId = customers[5].Id, OrderDate = DateTime.Now, Status = 0, Notes = "Đặt làm quà tặng" }
        );
        db.SaveChanges();
    }

    // Seed OrderDetails (Chi tiết đơn hàng)
    if (!db.OrderDetails.Any())
    {
        var orders = db.Orders.ToList();
        var products = db.Products.ToList();

        db.OrderDetails.AddRange(
            // Đơn hàng 1
            new OrderDetail { OrderId = orders[0].Id, ProductId = products[0].Id, Quantity = 1, UnitPrice = 32990000 },
            new OrderDetail { OrderId = orders[0].Id, ProductId = products[6].Id, Quantity = 2, UnitPrice = 5990000 },
            // Đơn hàng 2
            new OrderDetail { OrderId = orders[1].Id, ProductId = products[3].Id, Quantity = 1, UnitPrice = 45990000 },
            new OrderDetail { OrderId = orders[1].Id, ProductId = products[9].Id, Quantity = 1, UnitPrice = 2290000 },
            // Đơn hàng 3
            new OrderDetail { OrderId = orders[2].Id, ProductId = products[1].Id, Quantity = 1, UnitPrice = 28490000 },
            // Đơn hàng 4
            new OrderDetail { OrderId = orders[3].Id, ProductId = products[4].Id, Quantity = 1, UnitPrice = 38500000 },
            new OrderDetail { OrderId = orders[3].Id, ProductId = products[7].Id, Quantity = 3, UnitPrice = 290000 },
            // Đơn hàng 5
            new OrderDetail { OrderId = orders[4].Id, ProductId = products[2].Id, Quantity = 1, UnitPrice = 19990000 },
            // Đơn hàng 6
            new OrderDetail { OrderId = orders[5].Id, ProductId = products[8].Id, Quantity = 2, UnitPrice = 12500000 },
            // Đơn hàng 7
            new OrderDetail { OrderId = orders[6].Id, ProductId = products[5].Id, Quantity = 1, UnitPrice = 42000000 },
            new OrderDetail { OrderId = orders[6].Id, ProductId = products[6].Id, Quantity = 1, UnitPrice = 5990000 }
        );
        db.SaveChanges();
    }
}

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

// Buổi 5 - Thứ tự quan trọng: Authentication phải đứng TRƯỚC Authorization
// BƯỚC A: Xác nhận "Anh là ai?" (Kiểm tra thẻ bài / Cookie)
app.UseAuthentication();
// BƯỚC B: Xác nhận "Anh được làm gì?" (Kiểm tra quyền)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
