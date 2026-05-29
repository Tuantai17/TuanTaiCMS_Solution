import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import categoryProductService from '../services/categoryProductService';
import productService from '../services/productService';
import blogService from '../services/blogService';
import ProductCard from '../components/ProductCard';

// Thêm các thành phần và module Swiper
import { Swiper, SwiperSlide } from 'swiper/react';
import { Autoplay, Navigation, Pagination } from 'swiper/modules';

// Import CSS của Swiper
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

// Import 8 banner MyKingdom thực tế từ tài sản tĩnh nội bộ (Local Assets)
import banner1 from '../assets/images/banner1.webp';
import banner2 from '../assets/images/banner2.webp';
import banner3 from '../assets/images/banner3.webp';
import banner4 from '../assets/images/banner4.webp';
import banner5 from '../assets/images/banner5.webp';
import banner6 from '../assets/images/banner6.webp';
import banner7 from '../assets/images/banner7.jpg';
import banner8 from '../assets/images/banner8.jpg';

const Home = () => {
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchHomeData = async () => {
      try {
        setLoading(true);
        const [catData, prodData, postData] = await Promise.all([
          categoryProductService.getAllCategoryProducts(),
          productService.getAllProducts(),
          blogService.getAllPosts()
        ]);
        setCategories(catData.slice(0, 6)); // Lấy tối đa 6 danh mục
        setProducts(prodData.slice(0, 8));    // Lấy tối đa 8 sản phẩm hot
        setPosts(postData.slice(0, 3));       // Lấy tối đa 3 bài viết nổi bật
      } catch (error) {
        console.error("Lỗi khi tải dữ liệu trang chủ:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchHomeData();
  }, []);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang tải không gian trải nghiệm MyKingdom...</p>
      </div>
    );
  }

  // Mảng 8 banner chính thức của MyKingdom
  const banners = [
    { src: banner1, title: "Độc Quyền Website", desc: "Quốc tế thiếu nhi - Quà to giá nhỏ!" },
    { src: banner2, title: "Freeship Đơn Hàng", desc: "Freeship 20k cho mọi đơn hàng online" },
    { src: banner3, title: "Lego Playground 2026", desc: "Sân chơi Lego sáng tạo đầy màu sắc" },
    { src: banner4, title: "Children's Day 2026", desc: "Top 20 sản phẩm đồ chơi bán chạy nhất" },
    { src: banner5, title: "Xoay Nắn Sáng Tạo Tangle", desc: "Đồ chơi Tangle xoay nắn thư giãn thông minh" },
    { src: banner6, title: "Art Puzzles", desc: "Thế giới tranh ghép hình nghệ thuật cao cấp" },
    { src: banner7, title: "Lego Preschool", desc: "Bộ sản phẩm Lego giáo dục sớm cho trẻ nhỏ" },
    { src: banner8, title: "Clever Hippo Collection", desc: "Ba lô và phụ kiện học đường cao cấp" }
  ];

  return (
    <div className="home-page-container">
      {/* 1. HERO BANNER CAROUSEL (Sử dụng 8 banner MyKingdom thực tế & Swiper Slider) */}
      <div className="container mt-4">
        <div className="position-relative overflow-hidden rounded-lg shadow-sm animate--fade-in">
          <Swiper
            modules={[Autoplay, Navigation, Pagination]}
            speed={800}
            autoplay={{
              delay: 5000,
              disableOnInteraction: false,
            }}
            loop={true}
            navigation={{
              nextEl: '.swiper-button-next-custom',
              prevEl: '.swiper-button-prev-custom',
            }}
            pagination={{
              el: '.swiper-pagination-dots-custom',
              clickable: true,
            }}
            className="mySwiper1"
          >
            {banners.map((banner, idx) => (
              <SwiperSlide key={idx}>
                <Link to="/products" className="d-block position-relative">
                  <div className="image-content">
                    <img 
                      src={banner.src} 
                      className="d-block w-100 img-fluid" 
                      alt={banner.title} 
                      style={{ 
                        objectFit: 'cover',
                        width: '100%',
                        height: 'auto'
                      }} 
                    />
                  </div>
                </Link>
              </SwiperSlide>
            ))}
          </Swiper>

          {/* Custom Navigation - Nút Tròn Viền Đỏ & Mũi Tên Đỏ Chuẩn MyKingdom */}
          <div className="swiper-button-prev-custom" style={{
            position: 'absolute',
            top: '50%',
            left: '20px',
            transform: 'translateY(-50%)',
            zIndex: 10,
            cursor: 'pointer',
            transition: 'all 0.3s ease'
          }}>
            <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="1.5" y="1.5" width="45" height="45" rx="22.5" stroke="#CF102D" stroke-width="3"></rect>
              <path d="M20.0607 25.0607L24.4393 29.4393C25.3843 30.3843 27 29.715 27 28.3787L27 19.6213C27 18.285 25.3843 17.6157 24.4393 18.5607L20.0607 22.9393C19.4749 23.5251 19.4749 24.4749 20.0607 25.0607Z" fill="#CF102D"></path>
            </svg>
          </div>

          <div className="swiper-button-next-custom" style={{
            position: 'absolute',
            top: '50%',
            right: '20px',
            transform: 'translateY(-50%) rotate(180deg)',
            zIndex: 10,
            cursor: 'pointer',
            transition: 'all 0.3s ease'
          }}>
            <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="1.5" y="1.5" width="45" height="45" rx="22.5" stroke="#CF102D" stroke-width="3"></rect>
              <path d="M20.0607 25.0607L24.4393 29.4393C25.3843 30.3843 27 29.715 27 28.3787L27 19.6213C27 18.285 25.3843 17.6157 24.4393 18.5607L20.0607 22.9393C19.4749 23.5251 19.4749 24.4749 20.0607 25.0607Z" fill="#CF102D"></path>
            </svg>
          </div>

          {/* Custom Pagination - Các Chấm Tròn Chuyển Động */}
          <div className="swiper-pagination-dots-custom" style={{
            position: 'absolute',
            bottom: '20px',
            left: '50%',
            transform: 'translateX(-50%)',
            zIndex: 10,
            display: 'flex',
            gap: '8px'
          }}></div>
        </div>
      </div>

      {/* 2. CHƯƠNG TRÌNH KHUYẾN MÃI BANNER NHỎ */}
      <div className="container mt-4">
        <div className="row">
          <div className="col-12 col-md-4 mb-3">
            <div className="card border-0 bg-danger text-white p-3 rounded-lg text-center shadow-sm" style={{ background: 'linear-gradient(135deg, #c80f1e, #ff4d5a)' }}>
              <h5 className="font-weight-bold mb-1"><i className="fa-solid fa-gift mr-2"></i> QUÀ 1/6 TẶNG BÉ</h5>
              <p className="small mb-0 opacity-75">Tặng ngay hộp quà bí mật trị giá 250k</p>
            </div>
          </div>
          <div className="col-12 col-md-4 mb-3">
            <div className="card border-0 bg-primary text-white p-3 rounded-lg text-center shadow-sm" style={{ background: 'linear-gradient(135deg, #002664, #0056b3)' }}>
              <h5 className="font-weight-bold mb-1"><i className="fa-solid fa-truck-fast mr-2"></i> GIAO HỎA TỐC</h5>
              <p className="small mb-0 opacity-75">Miễn phí ship các đơn nội thành HCM dưới 5km</p>
            </div>
          </div>
          <div className="col-12 col-md-4 mb-3">
            <div className="card border-0 bg-warning text-dark p-3 rounded-lg text-center shadow-sm" style={{ background: 'linear-gradient(135deg, #ffcc00, #ff9500)' }}>
              <h5 className="font-weight-bold mb-1"><i className="fa-solid fa-percent mr-2"></i> SIÊU SALE HÀNG TUẦN</h5>
              <p className="small mb-0 font-weight-medium">Giảm giá cực sâu lên tới 50% hàng Outlet</p>
            </div>
          </div>
        </div>
      </div>

      {/* 3. LƯỚI DANH MỤC NỔI BẬT */}
      <div className="container mt-5">
        <h4 className="text-uppercase font-weight-bold text-dark border-left border-danger pl-3 mb-4" style={{ borderWidth: '4px !important' }}>
          Danh mục nổi bật
        </h4>
        <div className="row justify-content-center">
          {categories.map((item) => (
            <div className="col-6 col-sm-4 col-md-2 text-center mb-4" key={item.id}>
              <Link to="/products" className="text-decoration-none d-block card-category-item">
                <div className="category-circle-wrapper mx-auto mb-2 bg-light d-flex align-items-center justify-content-center shadow-sm rounded-circle border" style={{ width: '100px', height: '100px', transition: 'all 0.3s' }}>
                  <i className="fa-solid fa-puzzle-piece text-danger fs-3"></i>
                </div>
                <span className="font-weight-bold text-dark small text-uppercase" style={{ fontSize: '0.85rem' }}>{item.name}</span>
              </Link>
            </div>
          ))}
        </div>
      </div>

      {/* 4. SẢN PHẨM MỚI NHẤT & BÁN CHẠY (Sử dụng ProductCard tái sử dụng) */}
      <div className="container mt-5">
        <div className="d-flex justify-content-between align-items-end mb-4 border-bottom pb-2">
          <h4 className="text-uppercase font-weight-bold text-dark mb-0">
            <i className="fa-solid fa-fire text-danger mr-2"></i> Bộ sưu tập mới nhất
          </h4>
          <Link to="/products" className="btn btn-sm btn-outline-danger font-weight-bold rounded-pill px-3">Xem tất cả</Link>
        </div>
        <div className="row">
          {products.length === 0 ? (
            <div className="col-12 text-center py-4 text-muted">Chưa có sản phẩm nào.</div>
          ) : (
            products.map((item) => (
              <div className="col-12 col-sm-6 col-md-4 col-lg-3 mb-4" key={item.id}>
                <ProductCard item={item} />
              </div>
            ))
          )}
        </div>
      </div>

      {/* 5. KHU VỰC TIN TỨC & BLOG */}
      <div className="container mt-5 mb-5">
        <div className="d-flex justify-content-between align-items-end mb-4 border-bottom pb-2">
          <h4 className="text-uppercase font-weight-bold text-dark mb-0">
            <i className="fa-solid fa-newspaper text-info mr-2"></i> Blog sáng tạo & Mẹo chơi đồ chơi
          </h4>
          <Link to="/blog" className="btn btn-sm btn-outline-info font-weight-bold rounded-pill px-3">Tất cả bài viết</Link>
        </div>
        <div className="row">
          {posts.length === 0 ? (
            <div className="col-12 text-center text-muted">Chưa có tin tức nào.</div>
          ) : (
            posts.map((post) => (
              <div className="col-12 col-md-4 mb-4" key={post.id}>
                <div className="card h-100 shadow-sm border border-light rounded-lg overflow-hidden d-flex flex-column" style={{ transition: 'all 0.3s' }}>
                  <img src={post.imageUrl || "https://placehold.co/400x250/e9ecef/6c757d?text=No+Image"} className="card-img-top" alt={post.title} style={{ height: '180px', objectFit: 'cover' }} />
                  <div className="card-body p-3 d-flex flex-column justify-content-between flex-grow-1">
                    <div>
                      <span className="badge badge-info px-2 py-1 rounded small mb-2" style={{ fontSize: '0.7rem' }}>{post.categoryName || 'Tin tức'}</span>
                      <h6 className="card-title font-weight-bold text-dark text-truncate-2" style={{ fontSize: '0.95rem', height: '40px', overflow: 'hidden' }}>
                        <Link to={`/blog/${post.id}`} className="text-dark text-decoration-none hover-danger">
                          {post.title}
                        </Link>
                      </h6>
                    </div>
                    <div className="d-flex justify-content-between align-items-center text-secondary small mt-3 border-top pt-2">
                      <span><i className="fa-regular fa-calendar mr-1"></i> {new Date(post.createdDate).toLocaleDateString('vi-VN')}</span>
                      <Link to={`/blog/${post.id}`} className="font-weight-bold text-info text-decoration-none">Đọc thêm <i className="fa-solid fa-angle-right"></i></Link>
                    </div>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default Home;
