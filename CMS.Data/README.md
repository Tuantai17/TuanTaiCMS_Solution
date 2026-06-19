# CMS.Data - Entity Framework Core Data Access Layer

**CMS.Data** là dự án thư viện lớp (Class Library) đóng vai trò là tầng truy xuất dữ liệu (Data Access Layer) của toàn bộ Solution. Dự án này chịu trách nhiệm định nghĩa cấu trúc các bảng dữ liệu (Entities) và thiết lập ngữ cảnh kết nối SQL Server thông qua **Entity Framework Core (EF Core)**.

---

## 🎓 THÔNG TIN SINH VIÊN thực hiện
- **Họ và tên:** Nguyễn Tuấn Tài
- **Mã Số Sinh Viên (MSSV):** 2123110166
- **Lớp:** CCQ2311E
- **Môn học:** Chuyên đề ASP.NET

---

## 1. Cấu trúc thư mục của CMS.Data

```text
CMS.Data/
├── Entities/                  # Chứa định nghĩa các thực thể ánh xạ xuống bảng cơ sở dữ liệu
│   ├── User.cs                # Thực thể quản trị nội bộ (Admin/Staff)
│   ├── Customer.cs            # Thực thể khách hàng đăng ký mua sắm ở frontend
│   ├── CategoryProduct.cs     # Danh mục sản phẩm (Ví dụ: LEGO, Hot Wheels...)
│   ├── Product.cs             # Thông tin sản phẩm đồ chơi/thời trang
│   ├── ProductImage.cs        # Các hình ảnh phụ đi kèm của sản phẩm
│   ├── Category.cs            # Chuyên mục bài viết tin tức
│   ├── Post.cs                # Bài viết blog/tin tức
│   ├── Order.cs               # Đơn hàng tổng quan (ngày đặt, trạng thái, ghi chú)
│   ├── OrderDetail.cs         # Chi tiết từng mặt hàng và số lượng đã mua trong đơn hàng
│   ├── Menu.cs                # Thanh điều hướng menu động của Website React
│   └── Banner.cs              # Hình ảnh slider chuyển động trang chủ
├── Migrations/                # Chứa các file nâng cấp cấu trúc Database tự sinh của EF Core
├── ApplicationDbContext.cs    # Ngữ cảnh DbContext kết nối CSDL và khai báo các DbSet
└── CMS.Data.csproj            # File dự án chứa thông tin thư viện EF Core phụ thuộc
```

---

## 2. Chi tiết các bảng Cơ sở dữ liệu (DbSets)

Lớp `ApplicationDbContext.cs` khai báo 11 bảng dữ liệu phục vụ các nghiệp vụ quản trị và bán hàng:

| Tên DbSet | Thực thể (Entity) | Vai trò nghiệp vụ |
| :--- | :--- | :--- |
| `Users` | `User` | Lưu trữ tài khoản của Admin (quản trị) và Staff (nhân viên) |
| `Customers` | `Customer` | Lưu trữ tài khoản của Khách hàng đăng ký ở Frontend React |
| `CategoriesProducts` | `CategoryProduct` | Phân loại danh mục sản phẩm của cửa hàng |
| `Products` | `Product` | Danh sách sản phẩm, giá bán, tồn kho và trạng thái New/Sale |
| `ProductImages` | `ProductImage` | Lưu trữ các góc chụp phụ khác của sản phẩm |
| `Categories` | `Category` | Chuyên mục phân loại bài viết blog cẩm nang tin tức |
| `Posts` | `Post` | Danh sách bài viết tin tức, bài viết nổi bật |
| `Orders` | `Order` | Thông tin đơn hàng (ngày đặt, mã khách hàng, ghi chú, trạng thái) |
| `OrderDetails` | `OrderDetail` | Dòng chi tiết đơn hàng (mã sản phẩm, số lượng, đơn giá mua) |
| `Menus` | `Menu` | Danh sách liên kết menu điều hướng động |
| `Banners` | `Banner` | Các hình ảnh banner quảng cáo động hiển thị ở trang chủ |

---

## 3. Các mối quan hệ (Relationships) chính giữa các bảng

* **CategoryProduct (1) - Product (N):** Một danh mục sản phẩm chứa nhiều sản phẩm. Nếu xóa danh mục, các sản phẩm thuộc danh mục đó sẽ được xử lý ràng buộc.
* **Product (1) - ProductImage (N):** Một sản phẩm có thể hiển thị nhiều góc chụp hình ảnh phụ khác nhau.
* **Category (1) - Post (N):** Một chuyên mục tin tức chứa nhiều bài viết khác nhau.
* **Customer (1) - Order (N):** Một khách hàng có thể đặt nhiều đơn hàng trong lịch sử mua sắm.
* **Order (1) - OrderDetail (N):** Một đơn hàng chứa nhiều dòng chi tiết sản phẩm mua khác nhau.
* **Product (1) - OrderDetail (N):** Một dòng chi tiết đơn hàng liên kết trỏ tới thông tin của một sản phẩm cụ thể.

---

## 4. Hướng dẫn quản lý Cơ sở dữ liệu bằng EF Core CLI

Khi có sự thay đổi về cấu trúc Entity (thêm cột, sửa kiểu dữ liệu...), hãy chạy các lệnh sau ở cửa sổ Terminal tại thư mục gốc của Solution:

### Lệnh 1: Tạo tệp Migration mới
Lệnh này ghi nhận sự thay đổi cấu trúc mã nguồn sang file SQL trung gian:
```powershell
# Chạy lệnh tại thư mục gốc Solution, trỏ dự án chính vào CMS.Backend và dự án data vào CMS.Data
dotnet ef migrations add <TenMigrationMoi> --project CMS.Data --startup-project CMS.Backend
```

### Lệnh 2: Áp dụng thay đổi cấu trúc xuống SQL Server
Lệnh này chạy các file Migration để tạo bảng/cột mới trong SQL Server:
```powershell
dotnet ef database update --project CMS.Data --startup-project CMS.Backend
```

*(Lưu ý: Nếu làm việc trực tiếp trên Visual Studio, bạn có thể mở **Package Manager Console**, chọn dự án mặc định (Default project) là `CMS.Data` và dùng lệnh `Add-Migration <Ten>` và `Update-Database`).*
