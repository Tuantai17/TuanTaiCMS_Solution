import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import blogService from '../services/blogService';

const PostDetail = () => {
  const { id } = useParams();
  const [post, setPost] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPostDetail = async () => {
      try {
        setLoading(true);
        const data = await blogService.getPostDetail(id);
        setPost(data);
      } catch (error) {
        console.error("Lỗi khi tải chi tiết bài viết:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchPostDetail();
  }, [id]);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang mở nội dung cẩm nang...</p>
      </div>
    );
  }

  if (!post) {
    return (
      <div className="container text-center my-5 py-5">
        <i className="fa-solid fa-triangle-exclamation text-warning fs-1 mb-3"></i>
        <h4 className="font-weight-bold">Không tìm thấy bài viết này!</h4>
        <p className="text-secondary">Bài viết có thể đã bị xóa hoặc đường dẫn không đúng.</p>
        <Link to="/blog" className="btn btn-danger rounded-pill px-4 mt-3">Quay lại Blog</Link>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item"><a href="/blog" className="text-secondary text-decoration-none">Blog & Tin tức</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold text-truncate" aria-current="page" style={{ maxWidth: '300px' }}>{post.title}</li>
        </ol>
      </nav>

      <div className="row">
        {/* KHU VỰC BÊN TRÁI: CHI TIẾT BÀI VIẾT */}
        <div className="col-12 col-lg-8 mb-4">
          <article className="blog-post-wrapper card border-0 shadow-sm p-4 p-md-5 rounded-lg">
            <span className="badge badge-info px-3 py-2 rounded-pill font-weight-bold mb-3 text-uppercase align-self-start" style={{ fontSize: '0.75rem', letterSpacing: '0.5px' }}>
              {post.category?.name || 'Cẩm nang MyKingdom'}
            </span>

            <h1 className="h2 font-weight-bold text-dark mb-3" style={{ fontSize: '1.8rem', lineHeight: '1.35' }}>
              {post.title}
            </h1>

            <div className="d-flex align-items-center text-secondary small mb-4 pb-3 border-bottom">
              <span className="mr-3"><i className="fa-regular fa-calendar-days mr-1"></i> Ngày đăng: {new Date(post.createdDate).toLocaleDateString('vi-VN')}</span>
              <span><i className="fa-regular fa-user mr-1"></i> Tác giả: Ban Biên Tập MyKingdom</span>
            </div>

            {post.shortDescription && (
              <div className="p-3 bg-light rounded-lg border-left border-info font-italic text-secondary mb-4" style={{ borderWidth: '4px !important', fontSize: '0.95rem' }}>
                {post.shortDescription}
              </div>
            )}

            <div className="post-banner-wrapper rounded-lg overflow-hidden mb-4 shadow-sm" style={{ maxHeight: '400px' }}>
              <img 
                src={post.imageUrl || "https://placehold.co/800x450/e9ecef/6c757d?text=No+Image"} 
                className="img-fluid w-100 object-fit-cover" 
                alt={post.title} 
                style={{ objectFit: 'cover', maxHeight: '400px' }}
              />
            </div>

            <div className="post-html-content text-dark lh-lg" style={{ fontSize: '1.05rem', wordBreak: 'break-word' }}>
              {post.content ? (
                <div dangerouslySetInnerHTML={{ __html: post.content }} />
              ) : (
                <div>
                  <p>Hệ thống đang tiến hành biên soạn nội dung chi tiết cho bài viết này để mang lại cẩm nang bổ ích nhất dành cho quý phụ huynh và các em nhỏ.</p>
                  <p>Quý khách vui lòng quay trở lại sau hoặc khám phá thêm các bộ đồ chơi xếp hình thông minh và khuyến mãi hot tại thanh công cụ của chúng tôi.</p>
                  <p className="font-weight-bold mt-4">Trân trọng cảm ơn!</p>
                </div>
              )}
            </div>

            <div className="mt-5 border-top pt-4">
              <Link to="/blog" className="btn btn-outline-danger rounded-pill px-4 font-weight-bold text-uppercase" style={{ fontSize: '0.8rem' }}>
                <i className="fa-solid fa-chevron-left mr-2"></i> Quay lại tin tức
              </Link>
            </div>
          </article>
        </div>

        {/* SIDEBAR BÊN PHẢI: BÀI VIẾT KHÁC & ĐỒ CHƠI LIÊN QUAN */}
        <div className="col-12 col-lg-4">
          <div className="card shadow-sm border border-light rounded-lg overflow-hidden mb-4">
            <div className="card-header bg-danger text-white py-3 px-4">
              <h6 className="card-title font-weight-bold text-uppercase mb-0" style={{ fontSize: '0.9rem' }}>
                <i className="fa-solid fa-puzzle-piece mr-2"></i> Đồ chơi nổi bật
              </h6>
            </div>
            <div className="card-body p-3">
              <div className="d-flex align-items-center mb-3 pb-3 border-bottom">
                <img src="https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=80&q=80" alt="Keychron" className="rounded border mr-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
                <div>
                  <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2">Bàn phím cơ Keychron K2 Pro thông minh</h6>
                  <span className="text-danger font-weight-bold small">2.290.000 ₫</span>
                </div>
              </div>
              <div className="d-flex align-items-center mb-0">
                <img src="https://images.unsplash.com/photo-1603351154351-5e2d0600bb77?w=80&q=80" alt="AirPods" className="rounded border mr-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
                <div>
                  <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2">Tai nghe thông minh AirPods Pro 2 cực đỉnh</h6>
                  <span className="text-danger font-weight-bold small">5.990.000 ₫</span>
                </div>
              </div>
            </div>
          </div>

          <div className="card shadow-sm border-0 rounded-lg overflow-hidden text-center text-white py-5 px-4" style={{ background: 'linear-gradient(135deg, #c80f1e, #ff9500)', height: '220px' }}>
            <h4 className="font-weight-black text-uppercase tracking-wider">SIÊU SALE OUTLET</h4>
            <h2 className="display-4 font-weight-bold my-2" style={{ fontSize: '2.5rem' }}>-50%</h2>
            <p className="small opacity-75">Áp dụng cho mọi đơn hàng phụ kiện đồ chơi</p>
            <Link to="/products" className="btn btn-light btn-sm rounded-pill px-4 text-danger font-weight-bold text-uppercase mt-2 shadow-sm">Mua ngay</Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PostDetail;
