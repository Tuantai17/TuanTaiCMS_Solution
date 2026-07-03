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
│   ├── CustomerAddress.cs     # Sổ địa chỉ nhận hàng của khách hàng
│   ├── PasswordResetToken.cs  # Mã token đặt lại mật khẩu của khách hàng
│   ├── CategoryProduct.cs     # Danh mục sản phẩm (Ví dụ: LEGO, Hot Wheels...)
│   ├── Product.cs             # Thông tin sản phẩm đồ chơi/thời trang
│   ├── ProductImage.cs        # Các hình ảnh phụ đi kèm của sản phẩm
│   ├── ProductFavorite.cs     # Sản phẩm yêu thích của khách hàng (Wishlist)
│   ├── Inventory.cs           # Tồn kho sản phẩm
│   ├── InventoryTransaction.cs# Lịch sử giao dịch nhập/xuất kho
│   ├── ProductReview.cs       # Đánh giá sản phẩm của khách hàng
│   ├── ProductReviewImage.cs  # Hình ảnh đính kèm trong đánh giá
│   ├── ProductReviewReply.cs  # Phản hồi đánh giá từ Admin/Staff
│   ├── Category.cs            # Chuyên mục bài viết tin tức
│   ├── Post.cs                # Bài viết blog/tin tức
│   ├── Order.cs               # Đơn hàng tổng quan (ngày đặt, trạng thái, ghi chú)
│   ├── OrderDetail.cs         # Chi tiết từng mặt hàng và số lượng đã mua trong đơn hàng
│   ├── OrderActivityLog.cs    # Lịch sử hoạt động của đơn hàng
│   ├── OrderItemIssue.cs      # Sự cố đơn hàng (Đổi/Trả hàng/Khiếu nại)
│   ├── Menu.cs                # Thanh điều hướng menu động của Website React
│   ├── Banner.cs              # Hình ảnh slider chuyển động trang chủ
│   ├── SupportTicket.cs       # Yêu cầu hỗ trợ từ khách hàng
│   ├── SupportTicketMessage.cs# Chi tiết tin nhắn trao đổi trong yêu cầu hỗ trợ
│   ├── Notification.cs        # Thông báo hệ thống cho Admin/Staff/Customer
│   └── EmailLog.cs            # Lịch sử gửi email hệ thống
├── Enums/                     # Chứa các kiểu liệt kê trạng thái (Enums)
│   ├── OrderItemIssueEnums.cs # Trạng thái sự cố đơn hàng
│   ├── OrderStatus.cs         # Trạng thái đơn hàng
│   └── ReviewStatus.cs        # Trạng thái đánh giá
├── Migrations/                # Chứa các file nâng cấp cấu trúc Database tự sinh của EF Core
├── ApplicationDbContext.cs    # Ngữ cảnh DbContext kết nối CSDL và khai báo các DbSet
└── CMS.Data.csproj            # File dự án chứa thông tin thư viện EF Core phụ thuộc
```

---

## 2. Chi tiết các bảng Cơ sở dữ liệu (DbSets)

Lớp `ApplicationDbContext.cs` khai báo 12 bảng dữ liệu phục vụ các nghiệp vụ quản trị và bán hàng:

| Tên DbSet | Thực thể (Entity) | Vai trò nghiệp vụ |
| :--- | :--- | :--- |
| `Users` | `User` | Lưu trữ tài khoản của Admin (quản trị) và Staff (nhân viên) |
| `Customers` | `Customer` | Lưu trữ tài khoản của Khách hàng đăng ký ở Frontend React |
| `CustomerAddresses` | `CustomerAddress` | Lưu trữ danh sách địa chỉ nhận hàng của Khách hàng |
| `PasswordResetTokens` | `PasswordResetToken` | Lưu trữ mã token khôi phục mật khẩu |
| `CategoriesProducts` | `CategoryProduct` | Phân loại danh mục sản phẩm của cửa hàng |
| `Products` | `Product` | Danh sách sản phẩm, giá bán, tồn kho và trạng thái New/Sale |
| `ProductImages` | `ProductImage` | Lưu trữ các góc chụp phụ khác của sản phẩm |
| `ProductFavorites` | `ProductFavorite` | Lưu trữ danh sách sản phẩm yêu thích của khách hàng |
| `Inventories` | `Inventory` | Quản lý tồn kho theo sản phẩm |
| `InventoryTransactions` | `InventoryTransaction` | Lịch sử nhập, xuất kho |
| `ProductReviews` | `ProductReview` | Lưu trữ đánh giá sản phẩm |
| `ProductReviewImages` | `ProductReviewImage` | Hình ảnh kèm theo trong đánh giá |
| `ProductReviewReplies` | `ProductReviewReply` | Câu trả lời phản hồi từ nhân viên cho đánh giá |
| `Categories` | `Category` | Chuyên mục phân loại bài viết blog cẩm nang tin tức |
| `Posts` | `Post` | Danh sách bài viết tin tức, bài viết nổi bật |
| `Orders` | `Order` | Thông tin đơn hàng (ngày đặt, mã khách hàng, ghi chú, trạng thái) |
| `OrderDetails` | `OrderDetail` | Dòng chi tiết đơn hàng (mã sản phẩm, số lượng, đơn giá mua) |
| `OrderActivityLogs` | `OrderActivityLog` | Lưu lịch sử thay đổi trạng thái đơn hàng |
| `OrderItemIssues` | `OrderItemIssue` | Theo dõi sự cố (đổi/trả) trên từng sản phẩm của đơn hàng |
| `Menus` | `Menu` | Danh sách liên kết menu điều hướng động |
| `Banners` | `Banner` | Các hình ảnh banner quảng cáo động hiển thị ở trang chủ |
| `SupportTickets` | `SupportTicket` | Yêu cầu hỗ trợ từ người dùng |
| `SupportTicketMessages` | `SupportTicketMessage` | Nội dung chat giữa nhân viên và khách hàng |
| `Notifications` | `Notification` | Quản lý thông báo cho người dùng hoặc nhân viên |
| `EmailLogs` | `EmailLog` | Ghi nhận lịch sử gửi email |

---

## 3. Các mối quan hệ (Relationships) chính giữa các bảng

* **CategoryProduct (1) - Product (N):** Một danh mục sản phẩm chứa nhiều sản phẩm. Nếu xóa danh mục, các sản phẩm thuộc danh mục đó sẽ được xử lý ràng buộc.
* **Product (1) - ProductImage (N):** Một sản phẩm có thể hiển thị nhiều góc chụp hình ảnh phụ khác nhau.
* **Product (1) - Inventory (1):** Mỗi sản phẩm liên kết với một bản ghi tồn kho riêng biệt.
* **Product (1) - ProductReview (N):** Một sản phẩm có thể có nhiều đánh giá.
* **ProductReview (1) - ProductReviewReply (N):** Một đánh giá có thể có nhiều phản hồi từ Staff/Admin.
* **Category (1) - Post (N):** Một chuyên mục tin tức chứa nhiều bài viết khác nhau.
* **Customer (1) - CustomerAddress (N):** Một khách hàng có thể có nhiều địa chỉ nhận hàng trong sổ địa chỉ (nhà riêng, cơ quan...).
* **Customer (1) - Order (N):** Một khách hàng có thể đặt nhiều đơn hàng trong lịch sử mua sắm.
* **Customer (1) - SupportTicket (N):** Một khách hàng có thể tạo nhiều yêu cầu hỗ trợ.
* **SupportTicket (1) - SupportTicketMessage (N):** Một Ticket có thể chứa nhiều tin nhắn trao đổi.
* **Order (1) - OrderDetail (N):** Một đơn hàng chứa nhiều dòng chi tiết sản phẩm mua khác nhau.
* **Order (1) - OrderActivityLog (N):** Lưu lịch sử thao tác của đơn hàng.
* **OrderDetail (1) - OrderItemIssue (N):** Dòng chi tiết đơn hàng có thể có yêu cầu khiếu nại đổi trả.
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
