Hãy phân tích toàn bộ source code hiện tại và triển khai hoàn chỉnh trang “Lịch sử mua hàng” dành cho người dùng đã đăng nhập, dựa theo hình ảnh giao diện mẫu tôi cung cấp.

Không tạo project mới. Hãy chỉnh sửa trực tiếp trong dự án hiện tại.

Mục tiêu là tạo trang lịch sử đơn hàng có giao diện đồng bộ với trang Profile, Sổ địa chỉ và Đổi mật khẩu hiện tại, đồng thời kết nối dữ liệu thật từ backend.

==================================================

1. YÊU CẦU QUAN TRỌNG
   ==================================================

- Đọc toàn bộ cấu trúc dự án trước khi chỉnh sửa.
- Không làm mất logic hiện tại.
- Không làm hỏng Header, Footer, Sidebar tài khoản, đăng nhập, đăng xuất, giỏ hàng, thanh toán và đặt hàng.
- Không xóa hoặc đổi tên component, service, controller, model hay API đang hoạt động.
- Không tạo dữ liệu đơn hàng giả để thay thế dữ liệu thật.
- Không hard-code userId.
- Chỉ hiển thị đơn hàng của người dùng đang đăng nhập.
- Không cho người dùng xem đơn hàng của tài khoản khác.
- Không chỉ lọc đơn hàng ở frontend.
- Backend phải kiểm tra quyền sở hữu đơn hàng.
- Không tự ý thay đổi database nếu chưa kiểm tra cấu trúc.
- Không tạo route hoặc API trùng với chức năng đã có.
- Giao diện sử dụng tiếng Việt.
- Phải responsive trên desktop, tablet và mobile.
- Không để lỗi Console, lỗi API hoặc lỗi build.

==================================================
2. KIỂM TRA SOURCE TRƯỚC KHI TRIỂN KHAI
===========================================

Trước khi viết code, hãy kiểm tra chính xác:

- Framework frontend đang sử dụng.
- Cấu trúc thư mục frontend.
- File Router.
- Component Header.
- Component Footer.
- Component Sidebar tài khoản.
- Trang Profile hiện tại.
- Trang Sổ địa chỉ.
- Trang Đổi mật khẩu.
- Auth Context, Redux, Zustand, Pinia, Vuex hoặc Store hiện tại.
- Cách lưu token, cookie hoặc session.
- Axios instance hoặc API client hiện tại.
- Middleware hoặc route guard.
- Backend framework.
- Model User hoặc Customer.
- Model Order.
- Model OrderDetail.
- Model Product.
- Model Payment nếu có.
- Controller, Service và Repository liên quan đến đơn hàng.
- Danh sách trạng thái đơn hàng thực tế.
- API lấy đơn hàng của người dùng hiện tại.
- API xem chi tiết đơn hàng.
- API mua lại đơn hàng nếu đã có.
- Cách phân trang hiện tại của backend.
- Component Toast, Loading, Pagination, Modal và Badge đang có.

Không tự phỏng đoán tên file, tên Model, tên thuộc tính hoặc endpoint.

Phải đọc code thực tế rồi mới triển khai.

==================================================
3. ROUTE TRANG LỊCH SỬ MUA HÀNG
==================================

Route mong muốn:

/account/orders

Hoặc:

/order-history

Hãy giữ đúng convention Router hiện tại của dự án.

Khi người dùng bấm “Lịch sử mua hàng” trong Sidebar:

- Điều hướng đúng đến trang lịch sử mua hàng.
- Mục “Lịch sử mua hàng” phải ở trạng thái active.
- Không reload toàn bộ website nếu dự án đang dùng SPA Router.
- Không sử dụng href="#".

Nếu dùng React Router, ưu tiên:

- NavLink.
- Link.
- useNavigate.
- ProtectedRoute nếu dự án đang có.

Nếu dùng Vue Router, ưu tiên:

- RouterLink.
- router.push.
- Navigation Guard nếu dự án đang có.

Người chưa đăng nhập truy cập trang phải được chuyển đến trang đăng nhập.

==================================================
4. PHONG CÁCH GIAO DIỆN
=========================

Thiết kế bám sát ảnh mẫu:

- Thanh thông tin trên cùng màu xanh đậm.
- Header chính màu đỏ.
- Thanh menu điều hướng màu đỏ đậm.
- Nội dung nền xám rất nhạt.
- Card màu trắng.
- Bo góc từ 12px đến 16px.
- Border mảnh.
- Box-shadow nhẹ.
- Màu chủ đạo đỏ.
- Màu trạng thái sử dụng xanh lá, xanh dương, cam, vàng, đỏ và xám.
- Font chữ đồng bộ với website hiện tại.
- Khoảng cách rõ ràng và dễ đọc.
- Hover nhẹ từ 0.2s đến 0.3s.
- Không sử dụng quá nhiều màu gây rối mắt.
- Không làm thay đổi giao diện các trang khác.

==================================================
5. BỐ CỤC TRANG
=================

Giữ nguyên Header và Footer hiện tại.

Phần nội dung chính gồm hai cột:

Cột trái:

- Sidebar tài khoản.
- Chiều rộng khoảng 260px đến 300px trên desktop.
- Card màu trắng.
- Có thể sticky khi cuộn.

Cột phải:

- Nội dung lịch sử mua hàng.
- Chiếm phần chiều rộng còn lại.
- Card nền trắng.
- Có tiêu đề, bộ lọc, tìm kiếm, danh sách đơn hàng và phân trang.

Bố cục gợi ý:

---

| Header website                                    |
-----------------------------------------------------

| Sidebar tài khoản | Lịch sử mua hàng              |
|                   |                                |
| Avatar            | Bộ lọc trạng thái             |
| Tên người dùng    | Tìm kiếm + lọc thời gian      |
| Email             |                                |
| Thành viên        | Danh sách đơn hàng            |
|                   |                                |
| Thông tin TK      | Phân trang                     |
| Sổ địa chỉ        |                                |
| Lịch sử active    |                                |
| Đơn hàng của tôi  |                                |
| Yêu thích         |                                |
| Đổi mật khẩu      |                                |
| Thông báo         |                                |
| Đăng xuất         |                                |
---------------------------------------------------------

| Footer website                                    |
-----------------------------------------------------

==================================================
6. SIDEBAR TÀI KHOẢN
======================

Sidebar gồm:

1. Thông tin tài khoản
2. Sổ địa chỉ
3. Lịch sử mua hàng
4. Đơn hàng của tôi
5. Sản phẩm yêu thích
6. Đổi mật khẩu
7. Thông báo
8. Đăng xuất

Yêu cầu:

- “Lịch sử mua hàng” active với nền đỏ và chữ trắng.
- Mỗi mục có icon phù hợp.
- Có hover rõ ràng.
- Hiển thị avatar, họ tên, email và nhãn thành viên của người đang đăng nhập.
- Không hard-code thông tin tài khoản.
- Nếu không có avatar thì dùng ảnh mặc định.
- Không tạo Sidebar mới nếu dự án đã có component Sidebar dùng chung.
- Badge thông báo lấy dữ liệu thật nếu hệ thống hỗ trợ.
- Đăng xuất phải dùng đúng logic logout hiện tại.

==================================================
7. TIÊU ĐỀ TRANG
===================

Phần đầu nội dung hiển thị:

Tiêu đề:

“Lịch sử mua hàng”

Mô tả:

“Theo dõi tất cả đơn hàng bạn đã mua.”

Có thể thêm icon lịch sử hoặc đơn hàng cạnh tiêu đề.

==================================================
8. BỘ LỌC TRẠNG THÁI ĐƠN HÀNG
====================================

Tạo các tab hoặc nút lọc:

- Tất cả.
- Chờ xác nhận.
- Đang xử lý.
- Đang giao.
- Hoàn thành.
- Đã hủy.

Yêu cầu:

- Tab đang chọn có màu nổi bật.
- “Tất cả” dùng màu đỏ chủ đạo.
- Chờ xác nhận có thể dùng màu vàng.
- Đang xử lý dùng màu cam hoặc xanh dương.
- Đang giao dùng màu xanh dương.
- Hoàn thành dùng màu xanh lá.
- Đã hủy dùng màu xám hoặc đỏ.
- Mỗi tab có icon phù hợp.
- Khi đổi tab phải tải đúng danh sách đơn hàng.
- Không chỉ ẩn hiện dữ liệu ở frontend nếu backend hỗ trợ lọc.
- Nên truyền trạng thái bằng query parameter cho API.
- Đổi bộ lọc không làm mất các điều kiện tìm kiếm khác.
- Không reload toàn bộ trang nếu đang dùng SPA.

Tên trạng thái phải sử dụng đúng dữ liệu thực tế của backend.

Không hard-code trạng thái khác với database hoặc enum hiện tại.

==================================================
9. TÌM KIẾM VÀ LỌC THỜI GIAN
=================================

Tạo khu vực tìm kiếm gồm:

- Ô tìm kiếm theo mã đơn hàng.
- Bộ lọc khoảng thời gian.
- Có thể thêm nút “Đặt lại”.

Placeholder tìm kiếm:

“Tìm kiếm theo mã đơn hàng...”

Bộ lọc thời gian có thể gồm:

- Từ ngày.
- Đến ngày.

Hoặc dùng Date Range Picker nếu project đã có thư viện phù hợp.

Yêu cầu:

- Có debounce khoảng 300ms đến 500ms nếu tìm kiếm tự động.
- Hoặc tìm khi người dùng nhấn Enter.
- Không gọi API sau mỗi phím gõ nếu chưa debounce.
- Validate ngày bắt đầu không lớn hơn ngày kết thúc.
- Không cho chọn ngày kết thúc nhỏ hơn ngày bắt đầu.
- Nút đặt lại phải xóa toàn bộ bộ lọc.
- Khi lọc mới phải quay về trang đầu.
- Giữ bộ lọc khi chuyển trang.
- Có thể đồng bộ bộ lọc lên query string để reload trang không bị mất trạng thái.

==================================================
10. DANH SÁCH ĐƠN HÀNG
==========================

Trên desktop hiển thị dạng bảng.

Các cột:

- Mã đơn hàng.
- Ngày đặt.
- Sản phẩm.
- Tổng tiền.
- Phương thức thanh toán.
- Trạng thái.
- Thao tác.

Mỗi dòng đơn hàng hiển thị:

- Mã đơn.
- Ngày và giờ đặt hàng.
- Ảnh đại diện của sản phẩm đầu tiên.
- Tên sản phẩm.
- Số lượng sản phẩm.
- Nếu đơn có nhiều sản phẩm, hiển thị:
  “Và X sản phẩm khác”.
- Tổng tiền.
- Phương thức thanh toán.
- Trạng thái.
- Nút “Xem chi tiết”.
- Nút “Mua lại” nếu đủ điều kiện.

Yêu cầu:

- Dữ liệu phải lấy từ backend.
- Không hard-code đơn hàng mẫu.
- Không hiển thị đơn hàng của người dùng khác.
- Không để NullReference nếu sản phẩm hoặc ảnh bị null.
- Nếu ảnh sản phẩm bị lỗi thì dùng ảnh mặc định.
- Tên sản phẩm dài phải dùng ellipsis hoặc giới hạn dòng.
- Giá tiền định dạng theo Việt Nam, ví dụ:
  899.000 ₫
- Ngày giờ định dạng dễ đọc.
- Bảng có hover nhẹ.
- Header bảng rõ ràng.
- Không làm vỡ layout khi nội dung dài.
- Không tải toàn bộ chi tiết tất cả đơn hàng nếu API danh sách chỉ cần thông tin tóm tắt.

==================================================
11. TRẠNG THÁI ĐƠN HÀNG
============================

Hiển thị trạng thái bằng Badge.

Gợi ý màu:

- Chờ xác nhận: vàng.
- Đang xử lý: cam.
- Đang giao: xanh dương.
- Hoàn thành: xanh lá.
- Đã hủy: xám hoặc đỏ.
- Hoàn tiền: tím nếu hệ thống có.
- Không xác định: xám.

Không so sánh trạng thái bằng chuỗi rải rác trong nhiều component.

Hãy tạo một helper hoặc mapping chung, ví dụ:

- label.
- className.
- icon.

Mapping phải dựa trên trạng thái thực tế của backend.

Nếu backend dùng số hoặc enum thì chuyển đổi đúng cách.

==================================================
12. PHƯƠNG THỨC THANH TOÁN
==============================

Hiển thị phương thức thanh toán theo dữ liệu thật, ví dụ:

- COD.
- MoMo.
- VNPay.
- Thẻ Visa.
- Chuyển khoản.
- Ví điện tử.

Có thể hiển thị:

- Tên phương thức.
- Icon hoặc logo nhỏ nếu asset đã có.
- Bốn số cuối của thẻ nếu backend đã trả về thông tin an toàn.

Không hiển thị:

- Số thẻ đầy đủ.
- Mã CVV.
- Token thanh toán.
- Thông tin nhạy cảm.

Nếu phương thức không xác định, hiển thị:

“Không xác định”

==================================================
13. NÚT XEM CHI TIẾT
======================

Khi bấm “Xem chi tiết”:

- Điều hướng đến trang chi tiết đơn hàng hiện tại.
- Không tạo trang trùng nếu dự án đã có.
- Route gợi ý:

/account/orders/{id}

Hoặc:

/orders/{id}

Hoặc route hiện có trong dự án.

Backend phải kiểm tra:

- Đơn hàng có thuộc người dùng đang đăng nhập hay không.
- Không cho xem đơn hàng bằng cách thay đổi ID trên URL.

Không chỉ kiểm tra quyền ở frontend.

==================================================
14. NÚT MUA LẠI
=================

Hiển thị nút “Mua lại” khi nghiệp vụ cho phép, ví dụ:

- Đơn hàng đã hoàn thành.
- Đơn hàng đã hủy nếu sản phẩm vẫn còn bán.
- Không hiển thị nếu đơn đang xử lý hoặc đang giao, tùy nghiệp vụ.

Khi bấm “Mua lại”:

- Lấy danh sách sản phẩm từ đơn cũ.
- Kiểm tra sản phẩm còn tồn tại.
- Kiểm tra sản phẩm còn bán.
- Kiểm tra số lượng tồn kho.
- Thêm các sản phẩm hợp lệ vào giỏ hàng.
- Không tự động tạo đơn hàng mới ngay.
- Thông báo các sản phẩm không thể mua lại.
- Không thêm số lượng vượt tồn kho.
- Hiển thị Toast thành công.
- Cập nhật Cart Store hiện tại.

Nếu backend chưa hỗ trợ mua lại:

- Không tạo dữ liệu giả.
- Có thể ẩn nút.
- Hoặc ghi chú rõ phần API cần bổ sung.

==================================================
15. PHÂN TRANG
===============

Danh sách phải có phân trang.

Hiển thị:

- Trang hiện tại.
- Tổng số trang.
- Nút trang trước.
- Nút trang sau.
- Một số trang gần trang hiện tại.
- Có thể hiển thị dấu “...”.

Ví dụ:

< 1 2 3 ... 6 >

Yêu cầu:

- Khi chuyển trang gọi đúng API.
- Giữ nguyên trạng thái lọc và tìm kiếm.
- Không quay toàn bộ trang lên đầu nếu không cần.
- Có thể cuộn đến đầu danh sách đơn hàng.
- Không tải tất cả đơn hàng về frontend rồi mới phân trang.
- Backend phải phân trang nếu dữ liệu có thể lớn.

Có thể hiển thị dòng:

“Hiển thị 1–10 trong tổng số 24 đơn hàng”

Số liệu phải lấy từ response API.

==================================================
16. API DANH SÁCH ĐƠN HÀNG
==============================

Trước khi tạo API mới, hãy kiểm tra API hiện tại.

Endpoint tham khảo:

GET /api/account/orders

Query parameter tham khảo:

- status.
- keyword.
- fromDate.
- toDate.
- page.
- pageSize.

Ví dụ:

GET /api/account/orders?status=completed&keyword=MKD1005&page=1&pageSize=10

Đây chỉ là gợi ý.

Phải sử dụng đúng convention backend hiện tại.

Backend phải:

- Xác thực người dùng.
- Lấy userId từ token, cookie, session hoặc claims.
- Không nhận userId tùy ý từ query để xác định chủ sở hữu.
- Chỉ truy vấn đơn hàng của người dùng hiện tại.
- Áp dụng lọc trực tiếp trong database.
- Sắp xếp đơn hàng mới nhất trước.
- Phân trang ở backend.
- Chỉ select dữ liệu cần thiết.
- Không lấy toàn bộ bảng về RAM rồi mới lọc.
- Xử lý null an toàn.
- Không trả thông tin nhạy cảm.

Response gợi ý:

{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 24,
  "totalPages": 3
}

Không bắt buộc dùng đúng cấu trúc này nếu dự án đã có response format riêng.

==================================================
17. API CHI TIẾT ĐƠN HÀNG
=============================

Endpoint tham khảo:

GET /api/account/orders/{id}

Backend phải:

- Xác thực người dùng.
- Kiểm tra đơn hàng thuộc người dùng hiện tại.
- Trả về 404 hoặc 403 theo convention nếu không thuộc quyền sở hữu.
- Không trả thông tin nội bộ không cần thiết.
- Không để người dùng xem đơn hàng của tài khoản khác bằng cách đổi ID.

==================================================
18. SERVICE FRONTEND
====================

Tái sử dụng Axios instance hoặc API client hiện tại.

Có thể bổ sung các hàm:

- getMyOrders(params)
- getMyOrderDetail(id)
- reorder(id)

Đây chỉ là gợi ý.

Không tạo thêm Axios instance nếu dự án đã có.

Không hard-code base URL.

Request phải tự động gửi:

- Authorization header nếu dùng Bearer token.
- Cookie nếu dùng session hoặc HttpOnly cookie.
- withCredentials nếu hệ thống yêu cầu.

Phải xử lý:

- Token hết hạn.
- 401 Unauthorized.
- 403 Forbidden.
- Mất kết nối.
- Server error.
- Request bị hủy khi component unmount nếu cần.

==================================================
19. STATE CỦA TRANG
====================

Trang cần quản lý các trạng thái:

- orders.
- loading.
- error.
- currentPage.
- pageSize.
- totalItems.
- totalPages.
- selectedStatus.
- keyword.
- fromDate.
- toDate.
- reorderingOrderId.

Không tạo nhiều state trùng lặp.

Khi thay đổi:

- Trạng thái đơn hàng.
- Từ khóa.
- Khoảng ngày.
- Kích thước trang.

Phải đưa currentPage về 1.

==================================================
20. LOADING
===========

Khi đang tải dữ liệu:

- Hiển thị Skeleton Table hoặc Spinner.
- Không để khu vực trắng.
- Không hiển thị dữ liệu của bộ lọc cũ như thể là dữ liệu mới.
- Có thể giữ dữ liệu cũ nhưng phải hiển thị trạng thái loading rõ ràng.
- Không khóa toàn bộ trang nếu chỉ đang đổi trang hoặc bộ lọc.

Khi bấm mua lại:

- Chỉ disable nút của đơn hàng đang xử lý.
- Hiển thị:
  “Đang thêm...”
- Không disable toàn bộ danh sách.

==================================================
21. EMPTY STATE
===============

Nếu chưa có đơn hàng, hiển thị Empty State gồm:

- Icon hộp hàng hoặc giỏ hàng.
- Tiêu đề:
  “Bạn chưa có đơn hàng nào”
- Mô tả:
  “Hãy khám phá sản phẩm và bắt đầu mua sắm.”
- Nút:
  “Tiếp tục mua sắm”

Nút phải điều hướng về trang sản phẩm hoặc trang chủ.

Nếu không có kết quả do bộ lọc:

- Tiêu đề:
  “Không tìm thấy đơn hàng phù hợp”
- Mô tả:
  “Hãy thử thay đổi bộ lọc hoặc từ khóa tìm kiếm.”
- Nút:
  “Xóa bộ lọc”

Không hiển thị bảng trống lớn.

==================================================
22. XỬ LÝ LỖI
================

Xử lý các lỗi:

- Không tải được danh sách đơn hàng.
- Token hết hạn.
- Người dùng chưa đăng nhập.
- API không phản hồi.
- Mất kết nối.
- Không có quyền truy cập.
- Lỗi server.
- Đơn hàng không tồn tại.
- Mua lại thất bại.

Thông báo thân thiện:

- “Không thể tải lịch sử mua hàng. Vui lòng thử lại.”
- “Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.”
- “Không tìm thấy đơn hàng.”
- “Bạn không có quyền xem đơn hàng này.”
- “Không thể thêm lại sản phẩm vào giỏ hàng.”

Không hiển thị stack trace hoặc lỗi kỹ thuật cho người dùng.

==================================================
23. RESPONSIVE
==============

Desktop:

- Sidebar bên trái.
- Bảng đơn hàng bên phải.
- Bộ lọc nằm trên một hoặc hai hàng gọn gàng.

Tablet:

- Sidebar có thể thu nhỏ hoặc chuyển thành Drawer.
- Bảng có thể cuộn ngang trong phạm vi Card.
- Không xuất hiện thanh cuộn ngang toàn trang.

Mobile:

- Sidebar chuyển thành nút hoặc Drawer “Tài khoản của tôi”.
- Bộ lọc trạng thái có thể cuộn ngang.
- Ô tìm kiếm và bộ lọc ngày hiển thị một cột.
- Danh sách đơn hàng chuyển từ bảng thành Card.
- Mỗi Card hiển thị:
  - Mã đơn.
  - Ngày đặt.
  - Ảnh sản phẩm.
  - Tên sản phẩm.
  - Tổng tiền.
  - Trạng thái.
  - Nút xem chi tiết.
  - Nút mua lại.
- Nút thao tác rộng và dễ bấm.
- Không cắt nội dung.
- Header không bị vỡ.
- Không có thanh cuộn ngang toàn trang.

==================================================
24. ACCESSIBILITY
=================

- Input phải có label hoặc aria-label.
- Nút phải dùng thẻ button.
- Không dùng div giả làm nút.
- Focus state rõ ràng.
- Có thể thao tác bằng bàn phím.
- Badge trạng thái không chỉ phân biệt bằng màu mà phải có text.
- Icon phải có aria-hidden hoặc aria-label phù hợp.
- Bảng phải có header semantic.
- Nút chuyển trang phải có nhãn rõ ràng.
- Trạng thái loading nên có aria-live nếu phù hợp.

==================================================
25. CSS VÀ COMPONENT
=====================

Không viết quá nhiều inline style.

Tái sử dụng design system, component và CSS hiện tại.

Cấu trúc tham khảo với React:

src/pages/account/OrderHistoryPage.jsx
src/components/account/AccountSidebar.jsx
src/components/orders/OrderHistoryFilters.jsx
src/components/orders/OrderHistoryTable.jsx
src/components/orders/OrderHistoryCard.jsx
src/components/orders/OrderStatusBadge.jsx
src/components/common/Pagination.jsx
src/services/orderService.js
src/styles/order-history.css

Cấu trúc tham khảo với Vue:

src/views/account/OrderHistoryView.vue
src/components/account/AccountSidebar.vue
src/components/orders/OrderHistoryFilters.vue
src/components/orders/OrderHistoryTable.vue
src/components/orders/OrderHistoryCard.vue
src/components/orders/OrderStatusBadge.vue
src/services/orderService.js
src/assets/styles/order-history.css

Đây chỉ là gợi ý.

Phải điều chỉnh theo cấu trúc thật của dự án.

Không tách quá nhiều component nhỏ không cần thiết.

CSS phải:

- Có phạm vi rõ ràng.
- Không ghi đè style global.
- Không dùng !important tràn lan.
- Không làm hỏng các trang khác.
- Giữ đồng bộ với Profile, Sổ địa chỉ và Đổi mật khẩu.

==================================================
26. BẢO MẬT
=============

Phải đảm bảo:

- Chỉ người dùng đã đăng nhập mới truy cập được.
- Backend lấy userId từ token, cookie, session hoặc claims.
- Không nhận userId từ frontend để xác định đơn hàng.
- Không cho xem đơn hàng của tài khoản khác.
- Không cho mua lại từ đơn hàng của tài khoản khác.
- Không trả thông tin thanh toán nhạy cảm.
- Không trả password, token hoặc thông tin nội bộ.
- Không log token.
- Không log dữ liệu thanh toán nhạy cảm.
- Validate tất cả query parameter.
- Giới hạn pageSize hợp lý để tránh request quá lớn.
- Không để thay đổi ID trên URL vượt quyền sở hữu.

==================================================
27. KIỂM TRA SAU KHI TRIỂN KHAI
=================================

Kiểm tra tối thiểu các trường hợp:

1. Người chưa đăng nhập truy cập trang.
2. Người dùng chưa có đơn hàng.
3. Người dùng có nhiều đơn hàng.
4. Lọc trạng thái Tất cả.
5. Lọc Chờ xác nhận.
6. Lọc Đang xử lý.
7. Lọc Đang giao.
8. Lọc Hoàn thành.
9. Lọc Đã hủy.
10. Tìm kiếm đúng mã đơn.
11. Tìm kiếm mã không tồn tại.
12. Lọc theo khoảng ngày.
13. Chọn khoảng ngày không hợp lệ.
14. Chuyển trang.
15. Giữ bộ lọc khi chuyển trang.
16. Xem chi tiết đơn hàng.
17. Thử xem đơn hàng của người khác bằng cách đổi ID.
18. Bấm mua lại.
19. Sản phẩm mua lại đã hết hàng.
20. Sản phẩm mua lại đã bị xóa.
21. Token hết hạn.
22. API mất kết nối.
23. Hiển thị trên mobile.
24. Sidebar active đúng.
25. Không có lỗi Console.
26. Không có request trùng.
27. Không trả dữ liệu thanh toán nhạy cảm.

==================================================
28. TIÊU CHÍ HOÀN THÀNH
===========================

Trang chỉ được xem là hoàn thành khi:

- Route lịch sử mua hàng hoạt động.
- Route được bảo vệ.
- Sidebar hiển thị đúng.
- “Lịch sử mua hàng” active màu đỏ.
- Danh sách lấy từ backend.
- Chỉ hiển thị đơn hàng của người dùng hiện tại.
- Bộ lọc trạng thái hoạt động.
- Tìm kiếm hoạt động.
- Lọc thời gian hoạt động.
- Phân trang hoạt động.
- Badge trạng thái đúng.
- Xem chi tiết hoạt động.
- Mua lại hoạt động hoặc được ẩn nếu backend chưa hỗ trợ.
- Loading hoạt động.
- Empty State hoạt động.
- Error State hoạt động.
- Responsive tốt.
- Không hard-code đơn hàng.
- Không hard-code userId.
- Không có lỗi Console.
- Không có lỗi API.
- Không có lỗi build.
- Không làm hỏng các chức năng đang có.

==================================================
29. KẾT QUẢ CẦN TRẢ VỀ
===========================

Sau khi triển khai, hãy trả về:

1. Danh sách file đã chỉnh sửa.
2. Danh sách file mới đã tạo.
3. Route lịch sử mua hàng.
4. API đã sử dụng hoặc bổ sung.
5. Query parameter hỗ trợ.
6. Cấu trúc response phân trang.
7. Cách lấy người dùng đang đăng nhập.
8. Cách bảo vệ quyền sở hữu đơn hàng.
9. Cách ánh xạ trạng thái đơn hàng.
10. Cách xử lý tìm kiếm và bộ lọc.
11. Cách xử lý phân trang.
12. Cách xử lý chức năng mua lại.
13. Những phần chưa thể hoàn thành do backend chưa hỗ trợ.
14. Hướng dẫn chạy và kiểm tra.
15. Không chỉ giải thích; hãy chỉnh sửa code trực tiếp trong source.

Tạo thêm file:

ORDER_HISTORY_CHANGELOG.md

Nội dung file gồm:

- Mục tiêu triển khai.
- File đã sửa.
- File mới.
- Route.
- API.
- Query filter.
- Phân trang.
- Trạng thái đơn hàng.
- Chức năng xem chi tiết.
- Chức năng mua lại.
- Loading.
- Empty State.
- Responsive.
- Bảo mật và phân quyền.
- Cách kiểm tra.
- Những phần còn thiếu.

Hãy bắt đầu bằng việc đọc Router, Auth Store, Sidebar tài khoản, Model Order, OrderDetail, Product, API đơn hàng và trang chi tiết đơn hàng hiện tại. Sau khi xác định đúng cấu trúc mới được tiến hành chỉnh sửa code.
