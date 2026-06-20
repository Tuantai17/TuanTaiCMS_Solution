import React, { useEffect, useState, useMemo } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import blogService from '../services/blogService';
import { getMediaUrl } from '../utils/mediaUrl';

const POSTS_PER_PAGE = 6;

const PostList = () => {
  const { categoryId } = useParams();
  const navigate = useNavigate();
  const [posts, setPosts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [viewMode, setViewMode] = useState('list'); // 'list' hoặc 'grid'
  const [expandedCategories, setExpandedCategories] = useState({});

  // Fetch danh mục bài viết
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const data = await blogService.getBlogCategories();
        setCategories(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error('Lỗi khi tải danh mục:', error);
      }
    };
    fetchCategories();
  }, []);

  // Fetch bài viết theo danh mục hoặc tất cả
  useEffect(() => {
    const fetchPosts = async () => {
      try {
        setLoading(true);
        setErrorMessage('');
        setCurrentPage(1);
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

  // Lọc bài viết theo tìm kiếm
  const filteredPosts = useMemo(() => {
    if (!searchQuery.trim()) return posts;
    const query = searchQuery.toLowerCase().trim();
    return posts.filter(
      (post) =>
        post.title?.toLowerCase().includes(query) ||
        post.shortDescription?.toLowerCase().includes(query) ||
        post.categoryName?.toLowerCase().includes(query)
    );
  }, [posts, searchQuery]);

  // Phân trang
  const totalPages = Math.ceil(filteredPosts.length / POSTS_PER_PAGE);
  const startIndex = (currentPage - 1) * POSTS_PER_PAGE;
  const paginatedPosts = filteredPosts.slice(startIndex, startIndex + POSTS_PER_PAGE);

  // Toggle mở rộng danh mục
  const toggleCategory = (catId) => {
    setExpandedCategories((prev) => ({ ...prev, [catId]: !prev[catId] }));
  };

  // Tên danh mục hiện tại
  const currentCategoryName = categoryId
    ? categories.find((c) => String(c.id) === String(categoryId))?.name || 'Chuyên mục'
    : null;

  // Tổng số bài viết
  const totalAllPosts = posts.length;

  return (
    <div className="blog-page-container">
      {/* Breadcrumb */}
      <div className="container mt-3">
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb bg-transparent p-0 mb-3" style={{ fontSize: '0.85rem' }}>
            <li className="breadcrumb-item">
              <Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link>
            </li>
            <li className="breadcrumb-item">
              <Link to="/blog" className="text-secondary text-decoration-none">Blog & Tin tức</Link>
            </li>
            {currentCategoryName && (
              <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">
                {currentCategoryName}
              </li>
            )}
          </ol>
        </nav>
      </div>

      <div className="container mb-5">
        <div className="row">
          {/* ============ SIDEBAR ============ */}
          <div className="col-12 col-lg-3 mb-4 mb-lg-0">
            {/* Search Box */}
            <div className="blog-search-box mb-4">
              <div className="position-relative">
                <i className="fa-solid fa-magnifying-glass blog-search-icon"></i>
                <input
                  type="text"
                  className="form-control blog-search-input"
                  placeholder="Nhập từ khóa để tìm kiếm (ví dụ...)"
                  value={searchQuery}
                  onChange={(e) => {
                    setSearchQuery(e.target.value);
                    setCurrentPage(1);
                  }}
                />
              </div>
            </div>

            {/* Danh mục bài viết */}
            <div className="blog-sidebar-categories">
              <h6 className="blog-sidebar-title">
                <i className="fa-solid fa-bars-staggered me-2"></i>
                DANH MỤC BÀI VIẾT
              </h6>

              <div className="blog-category-list">
                {/* Tất cả */}
                <div className="blog-category-item">
                  <div
                    className={`blog-category-link ${!categoryId ? 'active' : ''}`}
                    onClick={() => {
                      navigate('/blog');
                    }}
                  >
                    <span className="blog-category-name">Tất cả</span>
                  </div>
                </div>

                {categories.map((cate) => {
                  const isActive = String(categoryId) === String(cate.id);
                  return (
                    <div key={cate.id} className="blog-category-item">
                      <div
                        className={`blog-category-link ${isActive ? 'active' : ''}`}
                        onClick={() => {
                          navigate(`/blog/category/${cate.id}`);
                        }}
                      >
                        <span className="blog-category-name">{cate.name}</span>
                        <button
                          className="blog-category-toggle"
                          onClick={(e) => {
                            e.stopPropagation();
                            toggleCategory(cate.id);
                          }}
                        >
                          <i className={`fa-solid fa-chevron-${expandedCategories[cate.id] ? 'up' : 'down'}`}></i>
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          {/* ============ MAIN CONTENT ============ */}
          <div className="col-12 col-lg-9">
            {/* Header: Title + View Toggle */}
            <div className="blog-content-header">
              <h4 className="blog-content-title">
                {currentCategoryName || 'Tất Cả Bài Viết'}
              </h4>
              <div className="blog-view-toggle">
                <button
                  className={`blog-view-btn ${viewMode === 'list' ? 'active' : ''}`}
                  onClick={() => setViewMode('list')}
                  title="Hiển thị dạng danh sách"
                >
                  <i className="fa-solid fa-list"></i>
                </button>
                <button
                  className={`blog-view-btn ${viewMode === 'grid' ? 'active' : ''}`}
                  onClick={() => setViewMode('grid')}
                  title="Hiển thị dạng lưới"
                >
                  <i className="fa-solid fa-grid-2"></i>
                </button>
              </div>
            </div>

            {/* Loading / Error / Empty States */}
            {loading && (
              <div className="text-center my-5 py-5">
                <div className="spinner-border text-danger" role="status">
                  <span className="sr-only">Đang tải...</span>
                </div>
                <p className="mt-3 text-secondary">Đang tải chuyên mục tin tức & cẩm nang...</p>
              </div>
            )}

            {errorMessage && !loading && (
              <div className="alert alert-warning border-0 shadow-sm rounded-lg" role="alert">
                <strong>Không tải được dữ liệu.</strong>
                <div className="small mt-1">{errorMessage}</div>
              </div>
            )}

            {!loading && !errorMessage && filteredPosts.length === 0 && (
              <div className="text-center py-5 border rounded-lg bg-light">
                <i className="fa-solid fa-folder-open fs-1 text-muted mb-3 d-block" style={{ opacity: 0.4 }}></i>
                <p className="text-secondary font-weight-medium mb-0">
                  {searchQuery
                    ? `Không tìm thấy bài viết nào phù hợp với "${searchQuery}"`
                    : categoryId
                      ? 'Chuyên mục này chưa có bài viết nào.'
                      : 'Chưa có bài viết tin tức nào trong hệ thống.'}
                </p>
              </div>
            )}

            {/* ===== LIST VIEW ===== */}
            {!loading && !errorMessage && filteredPosts.length > 0 && viewMode === 'list' && (
              <div className="blog-list-view">
                {paginatedPosts.map((post) => (
                  <div className="blog-list-card" key={post.id}>
                    <Link to={`/blog/${post.id}`} className="blog-list-card-image-link">
                      <img
                        src={getMediaUrl(post.imageUrl, 'https://placehold.co/500x300/e9ecef/6c757d?text=No+Image')}
                        className="blog-list-card-image"
                        alt={post.title}
                      />
                    </Link>
                    <div className="blog-list-card-body">
                      <Link to={`/blog/${post.id}`} className="blog-list-card-title">
                        {post.title}
                      </Link>
                      <p className="blog-list-card-desc">
                        {post.shortDescription || 'Đang cập nhật nội dung tóm tắt cho bài viết...'}
                      </p>
                      <div className="blog-list-card-meta">
                        <span className="blog-list-card-date">
                          <i className="fa-regular fa-calendar-days me-1"></i>
                          {post.createdDate
                            ? new Date(post.createdDate).toLocaleDateString('vi-VN', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric'
                              })
                            : 'Đang cập nhật'}
                        </span>
                        <span className="blog-list-card-author">
                          <i className="fa-regular fa-user me-1"></i>
                          BTV Quách Phụng
                        </span>
                      </div>
                      <Link to={`/blog/${post.id}`} className="blog-list-card-readmore">
                        Xem Thêm
                      </Link>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* ===== GRID VIEW ===== */}
            {!loading && !errorMessage && filteredPosts.length > 0 && viewMode === 'grid' && (
              <div className="blog-grid-view">
                {paginatedPosts.map((post) => (
                  <div className="blog-grid-card" key={post.id}>
                    <Link to={`/blog/${post.id}`} className="blog-grid-card-image-link">
                      <img
                        src={getMediaUrl(post.imageUrl, 'https://placehold.co/500x300/e9ecef/6c757d?text=No+Image')}
                        className="blog-grid-card-image"
                        alt={post.title}
                      />
                    </Link>
                    <div className="blog-grid-card-body">
                      <Link to={`/blog/${post.id}`} className="blog-grid-card-title">
                        {post.title}
                      </Link>
                      <p className="blog-grid-card-desc">
                        {post.shortDescription || 'Đang cập nhật nội dung tóm tắt cho bài viết...'}
                      </p>
                      <div className="blog-grid-card-meta">
                        <span>
                          <i className="fa-regular fa-calendar-days me-1"></i>
                          {post.createdDate
                            ? new Date(post.createdDate).toLocaleDateString('vi-VN', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric'
                              })
                            : 'Đang cập nhật'}
                        </span>
                        <span>
                          <i className="fa-regular fa-user me-1"></i>
                          BTV Quách Phụng
                        </span>
                      </div>
                      <Link to={`/blog/${post.id}`} className="blog-grid-card-readmore">
                        Xem Thêm
                      </Link>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Phân trang */}
            {!loading && totalPages > 1 && (
              <nav aria-label="Blog pagination" className="mt-4">
                <ul className="pagination justify-content-center gap-1">
                  <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}>
                    <button
                      className="page-link shadow-none border-0 font-weight-bold"
                      onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
                      style={{ color: '#4a4a8a', backgroundColor: 'transparent' }}
                    >
                      <i className="fa-solid fa-angles-left"></i> Trước
                    </button>
                  </li>
                  {[...Array(totalPages).keys()].map((page) => (
                    <li key={page + 1} className={`page-item ${currentPage === page + 1 ? 'active' : ''}`}>
                      <button
                        className="page-link shadow-none border-0 mx-1 rounded-circle font-weight-bold"
                        onClick={() => setCurrentPage(page + 1)}
                        style={{
                          width: '40px',
                          height: '40px',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          backgroundColor: currentPage === page + 1 ? '#4a4a8a' : '#f0f0f5',
                          color: currentPage === page + 1 ? '#ffffff' : '#555555'
                        }}
                      >
                        {page + 1}
                      </button>
                    </li>
                  ))}
                  <li className={`page-item ${currentPage === totalPages ? 'disabled' : ''}`}>
                    <button
                      className="page-link shadow-none border-0 font-weight-bold"
                      onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))}
                      style={{ color: '#4a4a8a', backgroundColor: 'transparent' }}
                    >
                      Sau <i className="fa-solid fa-angles-right"></i>
                    </button>
                  </li>
                </ul>
              </nav>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PostList;
