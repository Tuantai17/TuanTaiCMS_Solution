import React from 'react';
import { Link } from 'react-router-dom';

const Footer = () => {
  return (
    <footer className="footer-section mt-5 border-top pt-5" style={{ backgroundColor: '#f8f9fa' }}>
      <div className="container">
        <div className="row text-secondary" style={{ fontSize: '0.9rem' }}>
          {/* Cột 1: Thông tin cửa hàng */}
          <div className="col-12 col-md-4 mb-4">
            <h5 className="text-dark font-weight-bold text-uppercase mb-3" style={{ fontSize: '1rem', letterSpacing: '0.5px' }}>
              <span className="font-weight-extrabold text-uppercase">
                <span style={{ color: '#ff2d55' }}>M</span>
                <span style={{ color: '#ff9500' }}>Y</span>
                <span style={{ color: '#4cd964' }}>K</span>
                <span style={{ color: '#5ac8fa' }}>I</span>
                <span style={{ color: '#007aff' }}>N</span>
                <span style={{ color: '#5856d6' }}>G</span>
                <span style={{ color: '#ffcc00' }}>D</span>
                <span style={{ color: '#ff3b30' }}>O</span>
                <span style={{ color: '#ff2d55' }}>M</span>
              </span>
            </h5>
            <p className="lh-lg">
              Hệ thống cửa hàng đồ chơi MyKingdom - Vương quốc đồ chơi cao cấp chính hãng từ các thương hiệu hàng đầu thế giới dành cho mọi lứa tuổi của trẻ em Việt Nam.
            </p>
            <p className="mb-1"><i className="fa-solid fa-location-dot text-danger mr-2"></i> 180 Cao Lỗ, Phường 4, Quận 8, TP. Hồ Chí Minh</p>
            <p className="mb-1"><i className="fa-solid fa-phone text-success mr-2"></i> Hotline: 1900 1208</p>
            <p className="mb-0"><i className="fa-solid fa-envelope text-info mr-2"></i> hotro@mykingdom.vn</p>
          </div>

          {/* Cột 2: Hỗ trợ khách hàng */}
          <div className="col-12 col-md-3 mb-4 pl-md-5">
            <h6 className="text-dark font-weight-bold text-uppercase mb-3" style={{ fontSize: '0.95rem' }}>Hỗ trợ khách hàng</h6>
            <ul className="list-unstyled lh-lg">
              <li><Link to="/chinh-sach-bao-hanh" className="text-secondary text-decoration-none hover-dark">Chính sách bảo hành</Link></li>
              <li><Link to="/doi-tra-trong-7-ngay" className="text-secondary text-decoration-none hover-dark">Chính sách đổi trả</Link></li>
              <li><Link to="/chinh-sach-van-chuyen" className="text-secondary text-decoration-none hover-dark">Chính sách vận chuyển</Link></li>
              <li><Link to="/phuong-thuc-thanh-toan" className="text-secondary text-decoration-none hover-dark">Phương thức thanh toán</Link></li>
            </ul>
          </div>

          {/* Cột 3: Về chúng tôi */}
          <div className="col-12 col-md-2 mb-4">
            <h6 className="text-dark font-weight-bold text-uppercase mb-3" style={{ fontSize: '0.95rem' }}>Về chúng tôi</h6>
            <ul className="list-unstyled lh-lg">
              <li><Link to="/gioi-thieu" className="text-secondary text-decoration-none hover-dark">Giới thiệu MyKingdom</Link></li>
              <li><Link to="/he-thong-cua-hang" className="text-secondary text-decoration-none hover-dark">Hệ thống cửa hàng</Link></li>
              <li><Link to="/blog" className="text-secondary text-decoration-none hover-dark">Tin tức & Sự kiện</Link></li>
              <li><Link to="/lien-he" className="text-secondary text-decoration-none hover-dark">Liên hệ hợp tác</Link></li>
            </ul>
          </div>

          {/* Cột 4: Đăng ký nhận tin & Mạng xã hội */}
          <div className="col-12 col-md-3 mb-4">
            <h6 className="text-dark font-weight-bold text-uppercase mb-3" style={{ fontSize: '0.95rem' }}>Đăng ký nhận tin</h6>
            <p className="small">Hãy đăng ký để nhận thông tin khuyến mãi và các mẫu đồ chơi mới nhất từ MyKingdom.</p>
            <div className="input-group mb-3">
              <input type="email" className="form-control form-control-sm border-secondary shadow-none" placeholder="Email của bạn..." />
              <button className="btn btn-danger btn-sm text-uppercase font-weight-bold px-3">Gửi</button>
            </div>
            <div className="social-icons d-flex gap-3 fs-5 mt-2">
              <a href="#" className="text-primary mr-3"><i className="fa-brands fa-facebook"></i></a>
              <a href="#" className="text-danger mr-3"><i className="fa-brands fa-youtube"></i></a>
              <a href="#" className="text-info mr-3"><i className="fa-brands fa-square-instagram"></i></a>
              <a href="#" className="text-dark mr-3"><i className="fa-brands fa-tiktok"></i></a>
            </div>
          </div>
        </div>

        {/* Chân trang bản quyền */}
        <div className="border-top py-4 text-center text-secondary small">
          <p className="mb-1">© {new Date().getFullYear()} MyKingdom. Phát triển bởi Sinh Viên Nguyễn Tuấn Tài - Lớp CCQ2311E.</p>
          <p className="mb-0 text-muted">Dự án được kết nối Real-time với SQL Server thông qua ASP.NET Core Web API.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
