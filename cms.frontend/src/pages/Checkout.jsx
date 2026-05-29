import React from 'react';

const Checkout = () => {
  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item"><a href="/cart" className="text-secondary text-decoration-none">Giỏ hàng</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tiến hành thanh toán</li>
        </ol>
      </nav>

      <h2 className="font-weight-bold text-dark text-uppercase mb-4">
        <i className="fa-solid fa-credit-card text-danger mr-2"></i> Thông Tin Thanh Toán
      </h2>

      <div className="row">
        {/* CỘT TRÁI: FORM ĐIỀN THÔNG TIN KHÁCH HÀNG */}
        <div className="col-12 col-lg-7 mb-4">
          <div className="card shadow-sm border border-light rounded-lg p-4">
            <h5 className="font-weight-bold text-dark mb-4 border-bottom pb-3">Địa chỉ nhận hàng</h5>
            
            <form onSubmit={(e) => { e.preventDefault(); alert("Đặt hàng thành công! Đơn hàng của bạn đang được hệ thống xử lý."); }}>
              <div className="row">
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Họ và tên *</label>
                  <input type="text" className="form-control rounded-pill px-3 shadow-none border-secondary-50" placeholder="Nguyễn Văn A..." required />
                </div>
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Số điện thoại liên hệ *</label>
                  <input type="tel" className="form-control rounded-pill px-3 shadow-none border-secondary-50" placeholder="0912..." required />
                </div>
              </div>

              <div className="mb-3">
                <label className="small font-weight-bold text-secondary">Địa chỉ Email (Nhận hóa đơn)</label>
                <input type="email" className="form-control rounded-pill px-3 shadow-none border-secondary-50" placeholder="nguyenvanan@gmail.com..." />
              </div>

              <div className="mb-3">
                <label className="small font-weight-bold text-secondary">Địa chỉ giao hàng chi tiết *</label>
                <input type="text" className="form-control rounded-pill px-3 shadow-none border-secondary-50" placeholder="Số nhà, tên đường, phường/xã..." required />
              </div>

              <div className="row">
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Tỉnh / Thành phố *</label>
                  <select className="form-control rounded-pill px-3 shadow-none border-secondary-50" required>
                    <option value="">-- Chọn Tỉnh/Thành phố --</option>
                    <option value="HCM">Thành phố Hồ Chí Minh</option>
                    <option value="HN">Thủ đô Hà Nội</option>
                    <option value="DN">Thành phố Đà Nẵng</option>
                  </select>
                </div>
                <div className="col-12 col-md-6 mb-3">
                  <label className="small font-weight-bold text-secondary">Quận / Huyện *</label>
                  <select className="form-control rounded-pill px-3 shadow-none border-secondary-50" required>
                    <option value="">-- Chọn Quận/Huyện --</option>
                    <option value="Q1">Quận 1</option>
                    <option value="Q3">Quận 3</option>
                    <option value="Q8">Quận 8</option>
                  </select>
                </div>
              </div>

              <div className="mb-4">
                <label className="small font-weight-bold text-secondary">Ghi chú giao hàng</label>
                <textarea className="form-control rounded shadow-none border-secondary-50" rows="3" placeholder="Ví dụ: Gọi trước khi giao, giao giờ hành chính..."></textarea>
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

              <button type="submit" className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3" style={{ fontSize: '0.9rem' }}>
                <i className="fa-solid fa-circle-check mr-2"></i> Hoàn tất đặt hàng
              </button>
            </form>
          </div>
        </div>

        {/* CỘT PHẢI: TỔNG KẾT ĐƠN HÀNG TĨNH */}
        <div className="col-12 col-lg-5">
          <div className="card shadow-sm border border-light rounded-lg p-4 bg-light">
            <h5 className="font-weight-bold text-dark mb-4 border-bottom pb-3">Đơn hàng của bạn</h5>
            
            {/* Sản phẩm 1 */}
            <div className="d-flex align-items-center mb-3 pb-3 border-bottom">
              <img src="https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=80&q=80" alt="Đồ chơi 1" className="rounded border mr-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
              <div className="flex-grow-1">
                <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2">Đồ chơi lắp ráp máy bay trực thăng cứu hộ thông minh</h6>
                <span className="text-secondary small">Số lượng: 1</span>
              </div>
              <span className="font-weight-bold text-dark pl-2 small">1.250.000 ₫</span>
            </div>

            {/* Sản phẩm 2 */}
            <div className="d-flex align-items-center mb-4 pb-3 border-bottom">
              <img src="https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=80&q=80" alt="Đồ chơi 2" className="rounded border mr-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
              <div className="flex-grow-1">
                <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2">Khối lắp ghép Lego Ninjago Rồng Thần Hộ Mệnh cực đẹp</h6>
                <span className="text-secondary small">Số lượng: 2</span>
              </div>
              <span className="font-weight-bold text-dark pl-2 small">4.580.000 ₫</span>
            </div>

            {/* Chi tiết chi phí */}
            <div className="d-flex justify-content-between mb-2 text-secondary small">
              <span>Tạm tính:</span>
              <span className="font-weight-bold text-dark">5.830.000 ₫</span>
            </div>
            <div className="d-flex justify-content-between mb-3 text-secondary small">
              <span>Phí vận chuyển:</span>
              <span className="font-weight-bold text-dark">35.000 ₫</span>
            </div>
            <div className="d-flex justify-content-between border-top pt-3">
              <span className="font-weight-bold text-dark">Tổng thanh toán:</span>
              <span className="h4 font-weight-extrabold text-danger mb-0">5.865.000 ₫</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Checkout;
