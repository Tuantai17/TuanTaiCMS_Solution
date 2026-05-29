import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import blogService from '../services/blogService';

const PostList = () => {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPosts = async () => {
      try {
        setLoading(true);
        const data = await blogService.getAllPosts();
        setPosts(data);
      } catch (error) {
        console.error("Lỗi khi tải danh sách bài viết:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchPosts();
  }, []);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang tải chuyên mục tin tức & cẩm nang...</p>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Blog & Tin tức</li>
        </ol>
      </nav>

      {/* Tiêu đề trang tin tức */}
      <div className="text-center mb-5">
        <h2 className="text-uppercase font-weight-bold text-dark mb-2">Góc Sáng Tạo & Cẩm Nang Cho Bé</h2>
        <p className="text-secondary small max-width-600 mx-auto">
          Cập nhật những xu hướng đồ chơi mới nhất, mẹo giáo dục sáng tạo cùng cẩm nang lựa chọn đồ chơi thông minh chuẩn quốc tế giúp bé phát triển toàn diện.
        </p>
        <div className="mx-auto bg-danger mt-3" style={{ width: '60px', height: '3px', borderRadius: '2px' }}></div>
      </div>

      {/* Lưới bài viết */}
      {posts.length === 0 ? (
        <div className="text-center py-5 border rounded-lg bg-light">
          <i className="fa-solid fa-folder-open text-muted-50 fs-1 mb-3"></i>
          <p className="text-secondary font-weight-medium mb-0">Chưa có bài viết tin tức nào trong hệ thống.</p>
        </div>
      ) : (
        <div className="row">
          {posts.map((post) => (
            <div className="col-12 col-md-6 col-lg-4 mb-4" key={post.id}>
              <div className="card h-100 shadow-sm border border-light rounded-lg overflow-hidden d-flex flex-column" style={{ transition: 'all 0.3s' }}>
                {/* Ảnh bài viết */}
                <div className="position-relative overflow-hidden" style={{ height: '200px' }}>
                  <img 
                    src={post.imageUrl || "https://placehold.co/400x250/e9ecef/6c757d?text=No+Image"} 
                    className="card-img-top w-100 h-100 object-fit-cover transition-all" 
                    alt={post.title} 
                    style={{ objectFit: 'cover' }} 
                  />
                  <span className="position-absolute badge badge-info px-2 py-1" style={{ top: '10px', left: '10px', fontSize: '0.7rem' }}>
                    {post.categoryName || 'Tin tức'}
                  </span>
                </div>
                
                {/* Thân bài viết */}
                <div className="card-body p-4 d-flex flex-column justify-content-between flex-grow-1">
                  <div>
                    <h5 className="card-title font-weight-bold text-dark lh-sm" style={{ fontSize: '0.98rem', height: '44px', overflow: 'hidden' }}>
                      <Link to={`/blog/${post.id}`} className="text-dark text-decoration-none hover-danger">
                        {post.title}
                      </Link>
                    </h5>
                    <p className="card-text text-secondary small text-truncate-3 mt-2 mb-0" style={{ fontSize: '0.85rem', height: '54px', overflow: 'hidden' }}>
                      {post.shortDescription || 'Đang cập nhật nội dung tóm tắt cho bài viết...'}
                    </p>
                  </div>
                  
                  {/* Chân bài viết */}
                  <div className="d-flex justify-content-between align-items-center text-muted small mt-4 border-top pt-3">
                    <span>
                      <i className="fa-regular fa-calendar-days mr-1"></i> 
                      {new Date(post.createdDate).toLocaleDateString('vi-VN')}
                    </span>
                    <Link to={`/blog/${post.id}`} className="btn btn-sm btn-danger rounded-pill px-3 font-weight-bold text-uppercase" style={{ fontSize: '0.75rem' }}>
                      Đọc thêm
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default PostList;
