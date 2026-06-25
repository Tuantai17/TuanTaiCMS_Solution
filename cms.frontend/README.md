# CMS Frontend - TuanTaiCMS

Frontend của dự án **TuanTaiCMS** được xây dựng bằng thư viện **ReactJS (phiên bản 19)** nhằm tối ưu hóa giao diện hiển thị cho người dùng / khách hàng cuối (Customer). Phần này kết nối chặt chẽ với backend **ASP.NET Core 8.0 Web API** qua giao thức HTTP/JSON để hiển thị danh sách sản phẩm, tin tức, giỏ hàng, đặt hàng, quản lý lịch sử mua sắm, quản lý hồ sơ cá nhân và khôi phục mật khẩu tự động qua OTP email.

---

## 🎓 THÔNG TIN SINH VIÊN thực hiện
- **Họ và tên:** Nguyễn Tuấn Tài
- **Mã Số Sinh Viên (MSSV):** 2123110166
- **Lớp:** CCQ2311E
- **Môn học:** Chuyên đề ASP.NET

---

## 1. Tổng quan phân quyền hệ thống

Hệ thống được thiết kế phân chia thành **3 nhóm quyền / vai trò** rõ ràng:

| Nhóm quyền | Khu vực sử dụng | Có vào trang Quản trị (Admin) không? | Chức năng chính |
| :--- | :--- | :---: | :--- |
| **Admin** | Backend MVC `CMS.Backend` | **Có** | Quản trị toàn quyền hệ thống, quản lý tài khoản nội bộ (Admin/Staff), sản phẩm, bài viết, khách hàng và đơn hàng |
| **Staff** | Backend MVC `CMS.Backend` | **Có** | Nhân viên nghiệp vụ quản lý sản phẩm, đơn hàng, bài viết. Không được phép truy cập mục quản lý tài khoản hệ thống |
| **User / Customer** | Frontend ReactJS `cms.frontend` | **Không** | Xem sản phẩm, đọc tin tức, đăng ký/đăng nhập, mua hàng, xem lịch sử mua sắm, quản lý hồ sơ và tự khôi phục mật khẩu qua OTP |

> [!IMPORTANT]
> **Lưu ý quan trọng:**
> - Tài khoản **Admin** và **Staff** được lưu trữ tại bảng `Users` trong cơ sở dữ liệu.
> - Tài khoản **User / Customer** được lưu trữ tại bảng `Customers` trong cơ sở dữ liệu.
> - Khách hàng (Customer) hoàn toàn không thể đăng nhập hoặc truy cập được vào hệ thống quản trị Admin backend.

---

## 2. Các chức năng chính của Frontend

Giao diện Frontend ReactJS phục vụ toàn bộ các nhu cầu của **Customer**:
- **Trang chủ (Home):** Hiển thị banner chuyển động (Swiper slider), danh sách sản phẩm mới về (New Products), sản phẩm bán chạy (Best Sellers), tin tức nổi bật.
- **Trang cửa hàng (Shop):** Tìm kiếm sản phẩm theo từ khóa, lọc theo danh mục sản phẩm, lọc theo khoảng giá, sắp xếp theo tên hoặc sản phẩm bán chạy.
- **Trang chi tiết sản phẩm (Product Detail):** Xem thông tin mô tả chi tiết, hình ảnh phóng to, trạng thái tồn kho thực tế, xem các hình ảnh phụ liên quan và thêm sản phẩm vào giỏ hàng.
- **Trang giỏ hàng (Cart):** Xem danh sách sản phẩm đã chọn, thay đổi số lượng trực tiếp, tính toán tạm tính, tiền ship dự kiến và tổng tiền thanh toán.
- **Trang thanh toán (Checkout):** Nhập thông tin người nhận hàng (Họ tên, Số điện thoại, Địa chỉ giao hàng) và Ghi chú đơn hàng.
- **Trang hồ sơ cá nhân (Profile):** Xem, chỉnh sửa thông tin cá nhân và cập nhật ảnh đại diện.
- **Sổ địa chỉ (Addresses):** Quản lý danh sách địa chỉ giao hàng để thanh toán nhanh chóng hơn.
- **Đổi mật khẩu (Change Password):** Chức năng bảo mật để khách hàng tự thay đổi mật khẩu của mình.
- **Đăng ký (Register) & Đăng nhập (Login):** Xác thực tài khoản khách hàng.
- **Lịch sử đơn hàng (Order History & Detail):** Xem danh sách đơn hàng đã mua, ngày giờ đặt, trạng thái giao nhận và thông tin chi tiết từng mặt hàng trong đơn.
- **Quên mật khẩu (ForgotPassword):** Khôi phục mật khẩu an toàn qua 3 bước xác thực mã OTP gửi về Email cá nhân.
- **Blog / Cẩm nang:** Xem danh sách bài viết theo chuyên mục, tìm kiếm bài viết và đọc chi tiết nội dung tin tức có kèm định dạng rich text.

---

## 3. Công nghệ và Thư viện sử dụng

- **React Core:** `React 19.2.6` & `React-DOM 19.2.6`
- **Định tuyến đường dẫn (Routing):** `React Router DOM v7` (quản lý route mượt mà không load lại trang)
- **Kết nối API (HTTP Client):** `Axios` (kết nối và cấu hình interceptors xử lý lỗi tập trung)
- **Hiệu ứng chuyển động (Carousels):** `Swiper v12` (sử dụng cho Banner slider trang chủ)
- **Thiết kế UI:** Vanilla CSS tùy chỉnh kết hợp Bootstrap 4/5, Font Awesome v6 cho bộ icon cao cấp, Google Fonts (Outfit Font) mang lại thẩm mỹ tinh tế.
- **Trạng thái lưu trữ:** `localStorage` đồng bộ hóa trạng thái giỏ hàng và phiên đăng nhập của khách hàng.

---

## 4. Cấu trúc thư mục mã nguồn Frontend

```text
cms.frontend/
├── public/                       # File HTML tĩnh và các cấu hình chung của trình duyệt
├── src/                          # Thư mục chứa mã nguồn ReactJS chính
│   ├── api/
│   │   └── axiosClient.js        # Khởi tạo Axios, cấu hình baseUrl và interceptor
│   ├── assets/
│   │   ├── css/                  # File CSS riêng cho từng trang/component
│   │   └── images/               # Chứa các file ảnh tĩnh phục vụ giao diện
│   ├── components/
│   │   ├── Header.jsx            # Header chung hiển thị menu động và giỏ hàng
│   │   ├── Footer.jsx            # Chân trang hiển thị thông tin cửa hàng
│   │   ├── ProductCard.jsx       # Thẻ hiển thị sản phẩm mẫu gồm nhãn (New/Sale)
│   │   ├── SearchableSelect.jsx  # Component lựa chọn dropdown nâng cao có tìm kiếm
│   │   └── ScrollToTop.jsx       # Tự động cuộn màn hình lên đầu trang
│   ├── pages/
│   │   ├── Home.jsx              # Trang chủ hiển thị Banners động, sản phẩm mới
│   │   ├── Shop.jsx              # Trang danh sách sản phẩm với các bộ lọc thông minh
│   │   ├── ProductDetail.jsx     # Trang chi tiết sản phẩm và các thông số đi kèm
│   │   ├── Cart.jsx              # Giao diện giỏ hàng hỗ trợ thay đổi số lượng nhanh
│   │   ├── Checkout.jsx          # Trang điền thông tin đặt mua hàng
│   │   ├── Profile.jsx           # Trang hồ sơ cá nhân của khách hàng
│   │   ├── AddressesPage.jsx     # Trang quản lý danh bạ địa chỉ giao hàng
│   │   ├── ChangePassword.jsx    # Giao diện thay đổi mật khẩu
│   │   ├── Login.jsx             # Giao diện đăng nhập tài khoản khách hàng
│   │   ├── Register.jsx          # Giao diện đăng ký tài khoản khách hàng
│   │   ├── ForgotPassword.jsx    # Quy trình khôi phục mật khẩu qua Gmail
│   │   ├── OrderHistory.jsx      # Tra cứu danh sách đơn hàng đã mua
│   │   ├── OrderDetailPage.jsx   # Tra cứu thông tin chi tiết một đơn hàng
│   │   ├── PostList.jsx          # Danh sách bài viết blog dạng lưới/dòng
│   │   └── PostDetail.jsx        # Nội dung chi tiết của bài viết/tin tức
│   ├── services/                 # Nơi gọi API giao tiếp với Backend
│   │   ├── authService.js        
│   │   ├── productService.js     
│   │   ├── blogService.js        
│   │   ├── orderService.js       
│   │   ├── addressService.js     
│   │   ├── categoryProductService.js
│   │   ├── menuService.js        
│   │   └── bannerService.js      
│   ├── utils/
│   │   ├── mediaUrl.js           # Tiện ích chuẩn hóa URL tài nguyên media
│   │   ├── customerSession.js    # Tiện ích quản lý phiên đăng nhập
│   │   └── orderStatus.js        # Tiện ích xử lý trạng thái đơn hàng
│   ├── App.js                    # Định tuyến (Routes) quản lý URL và Component
│   └── index.js                  # Điểm khởi chạy React
├── .env                          # Biến cấu hình môi trường (REACT_APP_API_URL)
└── README.md                     # Tài liệu này
```

---

## 5. Các định tuyến (Routes) trên hệ thống

Các route được khai báo và quản lý tập trung trong file `src/App.js`:

| Đường dẫn URL | File Trang (`pages/`) | Mô tả chức năng |
| :--- | :--- | :--- |
| `/` | `Home.jsx` | Trang chủ hiển thị Banners, Sản phẩm mới/bán chạy |
| `/products` | `Shop.jsx` | Danh sách sản phẩm, tích hợp tìm kiếm và lọc |
| `/products/:id` | `ProductDetail.jsx` | Chi tiết sản phẩm, tồn kho và ảnh bổ sung |
| `/cart` | `Cart.jsx` | Giỏ hàng tạm tính của khách hàng |
| `/checkout` | `Checkout.jsx` | Điền thông tin giao hàng và đặt đơn |
| `/profile` | `Profile.jsx` | Trang quản lý tài khoản và hồ sơ |
| `/profile/change-password` | `ChangePassword.jsx` | Giao diện đổi mật khẩu bảo mật |
| `/account/addresses` | `AddressesPage.jsx` | Sổ địa chỉ của người dùng |
| `/login` | `Login.jsx` | Đăng nhập tài khoản |
| `/register` | `Register.jsx` | Đăng ký tài khoản mới |
| `/forgot-password` | `ForgotPassword.jsx` | Khôi phục mật khẩu qua mã OTP Gmail |
| `/order-history` | `OrderHistory.jsx` | Lịch sử đơn hàng của bản thân |
| `/my-orders` | `OrderHistory.jsx` | Lịch sử đơn hàng của bản thân |
| `/account/orders` | `OrderHistory.jsx` | Lịch sử đơn hàng của bản thân |
| `/account/orders/:id` | `OrderDetailPage.jsx` | Xem chi tiết nội dung một đơn hàng |
| `/blog` | `PostList.jsx` | Danh sách tất cả bài viết blog |
| `/blog/category/:categoryId` | `PostList.jsx` | Danh sách bài viết theo chuyên mục |
| `/blog/:id` | `PostDetail.jsx` | Nội dung chi tiết bài viết |

---

## 6. Các API kết nối từ Frontend sang Backend

Toàn bộ các tác vụ gọi API đều được cấu hình trong thư mục `src/services/` thông qua `axiosClient`.

### 6.1. Nhóm API Xác thực & Khách hàng (`authService.js`)
- Đăng ký tài khoản: `POST /api/Auth/CustomerRegister`
- Đăng nhập hệ thống: `POST /api/Auth/CustomerLogin`
- Gửi OTP Gmail: `POST /api/Auth/SendResetCode`
- Xác minh OTP: `POST /api/Auth/VerifyResetCode`
- Đặt mật khẩu mới: `POST /api/Auth/ResetPassword`

### 6.2. Nhóm API Sản phẩm (`productService.js` / `categoryProductService.js`)
- Lấy sản phẩm (lọc/sắp xếp/danh mục): `GET /api/Products`
- Lấy chi tiết sản phẩm: `GET /api/Products/{id}`
- Lấy phân loại sản phẩm: `GET /api/CategoriesProducts`

### 6.3. Nhóm API Giao diện (`menuService.js` / `bannerService.js`)
- Lấy cấu trúc thanh điều hướng động: `GET /api/Menus/hierarchy`
- Lấy banner trang chủ: `GET /api/Banners`

### 6.4. Nhóm API Bài viết & Tin tức (`blogService.js`)
- Lấy chuyên mục: `GET /api/Categories`
- Lấy bài viết (nổi bật, lọc): `GET /api/Posts`
- Xem một bài viết: `GET /api/Posts/{id}`

### 6.5. Nhóm API Đơn hàng & Địa chỉ (`orderService.js` / `addressService.js`)
- Tạo đơn hàng mới: `POST /api/Orders`
- Lịch sử đơn hàng: `GET /api/Orders/customer/{customerId}`
- Quản lý sổ địa chỉ: `GET, POST, PUT, DELETE /api/Addresses`

---

## 7. Dữ liệu lưu trữ cục bộ (LocalStorage)

- `customer`: Lưu thông tin định danh của khách hàng sau khi xác thực thành công.
- `cart`: Lưu trữ mảng đối tượng sản phẩm đang có trong giỏ hàng.

---

## 8. Hướng dẫn cấu hình & Chạy Frontend ReactJS

1. Mở file `.env` ở thư mục gốc của frontend và cấu hình URL API Backend:
   ```env
   REACT_APP_API_URL=https://localhost:7238/api
   ```
2. Mở Terminal tại thư mục `cms.frontend` và cài đặt package:
   ```bash
   npm install
   ```
3. Khởi động ReactJS cục bộ:
   ```bash
   npm start
   ```

Hệ thống sẽ biên dịch và chạy trên `http://localhost:3000`. Cần đảm bảo backend .NET đang chạy để lấy dữ liệu.

---

## 9. Chi tiết các luồng nghiệp vụ đặc trưng ở Frontend

### 9.1. Luồng mua sắm & Thanh toán (Checkout Flow)
1. Khách hàng lựa chọn sản phẩm từ Trang chủ hoặc trang Shop.
2. Thêm vào giỏ. Truy cập `/cart` để xem và chỉnh sửa số lượng.
3. Bấm **Tiến hành thanh toán**. Yêu cầu đăng nhập.
4. Mở trang `/checkout` hiển thị form, sổ địa chỉ tiện lợi và giỏ hàng tóm tắt.
5. Bấm **Đặt hàng**, gọi API `POST /api/Orders`. Backend trừ tồn kho và gửi email hóa đơn.
6. Hệ thống xóa giỏ hàng và chuyển về trang lịch sử đơn hàng.

### 9.2. Luồng Khôi phục mật khẩu bằng OTP (ForgotPassword Flow)
1. Vào trang Quên mật khẩu. Nhập email tài khoản.
2. Hệ thống gọi API sinh OTP 6 số ngẫu nhiên, tự gửi qua Gmail khách hàng.
3. Giao diện nhận OTP thiết kế 6 ô nhập chữ số riêng biệt với bộ đếm ngược thời gian.
4. Xác minh mã OTP hợp lệ.
5. Hiển thị form tạo mật khẩu mới an toàn và hoàn tất quy trình khôi phục.


