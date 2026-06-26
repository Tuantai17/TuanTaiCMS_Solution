import React from 'react';
import { Link } from 'react-router-dom';
import aboutHero from '../assets/images/mykingdom_about_hero.png';

const AboutPage = () => {
  return (
    <div className="about-page" style={{ backgroundColor: '#fcfcfc' }}>
      {/* Hero Section */}
      <section className="py-5 bg-white">
        <div className="container py-lg-5">
          <div className="row align-items-center">
            <div className="col-lg-6 mb-5 mb-lg-0 pr-lg-5">
              <span className="badge badge-danger px-3 py-2 text-uppercase mb-3 shadow-sm" style={{ letterSpacing: '1px', backgroundColor: '#e63946' }}>
                Về Chúng Tôi
              </span>
              <h1 className="font-weight-extrabold mb-4" style={{ fontSize: '3rem', color: '#1d3557', lineHeight: '1.2' }}>
                Khám Phá <span style={{ color: '#e63946' }}>Vương Quốc</span> Đồ Chơi MyKingdom
              </h1>
              <p className="lead text-secondary mb-4" style={{ lineHeight: '1.8', fontSize: '1.1rem' }}>
                MyKingdom không chỉ là một hệ thống cửa hàng, mà là nơi hiện thực hóa những giấc mơ của trẻ thơ. Chúng tôi tự hào mang đến những sản phẩm đồ chơi an toàn, chất lượng và mang tính giáo dục cao nhất từ các thương hiệu hàng đầu thế giới.
              </p>
              <Link to="/products" className="btn btn-primary btn-lg px-5 py-3 rounded-pill font-weight-bold shadow-sm" style={{ backgroundColor: '#1d3557', borderColor: '#1d3557' }}>
                Khám phá sản phẩm ngay <i className="fa-solid fa-arrow-right ml-2"></i>
              </Link>
            </div>
            <div className="col-lg-6">
              <div className="position-relative">
                <div className="position-absolute rounded-circle" style={{ width: '100%', height: '100%', backgroundColor: '#f1faee', top: '10%', left: '-5%', zIndex: 0 }}></div>
                <img
                  src={aboutHero}
                  alt="MyKingdom Store Interior"
                  className="img-fluid rounded-lg shadow-lg position-relative"
                  style={{ zIndex: 1, objectFit: 'cover', border: '5px solid white' }}
                />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-5" style={{ backgroundColor: '#1d3557' }}>
        <div className="container">
          <div className="row text-center text-white">
            <div className="col-6 col-md-3 mb-4 mb-md-0">
              <h2 className="font-weight-bold mb-2" style={{ color: '#f1faee', fontSize: '2.5rem' }}>10+</h2>
              <p className="text-uppercase mb-0" style={{ letterSpacing: '1px', color: '#a8dadc' }}>Năm Kinh Nghiệm</p>
            </div>
            <div className="col-6 col-md-3 mb-4 mb-md-0">
              <h2 className="font-weight-bold mb-2" style={{ color: '#f1faee', fontSize: '2.5rem' }}>500+</h2>
              <p className="text-uppercase mb-0" style={{ letterSpacing: '1px', color: '#a8dadc' }}>Thương Hiệu</p>
            </div>
            <div className="col-6 col-md-3">
              <h2 className="font-weight-bold mb-2" style={{ color: '#f1faee', fontSize: '2.5rem' }}>100+</h2>
              <p className="text-uppercase mb-0" style={{ letterSpacing: '1px', color: '#a8dadc' }}>Cửa Hàng Toàn Quốc</p>
            </div>
            <div className="col-6 col-md-3">
              <h2 className="font-weight-bold mb-2" style={{ color: '#f1faee', fontSize: '2.5rem' }}>1M+</h2>
              <p className="text-uppercase mb-0" style={{ letterSpacing: '1px', color: '#a8dadc' }}>Khách Hàng Tin Dùng</p>
            </div>
          </div>
        </div>
      </section>

      {/* Vision & Mission Section */}
      <section className="py-5 my-lg-5">
        <div className="container">
          <div className="text-center mb-5">
            <h2 className="font-weight-bold" style={{ color: '#1d3557' }}>Tầm Nhìn & Sứ Mệnh</h2>
            <p className="text-secondary mx-auto" style={{ maxWidth: '600px' }}>Đồng hành cùng hàng triệu gia đình Việt trong hành trình phát triển toàn diện của bé.</p>
          </div>
          
          <div className="row">
            <div className="col-md-6 mb-4">
              <div className="card h-100 border-0 shadow-sm transition-hover" style={{ borderRadius: '15px', overflow: 'hidden' }}>
                <div className="card-body p-5">
                  <div className="d-inline-block p-3 rounded-circle mb-4" style={{ backgroundColor: '#e6394615', color: '#e63946' }}>
                    <i className="fa-solid fa-eye fa-2x"></i>
                  </div>
                  <h4 className="font-weight-bold mb-3" style={{ color: '#1d3557' }}>Tầm Nhìn</h4>
                  <p className="text-secondary" style={{ lineHeight: '1.7' }}>
                    Trở thành điểm đến mua sắm đồ chơi trẻ em uy tín và lớn nhất tại Việt Nam. Chúng tôi không ngừng mở rộng hệ thống, nâng cao chất lượng dịch vụ để mang đến trải nghiệm tuyệt vời nhất cho mọi khách hàng.
                  </p>
                </div>
              </div>
            </div>
            <div className="col-md-6 mb-4">
              <div className="card h-100 border-0 shadow-sm transition-hover" style={{ borderRadius: '15px', overflow: 'hidden' }}>
                <div className="card-body p-5">
                  <div className="d-inline-block p-3 rounded-circle mb-4" style={{ backgroundColor: '#457b9d15', color: '#457b9d' }}>
                    <i className="fa-solid fa-bullseye fa-2x"></i>
                  </div>
                  <h4 className="font-weight-bold mb-3" style={{ color: '#1d3557' }}>Sứ Mệnh</h4>
                  <p className="text-secondary" style={{ lineHeight: '1.7' }}>
                    Đóng góp vào sự phát triển trí tuệ và thể chất của trẻ em thông qua những món đồ chơi an toàn, chất lượng. Mỗi món đồ chơi từ MyKingdom đều là một công cụ giúp bé học hỏi và sáng tạo.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Core Values */}
      <section className="py-5 bg-white">
        <div className="container">
          <div className="text-center mb-5">
            <h2 className="font-weight-bold" style={{ color: '#1d3557' }}>Tại Sao Chọn Chúng Tôi?</h2>
          </div>
          <div className="row text-center">
            <div className="col-lg-4 mb-4">
              <div className="p-4 border border-light rounded shadow-sm h-100 bg-white">
                <i className="fa-solid fa-shield-check fa-3x mb-4 text-success"></i>
                <h5 className="font-weight-bold mb-3">100% Chính Hãng</h5>
                <p className="text-secondary">Tất cả sản phẩm đều được kiểm định an toàn, nhập khẩu chính hãng từ các thương hiệu uy tín.</p>
              </div>
            </div>
            <div className="col-lg-4 mb-4">
              <div className="p-4 border border-light rounded shadow-sm h-100 bg-white">
                <i className="fa-solid fa-truck-fast fa-3x mb-4 text-warning"></i>
                <h5 className="font-weight-bold mb-3">Giao Hàng Siêu Tốc</h5>
                <p className="text-secondary">Dịch vụ giao hàng nhanh chóng trên toàn quốc. Giao hỏa tốc trong nội thành chỉ từ 2-4 tiếng.</p>
              </div>
            </div>
            <div className="col-lg-4 mb-4">
              <div className="p-4 border border-light rounded shadow-sm h-100 bg-white">
                <i className="fa-solid fa-headset fa-3x mb-4 text-info"></i>
                <h5 className="font-weight-bold mb-3">Hỗ Trợ Tận Tâm</h5>
                <p className="text-secondary">Đội ngũ nhân viên chuyên nghiệp luôn sẵn sàng tư vấn và giải đáp mọi thắc mắc của bạn 24/7.</p>
              </div>
            </div>
          </div>
        </div>
      </section>
      
      {/* Inline styles for some hover effects */}
      <style>{`
        .transition-hover {
          transition: transform 0.3s ease, box-shadow 0.3s ease;
        }
        .transition-hover:hover {
          transform: translateY(-10px);
          box-shadow: 0 1rem 3rem rgba(0,0,0,.175)!important;
        }
      `}</style>
    </div>
  );
};

export default AboutPage;
