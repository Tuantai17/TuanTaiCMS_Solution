import React, { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import BlogCategoryList from '../components/BlogCategoryList';
import blogService from '../services/blogService';

const PostList = () => {
  const { categoryId } = useParams();
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const fetchPosts = async () => {
      try {
        setLoading(true);
        setErrorMessage('');
        const data = categoryId
          ? await blogService.getPostsByCategory(categoryId)
          : await blogService.getAllPosts();

        setPosts(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error('Lỗi khi tải danh sách bài viết:', error);
        setErrorMessage('Không thể tải danh sách bài viết. Vui lòng kiểm tra kết nối API Backend.');
      } finally {
        setLoading(false);
      }
    };

    fetchPosts();
  }, [categoryId]);

  const pageTitle = categoryId ? 'Bài viết theo chuyên mục' : 'Góc Sáng Tạo & Cẩm Nang Cho Bé';
  const pageDescription = categoryId
    ? 'Danh sách bài viết đã được lọc theo chủ đề bạn chọn từ hệ thống chuyên mục tin tức.'
    : 'Cập nhật những xu hướng đồ chơi mới nhất, mẹo giáo dục sáng tạo cùng cẩm nang lựa chọn đồ chơi thông minh chuẩn quốc tế giúp bé phát triển toàn diện.';

  const renderPostContent = () => {
    if (loading) {
      return (
        <div className="text-center my-5 py-5">
          <div className="spinner-border text-danger" role="status">
            <span className="sr-only">Đang tải...</span>
          </div>
          <p className="mt-3 text-secondary">Đang tải chuyên mục tin tức & cẩm nang...</p>
        </div>
      );
    }

    if (errorMessage) {
      return (
        <div className="alert alert-warning border-0 shadow-sm rounded-lg" role="alert">
          <strong>Không tải được dữ liệu.</strong>
          <div className="small mt-1">{errorMessage}</div>
        </div>
      );
    }

    if (posts.length === 0) {
      return (
        <div className="text-center py-5 border rounded-lg bg-light">
          <i className="fa-solid fa-folder-open text-muted-50 fs-1 mb-3"></i>
          <p className="text-secondary font-weight-medium mb-0">
            {categoryId
              ? 'Chuyên mục này chưa có bài viết nào trong hệ thống.'
              : 'Chưa có bài viết tin tức nào trong hệ thống.'}
          </p>
        </div>
      );
    }

    return (
      <div className="row">
        {posts.map((post) => (
          <div className="col-12 col-md-6 mb-4" key={post.id}>
            <div className="card h-100 shadow-sm border border-light rounded-lg overflow-hidden d-flex flex-column" style={{ transition: 'all 0.3s' }}>
              <div className="position-relative overflow-hidden" style={{ height: '200px' }}>
                <img
                  src={post.imageUrl || 'https://placehold.co/400x250/e9ecef/6c757d?text=No+Image'}
                  className="card-img-top w-100 h-100 object-fit-cover transition-all"
                  alt={post.title}
                  style={{ objectFit: 'cover' }}
                />
                <span className="position-absolute badge badge-info px-2 py-1" style={{ top: '10px', left: '10px', fontSize: '0.7rem' }}>
                  {post.categoryName || 'Tin tức'}
                </span>
              </div>

              <div className="card-body p-4 d-flex flex-column justify-content-between flex-grow-1">
                <div>
                  <h5 className="card-title font-weight-bold text-dark lh-sm" style={{ fontSize: '0.98rem', minHeight: '44px', overflow: 'hidden' }}>
                    <Link to={`/blog/${post.id}`} className="text-dark text-decoration-none hover-danger">
                      {post.title}
                    </Link>
                  </h5>
                  <p className="card-text text-secondary small text-truncate-3 mt-2 mb-0" style={{ fontSize: '0.85rem', minHeight: '54px', overflow: 'hidden' }}>
                    {post.shortDescription || 'Đang cập nhật nội dung tóm tắt cho bài viết...'}
                  </p>
                </div>

                <div className="d-flex justify-content-between align-items-center text-muted small mt-4 border-top pt-3">
                  <span>
                    <i className="fa-regular fa-calendar-days mr-1"></i>
                    {post.createdDate ? new Date(post.createdDate).toLocaleDateString('vi-VN') : 'Đang cập nhật'}
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
    );
  };

  return (
    <div className="container mt-4">
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item">
            <Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link>
          </li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Blog & Tin tức</li>
        </ol>
      </nav>

      <div className="text-center mb-5">
        <h2 className="text-uppercase font-weight-bold text-dark mb-2">{pageTitle}</h2>
        <p className="text-secondary small max-width-600 mx-auto">{pageDescription}</p>
        <div className="mx-auto bg-danger mt-3" style={{ width: '60px', height: '3px', borderRadius: '2px' }}></div>
      </div>

      <div className="row align-items-start">
        <div className="col-12 col-lg-4 mb-4 mb-lg-0">
          <BlogCategoryList />
        </div>
        <div className="col-12 col-lg-8">
          {renderPostContent()}
        </div>
      </div>
    </div>
  );
};

export default PostList;
