import React, { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import blogService from '../services/blogService';

const BlogCategoryList = () => {
  const { categoryId } = useParams();
  const [blogCategories, setBlogCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const fetchBlogCategories = async () => {
      try {
        setLoading(true);
        setErrorMessage('');
        const data = await blogService.getBlogCategories();
        setBlogCategories(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error('Lỗi hệ thống khi gọi API chuyên mục tin tức:', error);
        setErrorMessage('Không thể tải chuyên mục bài viết. Vui lòng kiểm tra API Backend.');
      } finally {
        setLoading(false);
      }
    };

    fetchBlogCategories();
  }, []);

  if (loading) {
    return (
      <aside className="card border-0 shadow-sm p-4 rounded-lg bg-white mb-4">
        <div className="d-flex align-items-center text-secondary small">
          <span className="spinner-border spinner-border-sm text-danger mr-2" role="status" aria-hidden="true"></span>
          Đang nạp các chuyên mục bài viết...
        </div>
      </aside>
    );
  }

  return (
    <aside className="card border-0 shadow-sm p-4 rounded-lg bg-white mb-4">
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h5 className="card-title text-uppercase font-weight-bold text-dark mb-0" style={{ fontSize: '0.95rem' }}>
          <i className="fa-solid fa-tags mr-2 text-danger"></i>
          Chủ đề bài viết
        </h5>
        <span className="badge badge-light border text-muted">Blog</span>
      </div>

      {errorMessage ? (
        <div className="alert alert-warning small mb-0" role="alert">
          {errorMessage}
        </div>
      ) : (
        <div className="list-group list-group-flush">
          <Link
            to="/blog"
            className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center px-0 border-0 text-decoration-none small ${
              !categoryId ? 'text-danger font-weight-bold' : 'text-dark'
            }`}
          >
            <span>
              <i className="fa-solid fa-layer-group mr-2 text-muted"></i>
              Tất cả bài viết
            </span>
            <span className="badge badge-light border text-muted">All</span>
          </Link>

          {blogCategories.length === 0 ? (
            <p className="text-muted small mb-0 pt-2">Chưa có chủ đề tin tức nào.</p>
          ) : (
            blogCategories.map((cate) => {
              const isActive = String(categoryId) === String(cate.id);

              return (
                <Link
                  key={cate.id}
                  to={`/blog/category/${cate.id}`}
                  className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center px-0 border-0 text-decoration-none small ${
                    isActive ? 'text-danger font-weight-bold' : 'text-dark'
                  }`}
                >
                  <span>
                    <i className="fa-regular fa-hashtag mr-2 text-muted"></i>
                    {cate.name}
                  </span>
                  <span className="badge badge-light border text-muted">{cate.postCount ?? 'Read'}</span>
                </Link>
              );
            })
          )}
        </div>
      )}
    </aside>
  );
};

export default BlogCategoryList;
