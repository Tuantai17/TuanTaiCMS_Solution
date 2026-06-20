import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import orderService from '../services/orderService';
import addressService from '../services/addressService';

const Checkout = () => {
  const [customer, setCustomer] = useState(null);
  const [cartItems, setCartItems] = useState([]);
  const [notes, setNotes] = useState('');
  
  // Các trường thông tin giao hàng (lấy từ tài khoản đăng nhập và cho phép sửa)
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [province, setProvince] = useState('');
  const [district, setDistrict] = useState('');

  const [savedAddresses, setSavedAddresses] = useState([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [authChecking, setAuthChecking] = useState(true);

  const navigate = useNavigate();

  useEffect(() => {
    // 1. Kiểm tra đăng nhập
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      setAuthChecking(false);
      return; // Sẽ xử lý chuyển hướng ở phần UI
    }

    try {
      const parsedCustomer = JSON.parse(storedCustomer);
      setCustomer(parsedCustomer);
      
      // Điền sẵn thông tin mặc định từ tài khoản đăng nhập
      setFullName(parsedCustomer.fullName || '');
      setEmail(parsedCustomer.email || '');
      setPhone(parsedCustomer.phone || '');
      setAddress(parsedCustomer.address || '');

      // Tải danh sách địa chỉ đã lưu
      addressService.getAddresses(parsedCustomer.customerId).then(addresses => {
        setSavedAddresses(addresses || []);
        const defaultAddr = addresses?.find(a => a.isDefault);
        if (defaultAddr) {
          setFullName(defaultAddr.recipientName);
          setPhone(defaultAddr.phoneNumber);
          setAddress(defaultAddr.addressLine);
          setProvince(defaultAddr.provinceName);
          setDistrict(defaultAddr.districtName);
        }
      }).catch(e => console.error("Lỗi lấy danh sách địa chỉ khi checkout:", e));
    } catch (e) {
      localStorage.removeItem('customer');
    }
    setAuthChecking(false);

    // 2. Lấy giỏ hàng thực tế
    const storedCart = localStorage.getItem('cart');
    if (storedCart) {
      try {
        const parsedCart = JSON.parse(storedCart);
        setCartItems(parsedCart);
      } catch (e) {
        setCartItems([]);
      }
    }
  }, []);

  // Xử lý chuyển hướng nếu chưa đăng nhập
  useEffect(() => {
    if (!authChecking && !customer) {
      alert("Bạn cần đăng nhập tài khoản thành viên để thực hiện thanh toán mua hàng!");
      navigate('/login');
    }
  }, [authChecking, customer, navigate]);

  // Xử lý chuyển hướng nếu giỏ hàng trống
  useEffect(() => {
    if (!authChecking && customer && cartItems.length === 0) {
      alert("Giỏ hàng của bạn đang trống!");
      navigate('/cart');
    }
  }, [authChecking, customer, cartItems, navigate]);

  if (authChecking || !customer || cartItems.length === 0) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang xác thực thông tin...</span>
        </div>
        <p className="mt-3 text-secondary">Đang chuyển hướng không gian thanh toán...</p>
      </div>
    );
  }

  const subTotal = cartItems.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  const shippingFee = 35000;
  const total = subTotal + shippingFee;

  const handleSelectSavedAddress = (e) => {
    const addrId = parseInt(e.target.value);
    if (!addrId) return;
    const addr = savedAddresses.find(a => a.id === addrId);
    if (addr) {
      setFullName(addr.recipientName);
      setPhone(addr.phoneNumber);
      setAddress(addr.addressLine);
      setProvince(addr.provinceName);
      setDistrict(addr.districtName);
    }
  };

  const handlePlaceOrder = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    // Chuẩn bị thông tin giao hàng kết hợp địa chỉ tỉnh/thành
    const fullDeliveryAddress = `${address}, ${district}, ${province}`;
    const orderData = {
      customerId: customer.customerId,
      notes: notes ? `[Giao tới: ${fullName} - SĐT: ${phone} - Email: ${email} - Đ/C: ${fullDeliveryAddress}]. Ghi chú khách hàng: ${notes}` : `[Giao tới: ${fullName} - SĐT: ${phone} - Email: ${email} - Đ/C: ${fullDeliveryAddress}]`,
      cartItems: cartItems.map(item => ({
        productId: item.id,
        quantity: item.quantity
      }))
    };

    try {
      await orderService.createOrder(orderData);
      
      // Xóa sạch giỏ hàng khi đặt hàng thành công
      localStorage.removeItem('cart');
      
      // Kích hoạt sự kiện cập nhật số lượng giỏ hàng ở Header
      window.dispatchEvent(new Event('cartChange'));
      
      alert("Đặt hàng thành công! Cảm ơn bạn đã mua hàng tại MyKingdom.");
      navigate('/account/orders');
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Có lỗi xảy ra trong quá trình đặt hàng. Vui lòng thử lại sau.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mt-4 animate--fade-in">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
          <li className="breadcrumb-item"><Link to="/cart" className="text-secondary text-decoration-none">Giỏ hàng</Link></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tiến hành thanh toán</li>
        </ol>
      </nav>

      <h2 className="font-weight-bold text-dark text-uppercase mb-4">
        <i className="fa-solid fa-credit-card text-danger mr-2"></i> Thông Tin Thanh Toán
      </h2>

      {error && (
        <div className="alert alert-danger px-4 py-3 rounded-lg mb-4 text-center font-weight-bold small" role="alert">
          <i className="fa-solid fa-circle-exclamation mr-2"></i> {error}
        </div>
      )}

      <div className="row">
        {/* CỘT TRÁI: FORM ĐIỀN THÔNG TIN KHÁCH HÀNG */}
        <div className="col-12 col-lg-7 mb-4">
          <div className="card shadow-sm border border-light rounded-lg p-4">
            <h5 className="font-weight-bold text-dark mb-4 border-bottom pb-3">Địa chỉ nhận hàng</h5>
            
            {savedAddresses.length > 0 && (
              <div className="mb-4 p-3 rounded" style={{ backgroundColor: '#fff8e1', border: '1px solid #ffe082' }}>
                <label className="small font-weight-bold text-dark">
                  <i className="fa-solid fa-location-dot text-danger mr-2"></i> Chọn nhanh địa chỉ nhận hàng đã lưu
                </label>
                <select 
                  className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                  onChange={handleSelectSavedAddress}
                  defaultValue=""
                >
                  <option value="" disabled>-- Chọn địa chỉ đã lưu --</option>
                  {savedAddresses.map(addr => (
                    <option key={addr.id} value={addr.id}>
                      [{addr.addressType}] {addr.recipientName} - {addr.phoneNumber} ({addr.addressLine}, {addr.wardName}, {addr.districtName}, {addr.provinceName}){addr.isDefault ? ' [Mặc định]' : ''}
                    </option>
                  ))}
                </select>
              </div>
            )}

            <form onSubmit={handlePlaceOrder}>
              <div className="row">
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Họ và tên người nhận *</label>
                  <input 
                    type="text" 
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                    placeholder="Nguyễn Văn A..." 
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    required 
                  />
                </div>
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Số điện thoại liên hệ *</label>
                  <input 
                    type="tel" 
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                    placeholder="0912..." 
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    required 
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="small font-weight-bold text-secondary">Địa chỉ Email</label>
                <input 
                  type="email" 
                  className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                  placeholder="nguyenvanan@gmail.com..." 
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <div className="mb-3">
                <label className="small font-weight-bold text-secondary">Địa chỉ giao hàng chi tiết *</label>
                <input 
                  type="text" 
                  className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                  placeholder="Số nhà, tên đường, phường/xã..." 
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  required 
                />
              </div>

              <div className="row">
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Tỉnh / Thành phố *</label>
                  <select 
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                    value={province}
                    onChange={(e) => setProvince(e.target.value)}
                    required
                  >
                    <option value="">-- Chọn Tỉnh/Thành phố --</option>
                    <option value="Thành phố Hồ Chí Minh">Thành phố Hồ Chí Minh</option>
                    <option value="Thành phố Hà Nội">Thành phố Hà Nội</option>
                    <option value="Thành phố Đà Nẵng">Thành phố Đà Nẵng</option>
                  </select>
                </div>
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Quận / Huyện *</label>
                  <select 
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50" 
                    value={district}
                    onChange={(e) => setDistrict(e.target.value)}
                    required
                  >
                    <option value="">-- Chọn Quận/Huyện --</option>
                    <option value="Quận 1">Quận 1</option>
                    <option value="Quận 3">Quận 3</option>
                    <option value="Quận 8">Quận 8</option>
                    <option value="Quận Cầu Giấy">Quận Cầu Giấy</option>
                    <option value="Quận Hải Châu">Quận Hải Châu</option>
                  </select>
                </div>
              </div>

              <div className="mb-4">
                <label className="small font-weight-bold text-secondary">Ghi chú giao hàng</label>
                <textarea 
                  className="form-control rounded shadow-none border-secondary-50" 
                  rows="3" 
                  placeholder="Ví dụ: Gọi trước khi giao, giao giờ hành chính..."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                ></textarea>
              </div>

              <h5 className="font-weight-bold text-dark mb-3 border-top pt-4">Phương thức thanh toán</h5>
              <div className="mb-4">
                <div className="custom-control custom-radio mb-3">
                  <input type="radio" id="paymentCod" name="paymentMethod" className="custom-control-input" defaultChecked />
                  <label className="custom-control-label font-weight-bold text-dark" htmlFor="paymentCod">
                    <i className="fa-solid fa-hand-holding-dollar text-success mr-2"></i> Thanh toán khi nhận hàng (COD)
                  </label>
                </div>
                <div className="custom-control custom-radio">
                  <input type="radio" id="paymentBank" name="paymentMethod" className="custom-control-input" />
                  <label className="custom-control-label font-weight-bold text-dark" htmlFor="paymentBank">
                    <i className="fa-solid fa-building-columns text-primary mr-2"></i> Chuyển khoản ngân hàng qua mã QR
                  </label>
                </div>
              </div>

              <button 
                type="submit" 
                className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3" 
                style={{ fontSize: '0.9rem' }}
                disabled={loading}
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>
                    Đang gửi đơn đặt hàng...
                  </>
                ) : (
                  <>
                    <i className="fa-solid fa-circle-check mr-2"></i> Hoàn tất đặt hàng
                  </>
                )}
              </button>
            </form>
          </div>
        </div>

        {/* CỘT PHẢI: TỔNG KẾT ĐƠN HÀNG THỰC TẾ */}
        <div className="col-12 col-lg-5">
          <div className="card shadow-sm border border-light rounded-lg p-4 bg-light">
            <h5 className="font-weight-bold text-dark mb-4 border-bottom pb-3">Đơn hàng của bạn</h5>
            
            {/* Danh sách các sản phẩm thực tế */}
            <div className="checkout-items-list mb-4 overflow-auto" style={{ maxHeight: '350px' }}>
              {cartItems.map((item) => (
                <div className="d-flex align-items-center mb-3 pb-3 border-bottom" key={item.id}>
                  <img src={item.imageUrl || "https://placehold.co/150x150/e9ecef/6c757d?text=No+Image"} alt={item.name} className="rounded border mr-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
                  <div className="flex-grow-1">
                    <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2" style={{ lineHeight: '1.3' }}>{item.name}</h6>
                    <span className="text-secondary small">Số lượng: {item.quantity}</span>
                  </div>
                  <span className="font-weight-bold text-dark pl-2 small" style={{ whiteSpace: 'nowrap' }}>
                    {new Intl.NumberFormat('vi-VN').format(item.price * item.quantity)} ₫
                  </span>
                </div>
              ))}
            </div>

            {/* Chi tiết chi phí */}
            <div className="d-flex justify-content-between mb-2 text-secondary small">
              <span>Tạm tính hàng hóa:</span>
              <span className="font-weight-bold text-dark">{new Intl.NumberFormat('vi-VN').format(subTotal)} ₫</span>
            </div>
            <div className="d-flex justify-content-between mb-3 text-secondary small">
              <span>Phí vận chuyển:</span>
              <span className="font-weight-bold text-dark">{new Intl.NumberFormat('vi-VN').format(shippingFee)} ₫</span>
            </div>
            <div className="d-flex justify-content-between border-top pt-3">
              <span className="font-weight-bold text-dark">Tổng thanh toán:</span>
              <span className="h4 font-weight-extrabold text-danger mb-0">{new Intl.NumberFormat('vi-VN').format(total)} ₫</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Checkout;
