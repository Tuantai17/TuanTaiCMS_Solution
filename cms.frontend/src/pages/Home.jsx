 import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import categoryProductService from '../services/categoryProductService';
import productService from '../services/productService';
import blogService from '../services/blogService';
import bannerService from '../services/bannerService';
import ProductCard from '../components/ProductCard';
import { getMediaUrl } from '../utils/mediaUrl';

import { Swiper, SwiperSlide } from 'swiper/react';
import { Autoplay, Navigation, Pagination, Scrollbar } from 'swiper/modules';

import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import 'swiper/css/scrollbar';

import banner1 from '../assets/images/banner1.webp';
import banner2 from '../assets/images/banner2.webp';
import banner3 from '../assets/images/banner3.webp';
import banner4 from '../assets/images/banner4.webp';
import banner5 from '../assets/images/banner5.webp';
import banner6 from '../assets/images/banner6.webp';
import banner7 from '../assets/images/banner7.jpg';
import banner8 from '../assets/images/banner8.jpg';

import ProductSection from '../components/ProductSection';

const Home = () => {
  const [categories, setCategories] = useState([]);
  const [newestProducts, setNewestProducts] = useState([]);
  const [bestSellingProducts, setBestSellingProducts] = useState([]);
  const [saleProducts, setSaleProducts] = useState([]);
  const [posts, setPosts] = useState([]);
  const [banners, setBanners] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [newestPage, setNewestPage] = useState(1);
  const [salePage, setSalePage] = useState(1);
  const [blogPage, setBlogPage] = useState(1);
  const [bestSellingPage, setBestSellingPage] = useState(1);

  useEffect(() => {
    const fetchHomeData = async () => {
      try {
        setLoading(true);
        setLoadError('');
        const [catData, newestData, bestSellingData, saleData, postData, bannerData] = await Promise.all([
          categoryProductService.getAllCategoryProducts(),
          productService.getNewProducts(),
          productService.getBestSellingProducts(8),
          productService.getSaleProducts(),
          blogService.getFeaturedPosts(),
          bannerService.getBanners().catch(err => {
            console.error('Không tải được banner từ API, sử dụng fallback:', err);
            return [];
          })
        ]);

        // Lọc danh mục sản phẩm: chỉ lấy danh mục cha/gốc (tức là parentId/ParentId rỗng hoặc null)
        const parentCategories = (catData || []).filter(
          (cat) => (cat.parentId === null || cat.parentId === undefined) && (cat.ParentId === null || cat.ParentId === undefined)
        );
        // Lấy tất cả danh mục cha để hiển thị bằng Swiper
        setCategories(parentCategories);
        setNewestProducts(newestData || []);
        setBestSellingProducts(
          (bestSellingData || [])
            .filter((item) => item.isBestSelling || (item.soldQuantity || 0) > 0)
            .sort((a, b) => {
              if (a.isBestSelling && !b.isBestSelling) return -1;
              if (!a.isBestSelling && b.isBestSelling) return 1;
              return (b.soldQuantity || 0) - (a.soldQuantity || 0);
            })
            .slice(0, 8)
        );
        setSaleProducts(saleData || []);
        // Lấy tất cả bài viết nổi bật (IsFeatured = true) từ admin toggle
        setPosts(postData || []);
        setBanners(bannerData || []);
      } catch (error) {
        console.error('Lỗi khi tải dữ liệu trang chủ:', error);
        setLoadError('Không tải được dữ liệu trang chủ. Vui lòng kiểm tra backend API có đang chạy trên https://localhost:7238 hay không.');
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

  if (loadError) {
    return (
      <div className="container my-5">
        <div className="alert alert-warning shadow-sm" role="alert">
          <h5 className="font-weight-bold mb-2">Không kết nối được dữ liệu trang chủ</h5>
          <p className="mb-0">{loadError}</p>
        </div>
      </div>
    );
  }

  const displayBanners = banners && banners.length > 0
    ? banners.map(b => ({
        src: getMediaUrl(b.imageUrl),
        title: b.title || 'Banner quảng cáo',
        targetUrl: b.targetUrl
      }))
    : [
        { src: banner1, title: 'Độc quyền Website', targetUrl: '/products' },
        { src: banner2, title: 'Freeship đơn hàng', targetUrl: '/products' },
        { src: banner3, title: 'Lego Playground 2026', targetUrl: '/products' },
        { src: banner4, title: 'Children Day 2026', targetUrl: '/products' },
        { src: banner5, title: 'Xoay nắn sáng tạo Tangle', targetUrl: '/products' },
        { src: banner6, title: 'Art Puzzles', targetUrl: '/products' },
        { src: banner7, title: 'Lego Preschool', targetUrl: '/products' },
        { src: banner8, title: 'Clever Hippo Collection', targetUrl: '/products' }
      ];

  return (
    <div className="home-page-container">
      <div className="container mt-4">
        <div className="position-relative overflow-hidden rounded-lg shadow-sm animate--fade-in">
          <Swiper
            modules={[Autoplay, Navigation, Pagination]}
            speed={800}
            autoplay={{ delay: 5000, disableOnInteraction: false }}
            loop={true}
            navigation={{
              nextEl: '.swiper-button-next-custom',
              prevEl: '.swiper-button-prev-custom'
            }}
            pagination={{
              el: '.swiper-pagination-dots-custom',
              clickable: true
            }}
            className="mySwiper1"
          >
            {displayBanners.map((banner, idx) => {
              const hasLink = banner.targetUrl && banner.targetUrl.trim();
              const isExternal = hasLink && (banner.targetUrl.startsWith('http://') || banner.targetUrl.startsWith('https://'));
              const imageContent = (
                <div className="image-content">
                  <img
                    src={banner.src}
                    className="d-block w-100 img-fluid"
                    alt={banner.title}
                    style={{ objectFit: 'cover', width: '100%', height: 'auto' }}
                  />
                </div>
              );

              return (
                <SwiperSlide key={idx}>
                  {hasLink ? (
                    isExternal ? (
                      <a href={banner.targetUrl} target="_blank" rel="noopener noreferrer" className="d-block position-relative">
                        {imageContent}
                      </a>
                    ) : (
                      <Link to={banner.targetUrl} className="d-block position-relative">
                        {imageContent}
                      </Link>
                    )
                  ) : (
                    <div className="d-block position-relative">
                      {imageContent}
                    </div>
                  )}
                </SwiperSlide>
              );
            })}
          </Swiper>

          <div
            className="swiper-button-prev-custom"
            style={{
              position: 'absolute',
              top: '50%',
              left: '20px',
              transform: 'translateY(-50%)',
              zIndex: 10,
              cursor: 'pointer',
              transition: 'all 0.3s ease'
            }}
          >
            <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="1.5" y="1.5" width="45" height="45" rx="22.5" stroke="#CF102D" strokeWidth="3"></rect>
              <path d="M20.0607 25.0607L24.4393 29.4393C25.3843 30.3843 27 29.715 27 28.3787L27 19.6213C27 18.285 25.3843 17.6157 24.4393 18.5607L20.0607 22.9393C19.4749 23.5251 19.4749 24.4749 20.0607 25.0607Z" fill="#CF102D"></path>
            </svg>
          </div>

          <div
            className="swiper-button-next-custom"
            style={{
              position: 'absolute',
              top: '50%',
              right: '20px',
              transform: 'translateY(-50%) rotate(180deg)',
              zIndex: 10,
              cursor: 'pointer',
              transition: 'all 0.3s ease'
            }}
          >
            <svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="1.5" y="1.5" width="45" height="45" rx="22.5" stroke="#CF102D" strokeWidth="3"></rect>
              <path d="M20.0607 25.0607L24.4393 29.4393C25.3843 30.3843 27 29.715 27 28.3787L27 19.6213C27 18.285 25.3843 17.6157 24.4393 18.5607L20.0607 22.9393C19.4749 23.5251 19.4749 24.4749 20.0607 25.0607Z" fill="#CF102D"></path>
            </svg>
          </div>

          <div
            className="swiper-pagination-dots-custom"
            style={{
              position: 'absolute',
              bottom: '20px',
              left: '50%',
              transform: 'translateX(-50%)',
              zIndex: 10,
              display: 'flex',
              gap: '8px'
            }}
          ></div>
        </div>
      </div>

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

      <div className="container mt-5">
        <h4 className="text-uppercase font-weight-bold text-dark border-left border-danger pl-3 mb-4" style={{ borderWidth: '4px !important' }}>
          Danh mục nổi bật
        </h4>
        <div className="position-relative px-4 px-md-5">
          <Swiper
            modules={[Navigation]}
            spaceBetween={20}
            slidesPerView={5}
            navigation={{
              nextEl: '.cat-swiper-next',
              prevEl: '.cat-swiper-prev'
            }}
            breakpoints={{
              320: { slidesPerView: 2, spaceBetween: 12 },
              576: { slidesPerView: 3, spaceBetween: 15 },
              768: { slidesPerView: 4, spaceBetween: 20 },
              1024: { slidesPerView: 5, spaceBetween: 20 }
            }}
            className="category-swiper py-2"
          >
            {categories.map((item) => {
              const imgSrc = getMediaUrl(item.imageUrl);

              return (
                <SwiperSlide key={item.id} style={{ overflow: 'visible' }}>
                  <div className="text-center">
                    <Link to={`/products?category=${item.id}`} className="text-decoration-none d-block card-category-item">
                      <div className="category-circle-wrapper mx-auto mb-2 bg-light d-flex align-items-center justify-content-center shadow-sm rounded-circle border overflow-hidden" style={{ width: '100px', height: '100px', transition: 'all 0.3s' }}>
                        {imgSrc ? (
                          <img src={imgSrc} alt={item.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                        ) : (
                          <i className="fa-solid fa-puzzle-piece text-danger fs-3"></i>
                        )}
                      </div>
                      <span className="font-weight-bold text-dark small text-uppercase d-block text-truncate" style={{ fontSize: '0.82rem' }} title={item.name}>
                        {item.name}
                      </span>
                    </Link>
                  </div>
                </SwiperSlide>
              );
            })}
          </Swiper>

          {/* Navigation Arrows */}
          <button 
            className="cat-swiper-prev btn btn-light rounded-circle shadow-sm position-absolute d-flex align-items-center justify-content-center"
            style={{
              top: '50%',
              left: '0px',
              transform: 'translateY(-50%)',
              zIndex: 10,
              width: '38px',
              height: '38px',
              border: '1px solid #e9ecef',
              color: '#CF102D',
              padding: 0
            }}
            aria-label="Previous categories"
          >
            <i className="fa-solid fa-chevron-left" style={{ fontSize: '0.9rem' }}></i>
          </button>
          
          <button 
            className="cat-swiper-next btn btn-light rounded-circle shadow-sm position-absolute d-flex align-items-center justify-content-center"
            style={{
              top: '50%',
              right: '0px',
              transform: 'translateY(-50%)',
              zIndex: 10,
              width: '38px',
              height: '38px',
              border: '1px solid #e9ecef',
              color: '#CF102D',
              padding: 0
            }}
            aria-label="Next categories"
          >
            <i className="fa-solid fa-chevron-right" style={{ fontSize: '0.9rem' }}></i>
          </button>
        </div>
      </div>

      <ProductSection
        title="Sản phẩm bán chạy"
        iconClass="fa-solid fa-fire text-danger"
        outlineClass="btn-outline-danger"
        products={bestSellingProducts}
        emptyMessage="Chưa có sản phẩm bán chạy vì chưa có đơn hàng nào."
        currentPage={bestSellingPage}
        onPageChange={setBestSellingPage}
        pageSize={4}
        enablePagination={true}
      />

      <ProductSection
        title="Sản phẩm mới nhất"
        iconClass="fa-solid fa-clock text-primary"
        outlineClass="btn-outline-primary"
        products={newestProducts}
        currentPage={newestPage}
        onPageChange={setNewestPage}
        pageSize={4}
        enablePagination={true}
      />

      <ProductSection
        title="Khuyến mãi HOT"
        iconClass="fa-solid fa-tags text-warning"
        outlineClass="btn-outline-warning"
        products={saleProducts}
        currentPage={salePage}
        onPageChange={setSalePage}
        pageSize={4}
        enablePagination={true}
        emptyMessage="Hiện chưa có sản phẩm nào đang khuyến mãi."
      />

      <div className="container mt-5 mb-5 position-relative">
        <div className="d-flex justify-content-between align-items-end mb-4 border-bottom pb-2">
          <h4 className="text-uppercase font-weight-bold text-dark mb-0">
            <i className="fa-solid fa-newspaper text-info mr-2"></i> Blog sáng tạo & mẹo chơi đồ chơi
          </h4>
          <Link to="/blog" className="btn btn-sm btn-outline-info font-weight-bold rounded-pill px-3">Tất cả bài viết</Link>
        </div>

        {posts.length === 0 ? (
          <div className="col-12 text-center text-muted">Chưa có tin tức nào được bật hiển thị. Hãy bật toggle ở trang quản trị.</div>
        ) : (
          <div className="position-relative px-4 px-md-5">
            <Swiper
              modules={[Navigation, Scrollbar]}
              spaceBetween={24}
              slidesPerView={3}
              navigation={{
                nextEl: '.blog-swiper-next',
                prevEl: '.blog-swiper-prev'
              }}
              scrollbar={{
                el: '.swiper-scrollbar-custom .swiper-scrollbar',
                draggable: true,
                dragSize: 'auto'
              }}
              breakpoints={{
                320: { slidesPerView: 1, spaceBetween: 16 },
                768: { slidesPerView: 2, spaceBetween: 20 },
                1024: { slidesPerView: 3, spaceBetween: 24 }
              }}
              className="blog-swiper py-2"
            >
              {posts.map((post) => (
                <SwiperSlide key={post.id} style={{ overflow: 'visible' }}>
                  <div className="card h-100 border-0 rounded-lg overflow-hidden d-flex flex-column" style={{ backgroundColor: '#f8f9fa', transition: 'all 0.3s', borderRadius: '12px' }}>
                    <img
                      src={getMediaUrl(post.imageUrl, 'https://placehold.co/400x250/e9ecef/6c757d?text=No+Image')}
                      className="card-img-top"
                      alt={post.title}
                      style={{ height: '180px', objectFit: 'cover' }}
                    />
                    <div className="card-body p-3 d-flex flex-column justify-content-between flex-grow-1">
                      <div>
                        <h6 className="card-title font-weight-bold text-dark text-truncate-2" style={{ fontSize: '0.95rem', height: '40px', overflow: 'hidden', color: '#002664', lineHeight: '1.4' }}>
                          <Link to={`/blog/${post.id}`} className="text-dark text-decoration-none hover-danger">
                            {post.title}
                          </Link>
                        </h6>
                        <div className="d-flex text-secondary small mb-2" style={{ fontSize: '0.78rem' }}>
                          <span>{new Date(post.createdDate).toLocaleDateString('vi-VN')}</span>
                          <span className="mx-2">|</span>
                          <span>{post.categoryName || 'BTV Quách Phụng'}</span>
                        </div>
                        <p className="card-text text-muted small text-truncate-3" style={{ height: '60px', overflow: 'hidden', lineHeight: '1.4' }}>
                          {post.content ? post.content.replace(/<[^>]*>?/gm, '') : ''}
                        </p>
                      </div>
                      <div className="text-center mt-3 pt-2 border-top">
                        <Link to={`/blog/${post.id}`} className="font-weight-bold text-danger text-decoration-none" style={{ fontSize: '0.9rem' }}>
                          Xem Thêm
                        </Link>
                      </div>
                    </div>
                  </div>
                </SwiperSlide>
              ))}
            </Swiper>

            {/* Left navigation arrow */}
            <button
              className="blog-swiper-prev btn btn-light rounded-circle shadow-sm position-absolute d-flex align-items-center justify-content-center"
              style={{
                top: '40%',
                left: '0px',
                transform: 'translateY(-50%)',
                zIndex: 10,
                width: '40px',
                height: '40px',
                border: '2px solid #CF102D',
                color: '#CF102D',
                backgroundColor: '#fff',
                padding: 0
              }}
              aria-label="Previous posts"
            >
              <i className="fa-solid fa-chevron-left" style={{ fontSize: '0.9rem' }}></i>
            </button>

            {/* Right navigation arrow */}
            <button
              className="blog-swiper-next btn btn-light rounded-circle shadow-sm position-absolute d-flex align-items-center justify-content-center"
              style={{
                top: '40%',
                right: '0px',
                transform: 'translateY(-50%)',
                zIndex: 10,
                width: '40px',
                height: '40px',
                border: '2px solid #CF102D',
                color: '#CF102D',
                backgroundColor: '#fff',
                padding: 0
              }}
              aria-label="Next posts"
            >
              <i className="fa-solid fa-chevron-right" style={{ fontSize: '0.9rem' }}></i>
            </button>

            {/* Scrollbar with minion mascot */}
            <div className="swiper-scrollbar-custom">
              <div className="swiper-scrollbar"></div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};




export default Home;
