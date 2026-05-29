import React from 'react';
import { Link, NavLink } from 'react-router-dom';

const Header = () => {
  return (
    <header className="main-header-wrapper shadow-sm">
      {/* 1. TOP BAR (Màu xanh đậm đặc trưng MyKingdom) */}
      <div className="top-bar-navy py-1 text-white font-weight-light" style={{ backgroundColor: '#002664', fontSize: '0.85rem' }}>
        <div className="container d-flex justify-content-between align-items-center">
          <div className="top-bar-left d-flex align-items-center gap-3">
            <span className="mr-3"><i className="fa-solid fa-bolt text-warning mr-1"></i> Giao hàng hỏa tốc 4 tiếng</span>
            <span><i className="fa-solid fa-users text-info mr-1"></i> Chương trình thành viên</span>
          </div>
          <div className="top-bar-right d-flex align-items-center">
            <span className="mr-3"><i className="fa-solid fa-hand-holding-dollar mr-1"></i> Mua hàng trả góp</span>
            <span><i className="fa-solid fa-store mr-1"></i> Hệ thống 200 cửa hàng</span>
          </div>
        </div>
      </div>

      {/* 2. MAIN HEADER (Màu đỏ tươi nổi bật) */}
      <div className="main-header-red py-3" style={{ backgroundColor: '#c80f1e' }}>
        <div className="container">
          <div className="row align-items-center">
            {/* Logo phong cách MyKingdom đầy màu sắc */}
            <div className="col-12 col-md-3 text-center text-md-left mb-2 mb-md-0">
              <Link to="/" className="text-decoration-none d-inline-block">
                <span className="logo-mykingdom bg-white px-3 py-2 rounded-lg d-inline-block shadow-sm">
                  <span className="font-weight-extrabold text-uppercase" style={{ fontSize: '1.4rem', letterSpacing: '1px' }}>
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
                </span>
              </Link>
            </div>

            {/* Thanh tìm kiếm lớn ở giữa */}
            <div className="col-12 col-md-6 mb-2 mb-md-0">
              <div className="input-group search-bar-wrapper rounded-pill overflow-hidden bg-white px-2 py-1 shadow-sm">
                <div className="input-group-prepend border-0 bg-transparent">
                  <span className="input-group-text border-0 bg-transparent text-muted"><i className="fa-solid fa-magnifying-glass"></i></span>
                </div>
                <input
                  type="text"
                  className="form-control border-0 bg-transparent shadow-none"
                  placeholder="Nhập từ khóa để tìm kiếm (ví dụ: lắp ráp, mô hình, ba lô...)"
                  style={{ fontSize: '0.9rem' }}
                />
                <div className="input-group-append">
                  <button className="btn btn-warning rounded-pill px-4 text-dark font-weight-bold" style={{ fontSize: '0.85rem' }}>Tìm kiếm</button>
                </div>
              </div>
            </div>

            {/* Icons Tài khoản, Giỏ hàng, Ngôn ngữ bên phải */}
            <div className="col-12 col-md-3 d-flex justify-content-center justify-content-md-end align-items-center gap-4 text-white">
              <a href="#" className="text-white text-decoration-none mr-4" title="Tài khoản">
                <i className="fa-solid fa-user fs-5"></i>
              </a>
              <Link to="/cart" className="text-white text-decoration-none mr-4 position-relative" title="Giỏ hàng">
                <i className="fa-solid fa-bag-shopping fs-5"></i>
                <span className="position-absolute translate-middle badge rounded-pill bg-warning text-dark border-light" style={{ top: '-10px', right: '-12px', fontSize: '0.7rem' }}>2</span>
              </Link>
              <div className="d-flex align-items-center cursor-pointer">
                <img src="https://flagcdn.com/w20/vn.png" alt="VN" className="mr-1" />
                <i className="fa-solid fa-caret-down text-white-50" style={{ fontSize: '0.8rem' }}></i>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* 3. MENU BAR (Đỏ tươi cùng tông, phẳng và thoáng đãng) */}
      <nav className="navbar navbar-expand-lg py-1 shadow-sm" style={{ backgroundColor: '#b00c19', fontSize: '0.95rem' }}>
        <div className="container">
          <button className="navbar-toggler text-white border-white-50" type="button" data-toggle="collapse" data-target="#navbarNav">
            <span className="navbar-toggler-icon d-flex align-items-center justify-content-center"><i className="fa-solid fa-bars text-white"></i></span>
          </button>
          
          <div className="collapse navbar-collapse" id="navbarNav">
            <ul className="navbar-nav w-100 d-flex justify-content-between text-uppercase font-weight-bold">
              <li className="nav-item">
                <NavLink to="/" end className={({ isActive }) => isActive ? "nav-link text-white px-3 py-2 active-nav-link" : "nav-link text-white px-3 py-2"}>
                  <i className="fa-solid fa-house mr-1"></i> Trang chủ
                </NavLink>
              </li>
              <li className="nav-item">
                <a href="#" className="nav-link text-white-50 px-3 py-2 cursor-not-allowed">
                  <i className="fa-solid fa-star text-warning mr-1"></i> Độc quyền Online
                </a>
              </li>
              <li className="nav-item">
                <a href="#" className="nav-link text-white-50 px-3 py-2 cursor-not-allowed">
                  <i className="fa-solid fa-cubes text-info mr-1"></i> Lego
                </a>
              </li>
              <li className="nav-item">
                <a href="#" className="nav-link text-white-50 px-3 py-2 cursor-not-allowed">
                  Hàng mới
                </a>
              </li>
              <li className="nav-item">
                <NavLink to="/products" className={({ isActive }) => isActive ? "nav-link text-white px-3 py-2 active-nav-link" : "nav-link text-white px-3 py-2"}>
                  Sản phẩm
                </NavLink>
              </li>
              <li className="nav-item">
                <a href="#" className="nav-link text-white-50 px-3 py-2 cursor-not-allowed">
                  Khuyến mãi
                </a>
              </li>
              <li className="nav-item">
                <a href="#" className="nav-link text-white-50 px-3 py-2 cursor-not-allowed">
                  Thương hiệu
                </a>
              </li>
              <li className="nav-item">
                <NavLink to="/blog" className={({ isActive }) => isActive ? "nav-link text-white px-3 py-2 active-nav-link" : "nav-link text-white px-3 py-2"}>
                  Blog / Tin tức
                </NavLink>
              </li>
              <li className="nav-item">
                <span className="badge badge-warning text-dark font-weight-bold px-3 py-2 rounded-pill shadow-sm cursor-pointer align-self-center mt-1 mt-lg-0">
                  % OUTLET
                </span>
              </li>
            </ul>
          </div>
        </div>
      </nav>
    </header>
  );
};

export default Header;
