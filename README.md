#  TuanTaiCMS_Solution

Chào mừng bạn đến với **TuanTaiCMS_Solution**! Đây là đồ án thực hành dành cho môn học **Chuyên đề ASP.NET**. Dự án xây dựng một hệ thống quản trị nội dung (CMS) và quản lý bán hàng toàn diện, kết hợp sức mạnh của **ASP.NET Core 8.0 MVC** ở phần quản trị (Backend) và sự linh hoạt của **React 19** cho trải nghiệm người dùng (Frontend).

Dự án được thực hiện nhằm mục tiêu ứng dụng thực tiễn các kiến thức môn học để mang lại giải pháp quản trị dễ dàng, hiệu năng cao và có cấu trúc rõ ràng.

---

## 🎓 THÔNG TIN SINH VIÊN

| Thông tin                       | Chi tiết            |
| :------------------------------- | :------------------- |
| **Sinh viên thực hiện** | Nguyễn Tuấn Tài   |
| **MSSV**                   | 2123110166           |
| **Lớp**                   | CCQ2311E             |
| **Môn học**              | Chuyên đề ASP.NET |
| **Tên Solution**          | TuanTaiCMS_Solution  |

## 🏗️ Kiến trúc & Cấu trúc Thư mục

Giải pháp (Solution) được chia làm 3 phần chính theo mô hình chia sẻ trách nhiệm (Separation of Concerns):

```text
TuanTaiCMS_Solution/
├── 📂 CMS.Backend/         # Dự án ASP.NET Core 8.0 MVC (Quản trị viên)
│   ├── 📂 Controllers/     # Điều hướng và xử lý logic nghiệp vụ
│   ├── 📂 Views/           # Giao diện quản trị (HTML/Razor)
│   ├── 📂 Models/          # ViewModels và Binding Models
│   ├── 📂 wwwroot/         # File tĩnh (CSS, JS, Images, v.v.)
│   └── 📄 Program.cs       # Cấu hình dịch vụ, Middleware & Seed dữ liệu
│
├── 📂 CMS.Data/            # Dự án Class Library quản lý Cơ sở Dữ liệu
│   ├── 📂 Entities/        # Các thực thể dữ liệu (Database Models)
│   ├── 📂 Migrations/      # Lịch sử nâng cấp cấu trúc Database (EF Core)
│   └── 📄 ApplicationDbContext.cs # Đối tượng kết nối và truy vấn CSDL
│
└── 📂 cms.frontend/        # Ứng dụng khách hàng viết bằng ReactJS
    ├── 📂 src/             # Source code giao diện React
    ├── 📂 public/          # File tĩnh (Index.html, Assets, v.v.)
    └── 📄 package.json     # Quản lý thư viện và script Node.js
```

---

## 🛠️ Công nghệ Sử dụng

### 🖥️ Backend (CMS.Backend & CMS.Data)

- **Framework chính:** [ASP.NET Core 8.0 MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
- **ORM (Truy vấn CSDL):** [Entity Framework Core 8.0.8](https://learn.microsoft.com/en-us/ef/core/)
- **Hệ quản trị CSDL:** Microsoft SQL Server
- **Xác thực và Phân quyền:** Cookie Authentication (`Microsoft.AspNetCore.Authentication.Cookies`)
- **Các thực thể chính (Entities):**
  - `User`: Quản trị viên hệ thống.
  - `Category` & `Post`: Danh mục và bài viết tin tức.
  - `CategoryProduct` & `Product`: Danh mục và sản phẩm thương mại điện tử.
  - `Customer`: Thông tin khách hàng.
  - `Order` & `OrderDetail`: Đơn hàng và chi tiết sản phẩm trong đơn hàng.

### 🌐 Frontend (cms.frontend)

- **Thư viện chính:** [React 19.2.6](https://react.dev/)
- **Công cụ xây dựng:** `react-scripts` (Create React App)
- **Ngọn ngữ:** JavaScript (ES6+)

---

## ⚙️ Hướng dẫn Cài đặt & Khởi chạy

Để chạy được toàn bộ dự án trên máy cục bộ, vui lòng làm theo các bước dưới đây:

### 1. Yêu cầu hệ thống

- [SDK .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên.
- [Node.js](https://nodejs.org/) (Khuyên dùng bản LTS) & npm.
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/) (hoặc LocalDB).

---

### 2. Cấu hình & Chạy Backend (`CMS.Backend`)

#### **Bước A: Cấu hình Connection String**

Mở file `e:\ASP.NET\TuanTaiCMS_Solution\CMS.Backend\appsettings.json` và điều chỉnh chuỗi kết nối SQL Server của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TuanTaiCMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

#### **Bước B: Cập nhật Database (Migration)**

Mở terminal tại thư mục dự án và chạy các lệnh sau để khởi tạo cơ sở dữ liệu:

```powershell
# Di chuyển đến thư mục backend
cd CMS.Backend

# Áp dụng Migration vào SQL Server
dotnet ef database update --project ../CMS.Data/CMS.Data.csproj
```

_(Hoặc trong Visual Studio, mở **Package Manager Console**, chọn Default Project là `CMS.Data` và chạy lệnh `Update-Database`)_

> [!NOTE]
> Hệ thống có sẵn tính năng **Tự động Seed dữ liệu mẫu** trong `Program.cs`. Trong lần chạy đầu tiên, hệ thống sẽ tự động thêm các danh mục sản phẩm mẫu, danh sách sản phẩm (iPhone, MacBook, Dell XPS, v.v. kèm ảnh thực tế từ Unsplash), khách hàng, đơn hàng mẫu vào Database nếu chưa có dữ liệu.

#### **Bước C: Khởi chạy dự án Backend**

Chạy lệnh sau tại thư mục `CMS.Backend`:

```powershell
dotnet run
```

Ứng dụng sẽ chạy ở địa chỉ mặc định (thường là `https://localhost:7198` hoặc `http://localhost:5198`). Bạn có thể mở trình duyệt và truy cập để vào giao diện quản trị.

---

### 3. Cài đặt & Khởi chạy Frontend (`cms.frontend`)

#### **Bước A: Cài đặt thư viện**

Mở terminal mới tại thư mục `cms.frontend` và cài đặt các phụ thuộc Node.js:

```powershell
cd cms.frontend
npm install
```

#### **Bước B: Khởi chạy ứng dụng**

Chạy ứng dụng React ở môi trường lập trình local:

```powershell
npm start
```

Ứng dụng sẽ tự động mở trên trình duyệt tại địa chỉ `http://localhost:3000`.

---

## 👤 Tác giả

* **Họ và tên:** Nguyễn Tuấn Tài
* **GitHub:** [TuanTai17](https://github.com/TuanTai17)
