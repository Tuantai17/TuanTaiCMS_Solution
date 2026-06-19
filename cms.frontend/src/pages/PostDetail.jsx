import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import blogService from '../services/blogService';
import { getMediaUrl } from '../utils/mediaUrl';

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
          <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
          <li className="breadcrumb-item"><Link to="/blog" className="text-secondary text-decoration-none">Blog & Tin tức</Link></li>
          <li className="breadcrumb-item active text-danger font-weight-bold text-truncate" aria-current="page" style={{ maxWidth: '300px' }}>{post.title}</li>
        </ol>
      </nav>

      <div className="row">
        {/* CHI TIẾT BÀI VIẾT KHÔNG CÓ SIDEBAR */}
        <div className="col-12 mb-4">
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
                src={getMediaUrl(post.imageUrl, "https://placehold.co/800x450/e9ecef/6c757d?text=No+Image")} 
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
      </div>
    </div>
  );
};

export default PostDetail;
