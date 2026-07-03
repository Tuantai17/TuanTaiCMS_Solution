# Hỗ Trợ Khách Hàng Implementation

## Phạm vi đã triển khai

- Thêm module giao diện hỗ trợ khách hàng dạng ticket trong `cms.frontend`.
- Thêm điều hướng mới trong sidebar tài khoản tới:
  - `/account/support`
  - `/account/support/new`
  - `/account/support/:ticketId`
- Nối nút `Liên hệ hỗ trợ` ở trang chi tiết đơn hàng sang luồng tạo ticket có kèm `orderId`.
- Thêm giao diện quản trị ticket trong `CMS.Backend`:
  - `SupportTicketController`
  - `Views/SupportTicket/Index.cshtml`
  - `Views/SupportTicket/Details.cshtml`
  - menu `Hỗ trợ khách hàng` trong layout admin

## Chi tiết kỹ thuật

### Frontend customer support

- `cms.frontend/src/services/supportService.js`
  - Dùng `localStorage` để mô phỏng dữ liệu ticket/hội thoại chạy được ngay trong giao diện.
  - Có seed dữ liệu mẫu theo từng khách hàng đăng nhập để đảm bảo trang hiển thị đủ trạng thái.
  - Hỗ trợ:
    - danh sách ticket + đếm trạng thái
    - tạo ticket mới
    - gửi tin nhắn tiếp theo
    - mở lại ticket đã đóng
    - đánh dấu đã đọc

- `SupportTicketsPage.jsx`
  - Danh sách yêu cầu, tab trạng thái, tìm kiếm, lọc danh mục, phân trang.

- `SupportNewTicketPage.jsx`
  - Form tạo yêu cầu mới.
  - Có tiêu đề, loại vấn đề, đơn hàng liên quan, sản phẩm liên quan, nội dung, ảnh, emoji, sticker.

- `SupportTicketDetailPage.jsx`
  - Giao diện hội thoại hai phía khách hàng và nhân viên hỗ trợ.
  - Có vùng nhập tin nhắn, emoji, sticker, ảnh.
  - Có trạng thái đóng và hành động mở lại yêu cầu.

- `SupportTickets.css`
  - Toàn bộ style riêng cho module support, bám layout ảnh mẫu.

### Admin support UI

- `CMS.Backend/Controllers/SupportTicketController.cs`
  - Dựng UI quản trị theo dữ liệu mẫu để tương thích kiến trúc MVC admin hiện tại.

- `CMS.Backend/Models/SupportTicketAdminViewModels.cs`
  - ViewModel cho trang danh sách và chi tiết ticket.

## Lưu ý hiện tại

- Phần frontend customer support đang chạy bằng dữ liệu mock lưu ở `localStorage` để hoàn thiện luồng giao diện và trải nghiệm hội thoại ngay trong dự án hiện tại.
- Phần admin support hiện là giao diện MVC demo theo dữ liệu mẫu, chưa nối API/backend ticket thật.
- Nếu muốn nâng cấp sang dữ liệu thật, bước tiếp theo là triển khai entity + migration + API support ticket trong `CMS.Data` và `CMS.Backend`, rồi thay `supportService.js` bằng gọi `axiosClient`.
