# TuanTaiCMS_Solution - Hệ thống Quản trị Nội dung (CMS) & Bán hàng Toàn diện

Chào mừng bạn đến với **TuanTaiCMS_Solution**! Đây là đồ án thực hành môn học **Chuyên đề ASP.NET**, xây dựng một giải pháp CMS và thương mại điện tử hoàn chỉnh. Dự án tích hợp sự mạnh mẽ của **ASP.NET Core 8.0 MVC & Web API** ở phần quản trị (Backend) cùng giao diện người dùng sống động, phản hồi nhanh bằng **React 19** (Frontend).

Giải pháp được chia tách theo các lớp chịu trách nhiệm độc lập giúp dễ dàng mở rộng, bảo trì và kiểm thử.

---

## 🎓 THÔNG TIN SINH VIÊN thực hiện
- **Họ và tên:** Nguyễn Tuấn Tài
- **Mã Số Sinh Viên (MSSV):** 2123110166
- **Lớp:** CCQ2311E
- **Môn học:** Chuyên đề ASP.NET
- **Tên Solution:** TuanTaiCMS_Solution

---

## 🏗️ Kiến trúc & Cấu trúc Thư mục Solution

Thư mục dự án được tổ chức thành 3 phần chính. Mỗi phần đều đi kèm tài liệu hướng dẫn cấu hình và vận hành chi tiết riêng:

```text
TuanTaiCMS_Solution/
├── 📂 CMS.Backend/         # Dự án ASP.NET Core 8.0 MVC (Quản trị viên) & Web API Services
│   ├── 📂 Controllers/     # Logic điều phối giao diện MVC và các endpoint JSON APIs
│   ├── 📂 Views/           # Giao diện quản trị Admin Panel (Razor View Engine .cshtml)
│   ├── 📂 Helpers/         # Các tiện ích (gửi mail xác thực OTP Gmail, mã hóa BCrypt, seed dữ liệu)
│   ├── 📂 wwwroot/         # Tài nguyên tĩnh của trang quản trị (CSS, JS, ảnh upload)
│   └── 📄 README.md        # [Xem chi tiết tại CMS.Backend/README.md]
│
├── 📂 CMS.Data/            # Dự án Class Library quản lý Cơ sở dữ liệu bằng EF Core
│   ├── 📂 Entities/        # Định nghĩa 11 thực thể (Database Models)
│   ├── 📂 Migrations/      # Lịch sử đồng bộ hóa cấu trúc Database (EF Core Migrations)
│   ├── 📄 ApplicationDbContext.cs # Ngữ cảnh kết nối Database
│   └── 📄 README.md        # [Xem chi tiết tại CMS.Data/README.md]
│
├── 📂 cms.frontend/        # Ứng dụng khách hàng viết bằng ReactJS 19
│   ├── 📂 src/             # Source code giao diện React (pages, components, services, utils)
│   ├── 📂 public/          # Tài nguyên tĩnh phục vụ React client
│   └── 📄 README.md        # [Xem chi tiết tại cms.frontend/README.md]
│
├── 📄 TuanTaiCMS_Solution.sln # File quản lý liên kết các dự án của Visual Studio
└── 📄 README.md            # Tài liệu tổng quan của toàn bộ dự án (File này)
```

> [!NOTE]
> Vui lòng truy cập trực tiếp tài liệu chi tiết của từng thư mục dự án để đọc hướng dẫn sâu hơn:
> - Xem hướng dẫn quản trị, phân quyền MVC và cấu hình API tại: [CMS.Backend/README.md](file:///e:/ASP.NET/TuanTaiCMS_Solution/CMS.Backend/README.md)
> - Xem hướng dẫn quản lý Migration và sơ đồ các bảng dữ liệu tại: [CMS.Data/README.md](file:///e:/ASP.NET/TuanTaiCMS_Solution/CMS.Data/README.md)
> - Xem hướng dẫn cấu hình React, định tuyến và các dịch vụ kết nối API tại: [cms.frontend/README.md](file:///e:/ASP.NET/TuanTaiCMS_Solution/cms.frontend/README.md)

---

## 🛠️ Công nghệ Sử dụng chủ đạo

### 🖥️ 1. Backend & Data Layer
- **Framework chính:** [ASP.NET Core 8.0 MVC & Web API](https://learn.microsoft.com/en-us/aspnet/core/)
- **ORM & Database:** [Entity Framework Core 8.0](https://learn.microsoft.com/en-us/ef/core/) và Microsoft SQL Server.
- **Xác thực hệ thống Admin:** Cookie Authentication (`CookieAuthenticationDefaults`).
- **Mã hóa bảo mật:** `BCrypt.Net-Next` dùng để mã hóa một chiều mật khẩu của quản trị viên và khách hàng.
- **Đặc tả kiểm thử API:** Swagger UI (`Swashbuckle.AspNetCore`).

### 🌐 2. Frontend Client
- **Thư viện chính:** [React 19.2.6](https://react.dev/)
- **Xử lý routing:** `React Router DOM v7`.
- **Kết nối HTTP:** `Axios` hỗ trợ Interceptors tự động gọt dẹp response JSON.
- **Thư viện UI bổ trợ:** Swiper v12 (carousel hình ảnh), Bootstrap 4/5, Font Awesome v6, Outfit Font (Google Fonts).

---

## 🌟 Các chức năng nổi bật của dự án

1. **Phân quyền 3 nhóm người dùng chặt chẽ:**
   - **Admin:** Quản trị tối cao (được vào trang quản lý tài khoản nội bộ và nghiệp vụ).
   - **Staff:** Nhân viên nghiệp vụ (được duyệt đơn hàng, quản lý sản phẩm nhưng bị chặn mục quản lý tài khoản).
   - **Customer:** Khách hàng (chỉ giao dịch và tương tác tại giao diện React, bị chặn tuyệt đối trang Admin backend).
2. **Quy trình Mua hàng & Email tự động:**
   - Khách hàng thêm sản phẩm vào giỏ, điền thông tin và thực hiện đặt hàng.
   - Hệ thống chạy transaction an toàn: chèn hóa đơn mới, trừ số lượng tồn kho sản phẩm trong database và kích hoạt tác vụ nền gửi email HTML xác nhận hóa đơn đến Gmail của khách hàng.
3. **Khôi phục mật khẩu 3 bước qua OTP Email:**
   - Người dùng yêu cầu khôi phục mật khẩu. Hệ thống gửi mã OTP 6 số qua email.
   - Giao diện React thiết kế cao cấp (6 ô nhập tự chuyển focus, đếm ngược đếm giây trực quan) cho phép xác minh OTP và đổi mật khẩu mới bảo mật.
4. **Nội dung hiển thị động (Dynamic Content):**
   - Slider banner trang chủ và thanh menu điều hướng được load động từ Database thông qua Web API, giúp người quản trị dễ dàng thay đổi giao diện frontend từ trang Admin.

---

## ⚙️ Hướng dẫn Khởi chạy nhanh toàn bộ dự án

Để khởi chạy dự án trên máy tính cục bộ của bạn, vui lòng thực hiện tuần tự các bước dưới đây:

### Bước 1: Yêu cầu chuẩn bị
- Đã cài đặt [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
- Đã cài đặt [Node.js](https://nodejs.org/) (Khuyên dùng bản LTS) & npm.
- Đã cài đặt Microsoft SQL Server (hoặc LocalDB).

---

### Bước 2: Thiết lập Cơ sở dữ liệu và Chạy Backend

#### **A. Cập nhật chuỗi kết nối và SMTP**
Mở file [appsettings.json](file:///e:/ASP.NET/TuanTaiCMS_Solution/CMS.Backend/appsettings.json) trong thư mục `CMS.Backend` và thay đổi `DefaultConnection` và cấu hình `EmailSettings` của bạn:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TuanTaiCMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "email_cua_ban@gmail.com",
    "Username": "email_cua_ban@gmail.com",
    "Password": "mat_khau_ung_dung_gmail"
  }
}
```

#### **B. Áp dụng Database Migration**
Mở Terminal tại thư mục gốc của Solution và thực hiện cập nhật CSDL:
```powershell
# Di chuyển vào thư mục backend
cd CMS.Backend

# Chạy lệnh cập nhật database từ thư viện CMS.Data
dotnet ef database update --project ../CMS.Data/CMS.Data.csproj
```

#### **C. Chạy ứng dụng Backend**
Khởi chạy dự án ASP.NET Core:
```powershell
dotnet run
```
*Giao diện Swagger API sẽ chạy tại: [https://localhost:7238/swagger](https://localhost:7238/swagger)*
*Giao diện Admin Panel sẽ chạy tại: [https://localhost:7238](https://localhost:7238)*

---

### Bước 3: Cài đặt và Chạy Frontend ReactJS

#### **A. Cấu hình địa chỉ kết nối**
Mở file [.env](file:///e:/ASP.NET/TuanTaiCMS_Solution/cms.frontend/.env) trong thư mục `cms.frontend` để kiểm tra địa chỉ API:
```env
REACT_APP_API_URL=https://localhost:7238/api
```

#### **B. Khởi động Client**
Mở một cửa sổ Terminal mới, di chuyển vào thư mục frontend và khởi chạy:
```powershell
# Di chuyển vào thư mục frontend
cd cms.frontend

# Cài đặt các thư viện
npm install

# Khởi chạy dự án
npm start
```
*Ứng dụng React sẽ tự động mở tại địa chỉ: [http://localhost:3000](http://localhost:3000)*

---

## 👥 Tác giả thực hiện
* **Họ và tên:** Nguyễn Tuấn Tài
- **MSSV:** 2123110166
* **GitHub Profile:** [TuanTai17](https://github.com/TuanTai17)
