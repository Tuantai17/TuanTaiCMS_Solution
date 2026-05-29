import React from 'react';
import { Link } from 'react-router-dom';

const Cart = () => {
  // Giả lập 2 sản phẩm có sẵn trong giỏ hàng
  const cartItems = [
    {
      id: 1,
      name: "Đồ chơi lắp ráp máy bay trực thăng cứu hộ thông minh",
      price: 1250000,
      quantity: 1,
      sku: "SKU: #1004",
      imageUrl: "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=150&q=80"
    },
    {
      id: 2,
      name: "Khối lắp ghép Lego Ninjago Rồng Thần Hộ Mệnh cực đẹp",
      price: 2290000,
      quantity: 2,
      sku: "SKU: #1005",
      imageUrl: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=150&q=80"
    }
  ];

  const subTotal = cartItems.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  const shippingFee = 35000;
  const total = subTotal + shippingFee;

  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Giỏ hàng của bạn</li>
        </ol>
      </nav>

      <h2 className="font-weight-bold text-dark text-uppercase mb-4">
        <i className="fa-solid fa-basket-shopping text-danger mr-2"></i> Giỏ hàng của bạn
      </h2>

      <div className="row">
        {/* CỘT TRÁI: DANH SÁCH SẢN PHẨM TRONG GIỎ */}
        <div className="col-12 col-lg-8 mb-4">
          <div className="card shadow-sm border border-light rounded-lg overflow-hidden">
            <div className="card-header bg-danger text-white py-3 px-4">
              <h6 className="card-title font-weight-bold text-uppercase mb-0" style={{ fontSize: '0.9rem' }}>
                Danh sách vật phẩm ({cartItems.length})
              </h6>
            </div>
            <div className="card-body p-0">
              {cartItems.map((item) => (
                <div className="d-flex flex-column flex-sm-row align-items-center justify-content-between p-4 border-bottom" key={item.id}>
                  {/* Ảnh sản phẩm */}
                  <img src={item.imageUrl} alt={item.name} className="rounded border mb-3 mb-sm-0" style={{ width: '80px', height: '80px', objectFit: 'cover' }} />
                  
                  {/* Tên & SKU */}
                  <div className="flex-grow-1 pl-sm-4 text-center text-sm-left" style={{ maxWidth: '320px' }}>
                    <h6 className="font-weight-bold text-dark mb-1" style={{ fontSize: '0.9rem' }}>{item.name}</h6>
                    <span className="text-secondary small">{item.sku}</span>
                  </div>
                  
                  {/* Số lượng tăng giảm */}
                  <div className="d-flex align-items-center my-3 my-sm-0">
                    <button className="btn btn-sm btn-outline-secondary rounded-circle px-2 py-1"><i className="fa-solid fa-minus" style={{ fontSize: '0.75rem' }}></i></button>
                    <span className="mx-3 font-weight-bold">{item.quantity}</span>
                    <button className="btn btn-sm btn-outline-secondary rounded-circle px-2 py-1"><i className="fa-solid fa-plus" style={{ fontSize: '0.75rem' }}></i></button>
                  </div>
                  
                  {/* Đơn giá & Nút xóa */}
                  <div className="text-center text-sm-right pl-sm-3">
                    <p className="text-danger font-weight-bold mb-1" style={{ fontSize: '0.95rem' }}>
                      {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.price * item.quantity)}
                    </p>
                    <button className="btn btn-sm text-muted hover-danger border-0 bg-transparent p-0"><i className="fa-solid fa-trash mr-1"></i> Xóa</button>
                  </div>
                </div>
              ))}
            </div>
          </div>
          
          <div className="mt-4">
            <Link to="/products" className="btn btn-outline-danger rounded-pill px-4 font-weight-bold text-uppercase" style={{ fontSize: '0.8rem' }}>
              <i className="fa-solid fa-chevron-left mr-2"></i> Tiếp tục mua sắm
            </Link>
          </div>
        </div>

        {/* CỘT PHẢI: TÍNH TỔNG TIỀN & ĐẶT HÀNG */}
        <div className="col-12 col-lg-4">
          <div className="card shadow-sm border border-light rounded-lg overflow-hidden p-4">
            <h5 className="font-weight-bold text-dark border-bottom pb-3 mb-4">Tóm tắt đơn hàng</h5>
            
            {/* Các chi phí phụ */}
            <div className="d-flex justify-content-between mb-3 text-secondary" style={{ fontSize: '0.9rem' }}>
              <span>Tạm tính hàng hóa:</span>
              <span className="font-weight-bold text-dark">{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(subTotal)}</span>
            </div>
            <div className="d-flex justify-content-between mb-4 text-secondary" style={{ fontSize: '0.9rem' }}>
              <span>Phí vận chuyển dự kiến:</span>
              <span className="font-weight-bold text-dark">{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(shippingFee)}</span>
            </div>
            
            {/* Tổng tiền thanh toán */}
            <div className="d-flex justify-content-between border-top pt-3 mb-4">
              <span className="font-weight-bold text-dark">Tổng cộng thanh toán:</span>
              <span className="h4 font-weight-extrabold text-danger mb-0">
                {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(total)}
              </span>
            </div>
            
            {/* Nút đặt hàng */}
            <Link to="/checkout" className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3" style={{ fontSize: '0.85rem' }}>
              Tiến hành thanh toán <i className="fa-solid fa-arrow-right ml-2"></i>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;
