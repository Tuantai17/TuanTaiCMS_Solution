# ORDER HISTORY CHANGELOG

## Mục tiêu triển khai
- Xây dựng lại trang `Lịch sử mua hàng` cho khách hàng đã đăng nhập theo giao diện đồng bộ với khu vực tài khoản hiện có.
- Kết nối dữ liệu thật từ backend, không lọc đơn hàng của tài khoản khác ở frontend.
- Bổ sung API backend an toàn hơn để backend tự xác định khách hàng từ token phiên.

## File đã sửa
- `CMS.Backend/Controllers/AuthController.cs`
- `CMS.Backend/Controllers/OrdersController.cs`
- `cms.frontend/src/App.js`
- `cms.frontend/src/api/axiosClient.js`
- `cms.frontend/src/components/Header.jsx`
- `cms.frontend/src/pages/Checkout.jsx`
- `cms.frontend/src/pages/Login.jsx`
- `cms.frontend/src/pages/OrderHistory.jsx`
- `cms.frontend/src/services/orderService.js`

## File mới
- `CMS.Backend/Helpers/CustomerSessionTokenHelper.cs`
- `cms.frontend/src/assets/css/OrderHistory.css`
- `cms.frontend/src/components/account/AccountSidebar.jsx`
- `cms.frontend/src/pages/OrderDetailPage.jsx`
- `cms.frontend/src/utils/customerSession.js`
- `cms.frontend/src/utils/orderStatus.js`

## Route
- Lịch sử mua hàng chính: `/account/orders`
- Chi tiết đơn hàng: `/account/orders/:id`
- Giữ alias cũ để không phá liên kết đang tồn tại: `/order-history`

## API
- Đăng nhập khách hàng trả thêm token phiên:
  - `POST /api/Auth/CustomerLogin`
- Danh sách đơn hàng của khách đang đăng nhập:
  - `GET /api/Orders/my`
- Chi tiết đơn hàng của khách đang đăng nhập:
  - `GET /api/Orders/my/{id}`
- API cũ vẫn được giữ nguyên để tránh ảnh hưởng luồng hiện tại:
  - `GET /api/Orders/customer/{customerId}`

## Query filter
- `status`
- `keyword`
- `fromDate`
- `toDate`
- `page`
- `pageSize`

## Phân trang
- Backend phân trang trực tiếp ở `GET /api/Orders/my`.
- Response:
```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 0,
  "totalPages": 0
}
```

## Trạng thái đơn hàng
- Dùng đúng trạng thái đang có trong backend:
  - `0`: Chờ duyệt
  - `1`: Đang giao
  - `2`: Hoàn thành
  - `3`: Đã hủy
- Frontend gom mapping vào `cms.frontend/src/utils/orderStatus.js`.

## Chức năng xem chi tiết
- Nút `Xem chi tiết` điều hướng đến `/account/orders/:id`.
- Backend kiểm tra token phiên và chỉ trả đơn hàng có `CustomerId` khớp với khách đang đăng nhập.
- Nếu thay `id` trên URL sang đơn của tài khoản khác, API trả `404`.

## Chức năng mua lại
- Chưa bật nút `Mua lại`.
- Lý do: backend hiện chưa có API nghiệp vụ kiểm tra tồn kho, sản phẩm còn bán và thêm lại giỏ hàng theo đơn cũ.
- Trang đang ẩn chức năng này để tránh hiển thị tính năng giả.

## Loading
- Có skeleton/loading khi tải danh sách đơn hàng.
- Có trạng thái loading riêng cho trang chi tiết đơn hàng.

## Empty State
- Không có đơn hàng: hiển thị CTA `Tiếp tục mua sắm`.
- Không có kết quả theo bộ lọc: hiển thị CTA `Xóa bộ lọc`.

## Responsive
- Desktop: sidebar trái + bảng đơn hàng.
- Tablet: filter co lại theo grid, giữ layout 2 cột.
- Mobile: danh sách đơn hàng chuyển sang card, tab trạng thái cuộn ngang.

## Bảo mật và phân quyền
- Frontend lưu token phiên khách hàng nhận từ `CustomerLogin`.
- `axiosClient` tự gắn `Authorization: Bearer <token>` cho request.
- Backend không nhận `userId` từ query để xác định chủ đơn hàng ở API mới.
- Helper `CustomerSessionTokenHelper` xác thực token, lấy `customerId`, kiểm tra hạn dùng.

## Cách kiểm tra
1. Đăng nhập tài khoản khách hàng.
2. Mở `/account/orders`.
3. Thử lọc theo trạng thái, từ khóa, khoảng ngày, phân trang.
4. Mở chi tiết một đơn bằng nút `Xem chi tiết`.
5. Thử sửa URL sang `id` của đơn khác để kiểm tra chặn truy cập.
6. Đăng xuất rồi truy cập lại `/account/orders` để kiểm tra redirect về `/login`.

## Những phần còn thiếu
- Backend chưa lưu phương thức thanh toán theo đơn, nên UI hiển thị `Không xác định`.
- Backend chưa có API `reorder`.
- Build frontend vẫn còn warning cũ ở các file ngoài phạm vi tính năng này như `Footer.jsx`, `AddressesPage.jsx`, `Home.jsx`, `PostList.jsx`, `Register.jsx`.
- Build backend project thật tại thư mục `bin` đang bị process `CMS.Backend (10568)` khóa file; kiểm tra compile đã được xác nhận bằng build sang output tạm `.codex-temp/backend-build`.
