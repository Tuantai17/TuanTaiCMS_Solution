# CMS Frontend - TuanTaiCMS

Frontend của project **TuanTaiCMS** được xây dựng bằng **ReactJS** để phục vụ giao diện người dùng / khách hàng. Phần này kết nối với backend ASP.NET Core thông qua Web API để hiển thị sản phẩm, bài viết, đăng ký / đăng nhập khách hàng, đặt hàng và xem lịch sử mua hàng.

---

## 1. Mục tiêu của frontend

Frontend đảm nhiệm nhóm chức năng dành cho **User / Customer**:

| Nhóm | Cách xử lý hiện tại | Trạng thái |
|---|---|---|
| Admin | Đăng nhập vào backend MVC, được phép quản trị dữ liệu | Đã có ở project `CMS.Backend` |
| User / Customer | Đăng ký, đăng nhập ở frontend, đặt hàng và xem lịch sử | Đã có ở `cms.frontend` |

> **Ghi chú:** Backend MVC chỉ dành cho **Admin** quản trị dữ liệu. Frontend ReactJS dành cho **User / Customer** mua hàng.

---

## 2. Công nghệ sử dụng

- ReactJS
- React Router DOM
- Axios
- Bootstrap / CSS tùy chỉnh
- LocalStorage để lưu trạng thái khách hàng và giỏ hàng
- ASP.NET Core Web API làm backend dữ liệu

---

## 3. Cấu trúc thư mục chính

```text
cms.frontend/
├── public/
├── src/
│   ├── api/
│   │   └── axiosClient.js
│   ├── assets/
│   │   ├── css/
│   │   └── images/
│   ├── components/
│   │   ├── Header.jsx
│   │   ├── Footer.jsx
│   │   └── ProductCard.jsx
│   ├── pages/
│   │   ├── Home.jsx
│   │   ├── Shop.jsx
│   │   ├── ProductDetail.jsx
│   │   ├── Cart.jsx
│   │   ├── Checkout.jsx
│   │   ├── Login.jsx
│   │   ├── Register.jsx
│   │   ├── OrderHistory.jsx
│   │   ├── PostList.jsx
│   │   └── PostDetail.jsx
│   ├── services/
│   │   ├── authService.js
│   │   ├── productService.js
│   │   ├── categoryProductService.js
│   │   ├── blogService.js
│   │   └── orderService.js
│   ├── App.js
│   └── index.js
├── package.json
└── README.md
```

---

## 4. Các route chính

Các route được khai báo trong `src/App.js`:

| Đường dẫn | Trang | Chức năng |
|---|---|---|
| `/` | `Home.jsx` | Trang chủ |
| `/products` | `Shop.jsx` | Danh sách sản phẩm |
| `/products/:id` | `ProductDetail.jsx` | Chi tiết sản phẩm |
| `/cart` | `Cart.jsx` | Giỏ hàng |
| `/checkout` | `Checkout.jsx` | Thanh toán |
| `/login` | `Login.jsx` | Đăng nhập khách hàng |
| `/register` | `Register.jsx` | Đăng ký khách hàng |
| `/order-history` | `OrderHistory.jsx` | Lịch sử mua hàng |
| `/blog` | `PostList.jsx` | Danh sách bài viết |
| `/blog/:id` | `PostDetail.jsx` | Chi tiết bài viết |

---

## 5. Luồng phân quyền và tài khoản

Project hiện được tách thành 2 nhóm người dùng rõ ràng:

### 5.1. Admin

- Admin đăng nhập ở backend MVC.
- Admin quản trị danh mục, sản phẩm, bài viết, khách hàng, đơn hàng.
- Các controller quản trị trong backend dùng `[Authorize(Roles = "Admin")]`.

### 5.2. User / Customer

- Customer đăng ký ở trang `/register`.
- Customer đăng nhập ở trang `/login`.
- Sau khi đăng nhập, thông tin customer được lưu vào `localStorage` với key `customer`.
- Customer có thể:
  - Xem sản phẩm.
  - Thêm sản phẩm vào giỏ hàng.
  - Thanh toán đơn hàng.
  - Xem lịch sử mua hàng.

---

## 6. Các API frontend đang sử dụng

### 6.1. API đăng ký / đăng nhập customer

Service: `src/services/authService.js`

| Method | API | Mục đích |
|---|---|---|
| POST | `/Auth/CustomerRegister` | Đăng ký tài khoản khách hàng |
| POST | `/Auth/CustomerLogin` | Đăng nhập khách hàng |

Backend tương ứng: `CMS.Backend/Controllers/AuthController.cs`

### 6.2. API sản phẩm

Service: `src/services/productService.js`

| Method | API | Mục đích |
|---|---|---|
| GET | `/Products` | Lấy danh sách sản phẩm |
| GET | `/Products/{id}` | Lấy chi tiết sản phẩm |
| GET | `/Products/categoryproduct/{categoryProductId}` | Lấy sản phẩm theo danh mục |

Backend tương ứng: `CMS.Backend/Controllers/ProductsController.cs`

### 6.3. API danh mục sản phẩm

Service: `src/services/categoryProductService.js`

| Method | API | Mục đích |
|---|---|---|
| GET | `/CategoriesProducts` | Lấy danh sách danh mục sản phẩm |

Backend tương ứng: `CMS.Backend/Controllers/CategoriesProductsController.cs`

### 6.4. API bài viết

Service: `src/services/blogService.js`

| Method | API | Mục đích |
|---|---|---|
| GET | `/Posts` | Lấy danh sách bài viết |
| GET | `/Posts/{id}` | Lấy chi tiết bài viết |

Backend tương ứng: `CMS.Backend/Controllers/PostsController.cs`

### 6.5. API đơn hàng

Service: `src/services/orderService.js`

| Method | API | Mục đích |
|---|---|---|
| POST | `/Orders` | Tạo đơn hàng mới |
| GET | `/Orders/customer/{customerId}` | Lấy lịch sử mua hàng theo khách hàng |

Backend tương ứng: `CMS.Backend/Controllers/OrdersController.cs`

---

## 7. LocalStorage đang sử dụng

| Key | Ý nghĩa |
|---|---|
| `customer` | Lưu thông tin khách hàng sau khi đăng nhập |
| `cart` | Lưu danh sách sản phẩm trong giỏ hàng |

Ví dụ dữ liệu `customer`:

```json
{
  "customerId": 1,
  "fullName": "Nguyễn Văn A",
  "email": "customer@example.com",
  "phone": "0123456789",
  "address": "TP.HCM"
}
```

Ví dụ dữ liệu `cart`:

```json
[
  {
    "id": 1,
    "name": "Tên sản phẩm",
    "price": 100000,
    "imageUrl": "/images/product.jpg",
    "quantity": 2
  }
]
```

---

## 8. Cách chạy frontend

Mở terminal tại thư mục `cms.frontend`:

```bash
npm install
npm start
```

Sau khi chạy thành công, mở trình duyệt tại:

```text
http://localhost:3000
```

> **Lưu ý:** Backend ASP.NET Core cần chạy trước để frontend gọi API thành công.

---

## 9. Cấu hình API

File cấu hình axios nằm tại:

```text
src/api/axiosClient.js
```

Nếu backend chạy ở cổng khác, cần kiểm tra và chỉnh `baseURL` trong file này cho đúng với địa chỉ backend hiện tại.

Ví dụ:

```js
const axiosClient = axios.create({
  baseURL: 'https://localhost:xxxx/api'
});
```

---

## 10. Luồng sử dụng chính

### 10.1. Luồng mua hàng

1. Customer vào trang `/products`.
2. Chọn sản phẩm hoặc xem chi tiết sản phẩm.
3. Bấm thêm vào giỏ hàng.
4. Vào trang `/cart` kiểm tra sản phẩm.
5. Bấm thanh toán.
6. Nếu chưa đăng nhập, customer cần đăng nhập ở `/login`.
7. Vào `/checkout` để hoàn tất đặt hàng.
8. Frontend gọi API `POST /Orders`.
9. Backend tạo đơn hàng, chi tiết đơn hàng và trừ số lượng tồn kho.
10. Customer xem lại đơn hàng ở `/order-history`.

### 10.2. Luồng đăng nhập customer

1. Customer nhập email và mật khẩu ở `/login`.
2. Frontend gọi API `/Auth/CustomerLogin`.
3. Nếu đăng nhập thành công, thông tin customer được lưu vào `localStorage`.
4. Header cập nhật trạng thái tài khoản.

---

## 11. Minh chứng hiển thị theo luồng thực tế

Phần frontend hiện đã hiển thị đúng theo luồng **User / Customer** như sau:

### 11.1. Trang chi tiết sản phẩm

Đường dẫn ví dụ:

```text
http://localhost:3000/products/{id}
```

Trang chi tiết sản phẩm hiển thị các thông tin chính:

- Hình ảnh sản phẩm.
- Tên sản phẩm.
- Mã SKU sản phẩm.
- Giá niêm yết.
- Trạng thái tồn kho.
- Danh mục sản phẩm.
- Mô tả sản phẩm.
- Nút **Thêm vào giỏ**.
- Nút **Quay lại chọn thêm**.

Khi khách hàng bấm **Thêm vào giỏ**, sản phẩm được lưu vào `localStorage` với key `cart` và số lượng trên Header được cập nhật.

### 11.2. Trang giỏ hàng

Đường dẫn:

```text
http://localhost:3000/cart
```

Trang giỏ hàng hiển thị:

- Danh sách sản phẩm đã thêm.
- Hình ảnh sản phẩm.
- Tên sản phẩm.
- Mã sản phẩm.
- Số lượng.
- Giá sản phẩm.
- Nút xóa sản phẩm khỏi giỏ.
- Tóm tắt đơn hàng.
- Tạm tính hàng hóa.
- Phí vận chuyển dự kiến.
- Tổng cộng thanh toán.
- Nút **Tiến hành thanh toán**.

### 11.3. Trang lịch sử mua hàng

Đường dẫn:

```text
http://localhost:3000/order-history
```

Sau khi customer đặt hàng thành công, trang lịch sử mua hàng hiển thị:

- Mã đơn hàng.
- Ngày đặt hàng.
- Trạng thái đơn hàng.
- Danh sách sản phẩm trong đơn.
- Số lượng sản phẩm.
- Thành tiền từng sản phẩm.
- Tổng cộng thanh toán.
- Thông tin ghi chú / giao hàng.

Frontend lấy dữ liệu lịch sử đơn hàng bằng API:

```text
GET /Orders/customer/{customerId}
```

### 11.4. Màn hình quản lý đơn hàng phía Admin

Phần này thuộc project backend MVC tại `CMS.Backend`.

Đường dẫn ví dụ:

```text
https://localhost:7238/Order
```

Admin có thể xem danh sách đơn hàng với các thông tin:

- Mã đơn hàng.
- Tên khách hàng.
- Ngày đặt.
- Trạng thái.
- Số dòng hàng.
- Ghi chú.
- Các nút thao tác: xem, chi tiết, sửa, xóa.

### 11.5. Màn hình chi tiết đơn hàng phía Admin

Đường dẫn ví dụ:

```text
https://localhost:7238/Order/Details/{id}
```

Admin có thể xem chi tiết đơn hàng gồm:

- Tên khách hàng.
- Email.
- Ngày đặt.
- Trạng thái.
- Ghi chú giao hàng.
- Tổng giá trị đơn hàng.
- Danh sách sản phẩm trong đơn.
- Số lượng.
- Đơn giá.
- Thành tiền.

Điều này chứng minh luồng đặt hàng từ frontend đã liên kết được với backend quản trị đơn hàng.

---

## 12. Checklist kiểm thử trước khi nộp

- [ ] Chạy backend ASP.NET Core thành công.
- [ ] Chạy frontend ReactJS thành công.
- [ ] Đăng ký customer mới thành công.
- [ ] Đăng nhập customer thành công.
- [ ] Sản phẩm hiển thị động từ API.
- [ ] Bài viết hiển thị động từ API.
- [ ] Thêm sản phẩm vào giỏ hàng thành công.
- [ ] Số lượng giỏ hàng trên Header cập nhật đúng.
- [ ] Checkout tạo được đơn hàng.
- [ ] Bảng `Orders` có dữ liệu mới.
- [ ] Bảng `OrderDetails` có dữ liệu mới.
- [ ] `Products.StockQuantity` bị trừ đúng.
- [ ] Trang lịch sử mua hàng hiển thị đơn đã đặt.

---

## 12. Ghi chú quan trọng

- Mật khẩu customer hiện đang xử lý đơn giản theo yêu cầu bài học. Nếu triển khai thực tế nên hash mật khẩu.
- Customer hiện được lưu bằng `localStorage`, chưa dùng JWT hoặc cookie auth riêng cho frontend.
- API đặt hàng nhận `CustomerId` từ frontend, phù hợp bài tập nhưng chưa phải mô hình bảo mật cao.
- Backend MVC và Frontend ReactJS là 2 phần riêng:
  - Backend MVC: dành cho Admin.
  - Frontend ReactJS: dành cho User / Customer.

---

## 13. Scripts có sẵn

Trong thư mục `cms.frontend`, có thể chạy:

```bash
npm start
```

Chạy ứng dụng ở chế độ development.

```bash
npm test
```

Chạy test theo cấu hình Create React App.

```bash
npm run build
```

Build frontend ra thư mục `build` để deploy production.
