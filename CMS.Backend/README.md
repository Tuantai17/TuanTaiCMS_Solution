# CMS.Backend - TuanTaiCMS Administration & API Services

**CMS.Backend** là dự án lõi (Core Project) của Solution, được xây dựng trên nền tảng **ASP.NET Core 8.0 MVC** kết hợp **Web API**. Dự án này đảm nhận hai nhiệm vụ chính:
1. **Giao diện quản trị (MVC Admin Panel):** Dành cho Quản trị viên (Admin) và Nhân viên (Staff) quản lý toàn bộ dữ liệu hệ thống thông qua giao diện web Razor View (.cshtml).
2. **Cổng cung cấp dữ liệu (JSON Web API):** Phục vụ các yêu cầu rút/chèn dữ liệu của ứng dụng khách hàng ReactJS (`cms.frontend`).

---

## 🎓 THÔNG TIN SINH VIÊN thực hiện
- **Họ và tên:** Nguyễn Tuấn Tài
- **Mã Số Sinh Viên (MSSV):** 2123110166
- **Lớp:** CCQ2311E
- **Môn học:** Chuyên đề ASP.NET

---

## 1. Cấu trúc thư mục chính của Backend

```text
CMS.Backend/
├── Controllers/              # Điều hướng và xử lý logic nghiệp vụ chính
│   ├── AccountController.cs  # Quản lý đăng nhập/đăng xuất bằng Cookie cho Admin/Staff
│   ├── AuthController.cs     # [API] Đăng nhập, đăng ký, khôi phục mật khẩu OTP của Customer
│   ├── BannersController.cs  # [API] Cung cấp danh sách banner đang hiển thị lên React
│   ├── MenusController.cs    # [API] Cung cấp thanh menu điều hướng động lên React
│   ├── ProductsController.cs # [API] Cung cấp sản phẩm động và lọc theo yêu cầu
│   ├── OrdersController.cs   # [API] Nhận đơn đặt hàng và trả về lịch sử mua hàng
│   ├── UserController.cs     # Quản lý tài khoản nội bộ (chỉ dành cho Admin)
│   └── ...                   # Các Controller MVC phục vụ các nghiệp vụ CRUD khác
├── Models/                   # Chứa ViewModels và BindingModels truyền nhận dữ liệu
├── Views/                    # Giao diện quản trị Razor View (.cshtml)
│   ├── Account/              # Giao diện Đăng nhập, Từ chối quyền của Admin/Staff
│   ├── User/                 # Giao diện quản lý Admin/Staff
│   ├── Order/                # Giao diện quản lý & phê duyệt đơn hàng
│   ├── Shared/               # Layout Layout.cshtml, Sidebar và các partial view
│   └── ...                   # Các View quản trị danh mục, sản phẩm, bài viết
├── Helpers/                  # Tiện ích bổ trợ cho hệ thống
│   ├── EmailHelper.cs        # Xử lý gửi thư HTML bảo mật (OTP, xác nhận đơn hàng)
│   ├── PasswordHelper.cs     # Mã hóa mật khẩu một chiều bằng thuật toán BCrypt
│   └── DbInitializer.cs      # Seed dữ liệu mặc định ban đầu và dọn dẹp DB
├── wwwroot/                  # File tài nguyên tĩnh phục vụ Admin Panel (CSS, JS, Uploads, Images)
├── Program.cs                # File cấu hình dịch vụ, CORS, Middlewares và khởi chạy ứng dụng
└── appsettings.json          # File chứa chuỗi kết nối SQL Server và SMTP Gmail
```

---

## 2. Các thiết lập cấu hình quan trọng trong `Program.cs`

File `Program.cs` thiết lập các cơ chế hoạt động cốt lõi của Server:
* **JSON Serializer Option:** Cấu hình bỏ qua lỗi vòng lặp tham chiếu (`ReferenceHandler.IgnoreCycles`) giúp phản hồi JSON sạch hơn.
* **Xác thực Cookie (Cookie Authentication):** Định nghĩa bộ lọc xác thực cho hệ thống Admin qua `/Account/Login` và AccessDenied qua `/Account/AccessDenied`.
* **CORS Policy ("AllowAll"):** Cho phép ứng dụng khách hàng ReactJS (`http://localhost:3000`) gọi API và lấy dữ liệu JSON không bị lỗi CORS.
* **Dịch vụ bổ sung:** Swagger UI (`/swagger`) được kích hoạt để kiểm thử API và dịch vụ gửi mail `EmailHelper` được tiêm dưới dạng `AddTransient`.
* **Middlewares Pipeline:** Tuân thủ thứ tự: `UseCors` -> `UseAuthentication` -> `UseAuthorization` -> `MapControllers` (cho API) -> `MapControllerRoute` (cho MVC).

---

## 3. Cơ chế Khởi tạo dữ liệu & Tự động mã hóa mật khẩu

Khi chạy dự án, hệ thống sẽ thực thi đoạn mã tự động trong `Program.cs`:
1. **Tạo Database & Seed dữ liệu:** Gọi `DbInitializer.Initialize()` tự động tạo cấu trúc bảng trong SQL Server (nếu chưa có) và nạp dữ liệu Admin/Staff mặc định, Menu mặc định, Banner mặc định.
2. **Dọn dẹp tài khoản:** Tự động loại bỏ các tài khoản `Users` có Role không hợp lệ (Role khác `Admin` hoặc `Staff`).
3. **Mã hóa BCrypt tự động:** Quét toàn bộ mật khẩu Plain Text trong bảng `Users` và `Customers` (nếu có tài khoản chưa được mã hóa) để tiến hành mã hóa BCrypt nhằm nâng cao tính bảo mật.

---

## 4. Quản lý Quyền truy cập phía Admin/Staff (MVC)

Các Controller quản trị trong Backend được bảo vệ bởi thuộc tính `[Authorize]`:
* **Quyền nhân viên nghiệp vụ:** Cho phép cả Admin và Staff truy cập để cập nhật sản phẩm, đơn hàng, danh mục.
  ```csharp
  [Authorize(Roles = "Admin,Staff")]
  ```
* **Quyền quản trị viên tối cao:** Chỉ cho phép tài khoản Admin truy cập để quản lý danh sách nhân viên nội bộ (`UserController`).
  ```csharp
  [Authorize(Roles = "Admin")]
  ```

---

## 5. Hướng dẫn Khởi chạy & cấu hình Mail Service

### Bước 1: Cấu hình ConnectionString và Mail SMTP
Mở file `appsettings.json` trong thư mục `CMS.Backend` và thay đổi các cấu hình sau:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TuanTaiCMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 586,
    "SenderEmail": "your_email@gmail.com",
    "SenderName": "Vương Quốc Đồ Chơi MyKingdom",
    "Username": "your_email@gmail.com",
    "Password": "your_app_password" // Mật khẩu ứng dụng tạo từ tài khoản Google
  }
}
```

### Bước 2: Khởi chạy dự án Backend
Mở Terminal tại thư mục `CMS.Backend/` và chạy lệnh:
```bash
dotnet run
```
Sau khi chạy thành công, Server sẽ lắng nghe ở các địa chỉ cổng local:
- Địa chỉ MVC Admin: [https://localhost:7238](https://localhost:7238)
- Địa chỉ Swagger UI (Kiểm thử API): [https://localhost:7238/swagger](https://localhost:7238/swagger)

---

## 6. Tài khoản kiểm thử mặc định hệ thống quản trị
* **Tài khoản Admin:** `admin` / mật khẩu: `admin123`
* **Tài khoản Staff:** `staff` / mật khẩu: `staff123`
