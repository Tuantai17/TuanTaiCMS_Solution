# CMS Frontend - TuanTaiCMS

Frontend của dự án **TuanTaiCMS** được xây dựng bằng thư viện **ReactJS (phiên bản 19)** nhằm tối ưu hóa giao diện hiển thị cho người dùng / khách hàng cuối (Customer). Phần này kết nối chặt chẽ với backend **ASP.NET Core 8.0 Web API** qua giao thức HTTP/JSON để hiển thị danh sách sản phẩm, tin tức, giỏ hàng, đặt hàng, quản lý lịch sử mua sắm và khôi phục mật khẩu tự động qua OTP email.

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
| **User / Customer** | Frontend ReactJS `cms.frontend` | **Không** | Xem sản phẩm, đọc tin tức, đăng ký/đăng nhập, mua hàng, xem lịch sử mua sắm và tự khôi phục mật khẩu qua OTP |

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
- **Đăng ký (Register) & Đăng nhập (Login):** Xác thực tài khoản khách hàng.
- **Lịch sử đơn hàng (Order History):** Xem danh sách đơn hàng đã mua, ngày giờ đặt, trạng thái giao nhận và chi tiết từng sản phẩm trong đơn hàng.
- **Quên mật khẩu (ForgotPassword):** [Mới] Khôi phục mật khẩu an toàn qua 3 bước xác thực mã OTP gửi về Email cá nhân.
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
│   │   └── axiosClient.js        # Khởi tạo Axios, cấu hình baseUrl và interceptor xử lý JSON trả về
│   ├── assets/
│   │   ├── css/
│   │   │   └── App.css           # Định nghĩa CSS chính cho layout và hiệu ứng chuyển động toàn trang
│   │   └── images/               # Chứa các file ảnh tĩnh phục vụ giao diện
│   ├── components/
│   │   ├── Header.jsx            # Header chung hiển thị menu động, thanh tìm kiếm nhanh và giỏ hàng
│   │   ├── Header.css            # CSS thiết kế Header với hiệu ứng sticky và đổ bóng cao cấp
│   │   ├── Footer.jsx            # Chân trang hiển thị thông tin cửa hàng, liên hệ và chính sách
│   │   ├── ProductCard.jsx       # Thẻ hiển thị sản phẩm mẫu gồm nhãn (New/Sale), giá gốc và giá bán
│   │   ├── BlogCategoryList.jsx  # Danh sách chuyên mục tin tức hiển thị ở thanh bên (Sidebar)
│   │   └── ScrollToTop.jsx       # Tự động cuộn màn hình lên đầu trang khi thay đổi tuyến đường
│   ├── pages/
│   │   ├── Home.jsx              # Trang chủ hiển thị Banners động, sản phẩm mới, sản phẩm bán chạy
│   │   ├── Shop.jsx              # Trang danh sách sản phẩm với các bộ lọc thông minh (giá, danh mục, tìm kiếm)
│   │   ├── ProductDetail.jsx     # Trang chi tiết sản phẩm và các thông số đi kèm
│   │   ├── Cart.jsx              # Giao diện giỏ hàng hỗ trợ thay đổi số lượng nhanh
│   │   ├── Checkout.jsx          # Trang điền thông tin đặt mua hàng
│   │   ├── Login.jsx             # Giao diện đăng nhập tài khoản khách hàng
│   │   ├── Register.jsx          # Giao diện đăng ký tài khoản khách hàng
│   │   ├── ForgotPassword.jsx    # [NEW] Quy trình khôi phục mật khẩu 3 bước xác thực OTP qua Gmail
│   │   ├── OrderHistory.jsx      # Tra cứu danh sách đơn hàng đã mua
│   │   ├── PostList.jsx          # Danh sách bài viết blog hỗ trợ hiển thị dạng lưới (Grid) hoặc dòng (List)
│   │   └── PostDetail.jsx        # Nội dung chi tiết của bài viết/tin tức
│   ├── services/
│   │   ├── authService.js        # Gửi API đăng nhập, đăng ký và khôi phục mật khẩu bằng OTP
│   │   ├── productService.js     # Gọi API lấy sản phẩm (mới nhất, bán chạy, theo danh mục, tìm kiếm)
│   │   ├── categoryProductService.js # Gọi API lấy danh sách phân loại sản phẩm
│   │   ├── blogService.js        # Gọi API lấy bài viết tin tức, bài viết nổi bật và danh mục blog
│   │   ├── orderService.js       # Gọi API gửi đơn đặt hàng và tải lịch sử mua hàng
│   │   ├── menuService.js        # [NEW] Gọi API lấy danh sách thanh điều hướng động (Menus)
│   │   └── bannerService.js      # [NEW] Gọi API lấy danh sách hình ảnh quảng cáo (Banners)
│   ├── utils/
│   │   └── mediaUrl.js           # [NEW] Tiện ích chuẩn hóa URL tài nguyên media từ Backend (xử lý link ảnh local/cloud)
│   ├── App.js                    # File định tuyến (Route Layout) liên kết URL với các Trang
│   └── index.js                  # Điểm nút khởi chạy React 19 và nạp font chữ
├── .env                          # Chứa biến cấu hình môi trường phát triển (REACT_APP_API_URL)
├── package.json                  # Quản lý thư viện phụ thuộc và câu lệnh chạy dự án
└── README.md                     # Tài liệu hướng dẫn sử dụng và vận hành Frontend
```

---

## 5. Các định tuyến (Routes) trên hệ thống

Các route được khai báo và quản lý tập trung trong file `src/App.js`:

| Đường dẫn URL | File Trang (`pages/`) | Mô tả chức năng |
| :--- | :--- | :--- |
| `/` | `Home.jsx` | Trang chủ hiển thị Banners, Sản phẩm mới/bán chạy và Tin tức |
| `/products` | `Shop.jsx` | Danh sách sản phẩm, tích hợp tìm kiếm, lọc giá và lọc loại |
| `/products/:id` | `ProductDetail.jsx` | Chi tiết sản phẩm, xem tồn kho và ảnh bổ sung |
| `/cart` | `Cart.jsx` | Giỏ hàng tạm tính của khách hàng |
| `/checkout` | `Checkout.jsx` | Điền thông tin giao hàng và gửi đơn hàng (yêu cầu đăng nhập) |
| `/login` | `Login.jsx` | Đăng nhập tài khoản khách hàng |
| `/register` | `Register.jsx` | Đăng ký tài khoản khách hàng mới |
| `/forgot-password` | `ForgotPassword.jsx` | [Mới] Khôi phục mật khẩu 3 bước thông qua mã OTP Gmail |
| `/order-history` | `OrderHistory.jsx` | Tra cứu lịch sử đơn hàng của bản thân (yêu cầu đăng nhập) |
| `/blog` | `PostList.jsx` | Danh sách tất cả bài viết cẩm nang & blog tin tức |
| `/blog/category/:categoryId` | `PostList.jsx` | [Mới] Xem danh sách bài viết được lọc theo chuyên mục |
| `/blog/:id` | `PostDetail.jsx` | Đọc nội dung chi tiết bài viết tin tức |

---

## 6. Các API kết nối từ Frontend sang Backend

Toàn bộ các tác vụ gọi API đều được cấu hình trong thư mục `src/services/` thông qua `axiosClient`.

### 6.1. Nhóm API Xác thực & Khách hàng
*Thư mục kết nối: `src/services/authService.js`*
- **Đăng ký tài khoản:** `POST /api/Auth/CustomerRegister`
- **Đăng nhập hệ thống:** `POST /api/Auth/CustomerLogin`
- **Gửi OTP Gmail (Bước 1):** `POST /api/Auth/SendResetCode`
- **Xác minh OTP (Bước 2):** `POST /api/Auth/VerifyResetCode`
- **Đặt mật khẩu mới (Bước 3):** `POST /api/Auth/ResetPassword`

### 6.2. Nhóm API Sản phẩm
*Thư mục kết nối: `src/services/productService.js`*
- **Lấy tất cả sản phẩm (kèm tìm kiếm/sắp xếp/lọc):** `GET /api/Products`
- **Lấy danh sách sản phẩm mới về:** `GET /api/Products?filter=new`
- **Lấy danh sách sản phẩm bán chạy:** `GET /api/Products?sortBy=best-selling`
- **Lọc sản phẩm theo danh mục:** `GET /api/Products/categoryproduct/{categoryId}`
- **Lấy chi tiết một sản phẩm:** `GET /api/Products/{id}`

### 6.3. Nhóm API Danh mục sản phẩm
*Thư mục kết nối: `src/services/categoryProductService.js`*
- **Tải danh mục phân loại sản phẩm:** `GET /api/CategoriesProducts`

### 6.4. Nhóm API Menu điều hướng động
*Thư mục kết nối: `src/services/menuService.js`*
- **Lấy danh sách các liên kết menu đang bật:** `GET /api/Menus`
- **Lấy cấu trúc menu dạng cha - con:** `GET /api/Menus/hierarchy`

### 6.5. Nhóm API Banner trang chủ
*Thư mục kết nối: `src/services/bannerService.js`*
- **Lấy các hình ảnh banner quảng cáo động hiển thị ở trang chủ:** `GET /api/Banners`

### 6.6. Nhóm API Bài viết & Tin tức
*Thư mục kết nối: `src/services/blogService.js`*
- **Lấy chuyên mục bài viết:** `GET /api/Categories`
- **Lấy toàn bộ bài viết:** `GET /api/Posts`
- **Lấy bài viết nổi bật (trang chủ):** `GET /api/Posts/featured`
- **Lọc bài viết theo chuyên mục:** `GET /api/Posts/category/{categoryId}`
- **Lấy chi tiết một bài viết:** `GET /api/Posts/{id}`

### 6.7. Nhóm API Đơn hàng
*Thư mục kết nối: `src/services/orderService.js`*
- **Tạo đơn hàng mới (Trừ tồn kho, gửi email):** `POST /api/Orders`
- **Lịch sử đơn hàng của khách hàng:** `GET /api/Orders/customer/{customerId}`

---

## 7. Dữ liệu lưu trữ cục bộ (LocalStorage)

Để tối ưu hóa trải nghiệm và duy trì trạng thái ứng dụng khi người dùng tải lại trang:
- `customer`: Lưu thông tin định danh của khách hàng sau khi xác thực thành công.
  ```json
  {
    "customerId": 1,
    "fullName": "Nguyễn Văn A",
    "email": "customer@example.com",
    "phone": "0987654321",
    "address": "Quận 1, TP. Hồ Chí Minh"
  }
  ```
- `cart`: Lưu trữ mảng đối tượng sản phẩm đang có trong giỏ hàng.
  ```json
  [
    {
      "id": 10,
      "name": "Đồ chơi lắp ráp LEGO City",
      "price": 450000,
      "imageUrl": "/images/lego-city.jpg",
      "quantity": 2
    }
  ]
  ```

---

## 8. Hướng dẫn cấu hình & Chạy Frontend ReactJS

### Bước 1: Thiết lập cấu hình kết nối API
Địa chỉ cổng chạy Web API Backend được định nghĩa tập trung trong file `.env` ở thư mục gốc của frontend:
```env
REACT_APP_API_URL=https://localhost:7238/api
```
*Lưu ý: Nếu backend ASP.NET chạy ở cổng khác, hãy cập nhật cổng tương ứng trong file `.env` và khởi động lại dự án React.*

### Bước 2: Cài đặt các thư viện Node Modules
Mở Terminal tại thư mục `cms.frontend` và cài đặt các gói phụ thuộc cần thiết:
```bash
npm install
```

### Bước 3: Khởi chạy dự án ở môi trường cục bộ
Khởi động máy chủ phát triển local ReactJS:
```bash
npm start
```
Hệ thống sẽ tự động biên dịch và mở ứng dụng trên trình duyệt web tại địa chỉ:
```text
http://localhost:3000
```
*(Yêu cầu: Backend ASP.NET Core phải được khởi chạy trước để API phản hồi đúng dữ liệu hiển thị động).*

---

## 9. Chi tiết các luồng nghiệp vụ đặc trưng ở Frontend

### 9.1. Luồng mua sắm & Thanh toán (Checkout Flow)
1. Khách hàng lựa chọn sản phẩm từ Trang chủ hoặc trang Shop.
2. Bấm nút **Thêm vào giỏ** từ danh sách hoặc trang Chi tiết sản phẩm.
3. Số lượng giỏ hàng trên Header tự động cập nhật ngay lập tức nhờ cơ chế quản lý state.
4. Truy cập trang `/cart` để rà soát danh sách sản phẩm, cập nhật số lượng hoặc xóa bớt.
5. Bấm nút **Tiến hành thanh toán**.
6. Hệ thống kiểm tra phiên đăng nhập (`localStorage`). Nếu chưa đăng nhập, khách hàng được điều hướng tự động sang `/login`.
7. Sau khi đăng nhập, hệ thống mở trang `/checkout` hiển thị form thông tin nhận hàng và danh sách sản phẩm tóm tắt.
8. Bấm **Đặt hàng**, frontend gửi yêu cầu `POST /api/Orders` kèm danh sách giỏ hàng.
9. Backend tiếp nhận đơn, thực hiện transaction trừ số lượng sản phẩm tồn kho trong database và đồng thời kích hoạt tác vụ nền gửi email HTML xác nhận hóa đơn tự động về hòm thư khách hàng.
10. Giỏ hàng của khách hàng tại client tự động xóa sạch và màn hình chuyển hướng về trang lịch sử đơn hàng `/order-history` để theo dõi trạng thái.

### 9.2. Luồng Khôi phục mật khẩu bằng OTP 3 bước (ForgotPassword Flow)
1. Tại trang `/login`, khách hàng bấm liên kết **Quên mật khẩu?** để chuyển đến `/forgot-password`.
2. **Bước 1 (Nhập Email):** Khách hàng điền email tài khoản đã đăng ký. Hệ thống gọi API `POST /api/Auth/SendResetCode`.
   - Backend sẽ sinh mã OTP ngẫu nhiên gồm 6 chữ số (hiệu lực trong 5 phút) và gửi email HTML bảo mật đến Gmail khách hàng.
   - *Tính năng hỗ trợ lập trình viên:* Nếu máy tính không có mạng hoặc cấu hình SMTP lỗi, API backend sẽ trả về mã OTP trực tiếp trong phản hồi dưới dạng trường `otpForTesting` để hỗ trợ test nhanh trên giao diện frontend.
3. **Bước 2 (Nhập OTP):** Giao diện chuyển sang phần nhập mã OTP với 6 ô nhập chữ số riêng biệt có tính năng tự động focus ô tiếp theo khi điền số, tự động xóa lùi bằng Backspace, dán (paste) nhanh từ bộ nhớ tạm và hiển thị đồng hồ đếm ngược 5 phút đi kèm thanh tiến trình trực quan. Khách hàng bấm **Xác minh**, hệ thống gọi API `POST /api/Auth/VerifyResetCode`.
4. **Bước 3 (Nhập mật khẩu mới):** Khi OTP hợp lệ, giao diện hiển thị form nhập mật khẩu mới (yêu cầu độ dài tối thiểu 6 ký tự) và ô nhập lại mật khẩu xác nhận. Khách hàng bấm **Đặt lại mật khẩu**, hệ thống gọi API `POST /api/Auth/ResetPassword`. Giao diện hiển thị thông báo thành công và tự động chuyển về trang Đăng nhập sau 2.5 giây.

---

## 10. Checklist Kiểm thử & Đánh giá trước khi nộp Đồ án

Học viên vui lòng kiểm tra kỹ các hạng mục sau để đảm bảo đồ án chạy hoàn chỉnh:
- [x] Backend ASP.NET Core được chạy trước thành công và cổng API khớp cấu hình `.env` frontend.
- [x] Khởi chạy frontend ReactJS không gặp lỗi compile.
- [x] Khách hàng (Customer) đăng nhập và đăng ký tài khoản mới hoạt động chính xác.
- [x] Dữ liệu Slider banner trang chủ và thanh menu điều hướng được lấy động thông qua các API tương ứng.
- [x] Trang Shop tìm kiếm nhanh theo tên sản phẩm và lọc khoảng giá hoạt động chính xác.
- [x] Khách hàng thực hiện thêm sản phẩm vào giỏ hàng thành công, hiển thị đúng ảnh thu nhỏ sản phẩm bằng cách dùng helper `getMediaUrl`.
- [x] Tiến hành đặt hàng thành công, số lượng hàng tồn kho (StockQuantity) trong CSDL bị trừ tương ứng, nhận được email HTML tự động xác nhận đơn hàng từ MyKingdom gửi về Gmail khách hàng.
- [x] Trang lịch sử mua sắm hiển thị đúng và đầy đủ thông tin đơn hàng vừa đặt kèm chi tiết sản phẩm.
- [x] Chức năng khôi phục mật khẩu OTP hoạt động mượt mà đầy đủ qua cả 3 bước (gửi mã OTP, xác thực mã và đổi mật khẩu mới).
- [x] Bộ lọc chuyên mục tin tức và hiển thị danh sách bài viết dạng List / Grid hoạt động trơn tru.
- [x] Trang chi tiết bài viết blog hiển thị đúng định dạng nội dung dạng HTML được biên tập từ admin.
