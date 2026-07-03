import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import orderService from '../services/orderService';
import addressService from '../services/addressService';
import SearchableSelect from '../components/SearchableSelect';
import '../assets/css/Checkout.css';

const Checkout = () => {
  const [customer, setCustomer] = useState(null);
  const [cartItems, setCartItems] = useState([]);
  const [notes, setNotes] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('cod');
  
  // Các trường thông tin giao hàng
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [province, setProvince] = useState('');
  const [district, setDistrict] = useState('');
  const [ward, setWard] = useState('');

  // Tỉnh thành API states
  const [provincesList, setProvincesList] = useState([]);
  const [availableDistricts, setAvailableDistricts] = useState([]);
  const [availableWards, setAvailableWards] = useState([]);

  const [savedAddresses, setSavedAddresses] = useState([]);
  const [selectedAddressId, setSelectedAddressId] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [authChecking, setAuthChecking] = useState(true);

  const navigate = useNavigate();

  useEffect(() => {
    // 1. Kiểm tra đăng nhập
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      setAuthChecking(false);
      return; 
    }

    try {
      const parsedCustomer = JSON.parse(storedCustomer);
      setCustomer(parsedCustomer);
      
      setFullName(parsedCustomer.fullName || '');
      setEmail(parsedCustomer.email || '');
      setPhone(parsedCustomer.phone || '');
      setAddress(parsedCustomer.address || '');

      addressService.getAddresses(parsedCustomer.customerId).then(addresses => {
        setSavedAddresses(addresses || []);
        const defaultAddr = addresses?.find(a => a.isDefault);
        if (defaultAddr) {
          setSelectedAddressId(defaultAddr.id);
          setFullName(defaultAddr.recipientName);
          setPhone(defaultAddr.phoneNumber);
          setAddress(defaultAddr.addressLine);
          setProvince(defaultAddr.provinceName);
          setDistrict(defaultAddr.districtName);
          setWard(defaultAddr.wardName || '');
        }
      }).catch(e => console.error("Lỗi lấy danh sách địa chỉ khi checkout:", e));
    } catch (e) {
      localStorage.removeItem('customer');
    }
    setAuthChecking(false);

    // 2. Lấy giỏ hàng đã chọn từ sessionStorage
    const storedCheckoutItems = sessionStorage.getItem('checkoutItems');
    if (storedCheckoutItems) {
      try {
        const parsedItems = JSON.parse(storedCheckoutItems);
        setCartItems(parsedItems);
      } catch (e) {
        setCartItems([]);
      }
    } else {
      // Fallback: Nếu không có checkoutItems (trường hợp mua ngay), thử lấy từ cart
      const storedCart = localStorage.getItem('cart');
      if (storedCart) {
        try {
          const parsedCart = JSON.parse(storedCart);
          setCartItems(parsedCart);
        } catch (e) {
          setCartItems([]);
        }
      }
    }
  }, []);

  // Fetch Provinces
  useEffect(() => {
    fetch('https://provinces.open-api.vn/api/p/')
      .then(res => res.json())
      .then(data => setProvincesList(data))
      .catch(err => console.error("Error fetching provinces:", err));
  }, []);

  // Fetch Districts when Province changes
  useEffect(() => {
    if (!province || provincesList.length === 0) {
      setAvailableDistricts([]);
      return;
    }
    const selectedProv = provincesList.find(p => p.name === province);
    if (selectedProv) {
      fetch(`https://provinces.open-api.vn/api/p/${selectedProv.code}?depth=2`)
        .then(res => res.json())
        .then(data => setAvailableDistricts(data.districts || []))
        .catch(err => console.error("Error fetching districts:", err));
    }
  }, [province, provincesList]);

  // Fetch Wards when District changes
  useEffect(() => {
    if (!district || availableDistricts.length === 0) {
      setAvailableWards([]);
      return;
    }
    const selectedDist = availableDistricts.find(d => d.name === district);
    if (selectedDist) {
      fetch(`https://provinces.open-api.vn/api/d/${selectedDist.code}?depth=2`)
        .then(res => res.json())
        .then(data => setAvailableWards(data.wards || []))
        .catch(err => console.error("Error fetching wards:", err));
    }
  }, [district, availableDistricts]);

  useEffect(() => {
    if (!authChecking && !customer) {
      alert("Bạn cần đăng nhập tài khoản thành viên để thực hiện thanh toán mua hàng!");
      navigate('/login');
    }
  }, [authChecking, customer, navigate]);

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
  const total = subTotal;

  const handleSelectSavedAddress = (e) => {
    const addrId = e.target.value;
    setSelectedAddressId(addrId);
    if (!addrId) {
      setFullName(customer?.fullName || '');
      setPhone(customer?.phone || '');
      setAddress('');
      setProvince('');
      setDistrict('');
      setWard('');
      return;
    }
    const parsedId = parseInt(addrId);
    const addr = savedAddresses.find(a => a.id === parsedId);
    if (addr) {
      setFullName(addr.recipientName);
      setPhone(addr.phoneNumber);
      setAddress(addr.addressLine);
      setProvince(addr.provinceName);
      setDistrict(addr.districtName);
      setWard(addr.wardName || '');
    }
  };

  const handlePlaceOrder = async (e) => {
    e.preventDefault();
    if (!province || !district || !ward) {
      setError('Vui lòng chọn đầy đủ Tỉnh/Thành, Quận/Huyện và Phường/Xã');
      window.scrollTo(0, 0);
      return;
    }

    setError('');
    setLoading(true);

    const fullDeliveryAddress = `${address}, ${ward}, ${district}, ${province}`;
    const paymentStr = paymentMethod === 'cod' ? 'Thanh toán khi nhận hàng (COD)' : 'Chuyển khoản qua QR';
    const finalNotes = notes 
      ? `[Giao tới: ${fullName} - SĐT: ${phone} - Email: ${email} - Đ/C: ${fullDeliveryAddress}] [PTTT: ${paymentStr}]. Ghi chú KH: ${notes}` 
      : `[Giao tới: ${fullName} - SĐT: ${phone} - Email: ${email} - Đ/C: ${fullDeliveryAddress}] [PTTT: ${paymentStr}]`;

    const orderData = {
      customerId: customer.customerId,
      notes: finalNotes,
      cartItems: cartItems.map(item => ({
        productId: item.id,
        quantity: item.quantity
      }))
    };

    try {
      const result = await orderService.createOrder(orderData);
      
      // Xoa cac san pham da thanh toan khoi gio hang thay vi xoa toan bo
      const currentCartStr = localStorage.getItem('cart');
      if (currentCartStr) {
        let currentCart = JSON.parse(currentCartStr);
        const checkedOutIds = cartItems.map(i => i.id);
        currentCart = currentCart.filter(item => !checkedOutIds.includes(item.id));
        localStorage.setItem('cart', JSON.stringify(currentCart));
      }
      
      sessionStorage.removeItem('checkoutItems');
      window.dispatchEvent(new Event('cartChange'));
      
      // Hien thi thong bao phu hop dua vao emailSent
      const orderCode = result?.orderCode || `#${result?.orderId}`;
      if (result?.emailSent) {
        console.log(`Email xác nhận đã được gửi cho đơn hàng ${orderCode}`);
      }
      navigate('/order-success');
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Có lỗi xảy ra trong quá trình đặt hàng. Vui lòng thử lại sau.');
      }
      window.scrollTo(0, 0);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="checkout-page">
      <div className="container pt-4 animate--fade-in">
        {/* Breadcrumb */}
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
            <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
            <li className="breadcrumb-item"><Link to="/cart" className="text-secondary text-decoration-none">Giỏ hàng</Link></li>
            <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tiến hành thanh toán</li>
          </ol>
        </nav>

        <h2 className="checkout-header-title">
          <i className="fa-solid fa-credit-card"></i> THÔNG TIN THANH TOÁN
        </h2>

        {error && (
          <div className="alert alert-danger px-4 py-3 rounded-lg mb-4 text-center font-weight-bold small" role="alert">
            <i className="fa-solid fa-circle-exclamation mr-2"></i> {error}
          </div>
        )}

        <div className="row">
          {/* CỘT TRÁI: FORM */}
          <div className="col-12 col-lg-7">
            <form onSubmit={handlePlaceOrder}>
              
              {/* KHỐI 1: THÔNG TIN LIÊN HỆ */}
              <div className="checkout-card">
                <h3 className="checkout-card-title"><i className="fa-solid fa-address-card"></i> 1. Thông tin liên hệ</h3>
                <div className="row">
                  <div className="col-12 col-md-6 checkout-form-group">
                    <label className="checkout-label">Họ và tên người nhận <span className="text-danger">*</span></label>
                    <div className="checkout-input-wrapper">
                      <span className="checkout-input-icon"><i className="fa-solid fa-user"></i></span>
                      <input 
                        type="text" 
                        className="checkout-input" 
                        placeholder="Nhập họ tên" 
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        required 
                      />
                    </div>
                  </div>
                  <div className="col-12 col-md-6 checkout-form-group">
                    <label className="checkout-label">Số điện thoại liên hệ <span className="text-danger">*</span></label>
                    <div className="checkout-input-wrapper">
                      <span className="checkout-input-icon"><i className="fa-solid fa-phone"></i></span>
                      <input 
                        type="tel" 
                        className="checkout-input" 
                        placeholder="Nhập số điện thoại" 
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        required 
                      />
                    </div>
                  </div>
                </div>
                <div className="checkout-form-group mb-0">
                  <label className="checkout-label">Địa chỉ Email</label>
                  <div className="checkout-input-wrapper">
                    <span className="checkout-input-icon"><i className="fa-solid fa-envelope"></i></span>
                    <input 
                      type="email" 
                      className="checkout-input" 
                      placeholder="nguyenvanan@gmail.com" 
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                    />
                  </div>
                </div>
              </div>

              {/* KHỐI 2: ĐỊA CHỈ GIAO HÀNG */}
              <div className="checkout-card">
                <h3 className="checkout-card-title"><i className="fa-solid fa-truck-fast"></i> 2. Địa chỉ giao hàng</h3>
                
                {savedAddresses.length > 0 && (
                  <div className="checkout-saved-address">
                    <label><i className="fa-solid fa-bookmark"></i> Chọn nhanh địa chỉ đã lưu</label>
                    <select 
                      className="form-control rounded-lg px-3 shadow-none border-secondary-50"
                      onChange={handleSelectSavedAddress}
                      value={selectedAddressId || ''}
                    >
                      <option value="">-- Sử dụng thông tin cá nhân --</option>
                      {savedAddresses.map(addr => (
                        <option key={addr.id} value={addr.id}>
                          [{addr.addressType}] {addr.recipientName} - {addr.phoneNumber} ({addr.addressLine}, {addr.wardName}, {addr.districtName}, {addr.provinceName}){addr.isDefault ? ' [Mặc định]' : ''}
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                <div className="checkout-form-group mb-3">
                  <label className="checkout-label">Tỉnh / Thành phố <span className="text-danger">*</span></label>
                  <SearchableSelect 
                    options={provincesList}
                    value={province}
                    onChange={(val) => {
                      setProvince(val);
                      setDistrict('');
                      setWard('');
                    }}
                    placeholder="Chọn Tỉnh/Thành"
                    icon="fa-solid fa-map-location-dot"
                  />
                </div>

                <div className="row">
                  <div className="col-12 col-md-6 checkout-form-group">
                    <label className="checkout-label">Quận / Huyện <span className="text-danger">*</span></label>
                    <SearchableSelect 
                      options={availableDistricts}
                      value={district}
                      onChange={(val) => {
                        setDistrict(val);
                        setWard('');
                      }}
                      placeholder="Chọn Quận/Huyện"
                      icon="fa-solid fa-building"
                      disabled={!province}
                    />
                  </div>
                  <div className="col-12 col-md-6 checkout-form-group">
                    <label className="checkout-label">Phường / Xã <span className="text-danger">*</span></label>
                    <SearchableSelect 
                      options={availableWards}
                      value={ward}
                      onChange={(val) => setWard(val)}
                      placeholder="Chọn Phường/Xã"
                      icon="fa-solid fa-map"
                      disabled={!district}
                    />
                  </div>
                </div>

                <div className="checkout-form-group mb-0">
                  <label className="checkout-label">Địa chỉ giao hàng chi tiết <span className="text-danger">*</span></label>
                  <div className="checkout-input-wrapper">
                    <span className="checkout-input-icon"><i className="fa-solid fa-house-chimney"></i></span>
                    <input 
                      type="text" 
                      className="checkout-input" 
                      placeholder="Số nhà, tên đường, tòa nhà, căn hộ..." 
                      value={address}
                      onChange={(e) => setAddress(e.target.value)}
                      required 
                    />
                  </div>
                </div>
              </div>

              {/* KHỐI 3: GHI CHÚ ĐƠN HÀNG */}
              <div className="checkout-card">
                <h3 className="checkout-card-title"><i className="fa-solid fa-clipboard"></i> 3. Ghi chú cho đơn hàng</h3>
                <div className="checkout-form-group mb-0">
                  <textarea 
                    className="checkout-textarea" 
                    placeholder="Ví dụ: Gọi điện cho tôi trước khi giao, giao vào giờ hành chính..."
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                  ></textarea>
                </div>
              </div>

              {/* KHỐI 4: THANH TOÁN */}
              <div className="checkout-card">
                <h3 className="checkout-card-title"><i className="fa-solid fa-wallet"></i> 4. Phương thức thanh toán</h3>
                
                <div className="payment-methods-grid mb-0">
                  <div 
                    className={`payment-method-box ${paymentMethod === 'cod' ? 'selected' : ''}`}
                    onClick={() => setPaymentMethod('cod')}
                  >
                    <div className="payment-method-icon"><i className="fa-solid fa-hand-holding-dollar"></i></div>
                    <div className="payment-method-info">
                      <h6>Thanh toán khi nhận hàng</h6>
                      <p>Thanh toán COD</p>
                    </div>
                  </div>
                  <div 
                    className={`payment-method-box ${paymentMethod === 'bank' ? 'selected' : ''}`}
                    onClick={() => setPaymentMethod('bank')}
                  >
                    <div className="payment-method-icon"><i className="fa-solid fa-qrcode"></i></div>
                    <div className="payment-method-info">
                      <h6>Chuyển khoản Ngân hàng</h6>
                      <p>Quét mã QR Code</p>
                    </div>
                  </div>
                </div>
              </div>

            </form>
          </div>

          {/* CỘT PHẢI: TỔNG KẾT */}
          <div className="col-12 col-lg-5">
            <div className="order-summary-card">
              <h3 className="checkout-card-title"><i className="fa-solid fa-cart-shopping"></i> Đơn hàng của bạn</h3>
              
              <div className="order-item-list">
                {cartItems.map((item) => (
                  <div className="order-item" key={item.id}>
                    <img src={item.imageUrl || "https://placehold.co/150x150/e9ecef/6c757d?text=No+Image"} alt={item.name} className="order-item-img" />
                    <div className="order-item-details">
                      <h4 className="order-item-name">{item.name}</h4>
                      <span className="order-item-qty">Số lượng: <strong>{item.quantity}</strong></span>
                    </div>
                    <div className="order-item-price">
                      {new Intl.NumberFormat('vi-VN').format(item.price * item.quantity)} ₫
                    </div>
                  </div>
                ))}
              </div>

              <div className="summary-row">
                <span>Tạm tính hàng hóa:</span>
                <span className="font-weight-bold text-dark">{new Intl.NumberFormat('vi-VN').format(subTotal)} ₫</span>
              </div>
              
              <div className="summary-row total">
                <span className="lbl">Tổng thanh toán:</span>
                <span className="val">{new Intl.NumberFormat('vi-VN').format(total)} ₫</span>
              </div>

              <button 
                type="button" 
                className="btn-checkout-submit" 
                disabled={loading}
                onClick={handlePlaceOrder}
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                    Đang xử lý...
                  </>
                ) : (
                  <>
                    Hoàn tất đặt hàng <i className="fa-solid fa-arrow-right"></i>
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Checkout;
