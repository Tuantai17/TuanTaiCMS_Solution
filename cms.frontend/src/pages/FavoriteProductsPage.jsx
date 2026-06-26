import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import ProductCard from '../components/ProductCard';
import favoriteService from '../services/favoriteService';
import { useFavorite } from '../contexts/FavoriteContext';

const FavoriteProductsPage = () => {
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const { favoriteCount, fetchFavoriteCount } = useFavorite();

  useEffect(() => {
    fetchFavorites(page);
  }, [page]);

  const fetchFavorites = async (pageNum) => {
    try {
      setLoading(true);
      const data = await favoriteService.getFavorites(pageNum, 12, '');
      if (data && Array.isArray(data.items)) {
        setFavorites(data.items);
        setTotalPages(data.totalPages || 1);
        fetchFavoriteCount();
      } else {
        setFavorites([]);
        setTotalPages(1);
      }
    } catch (error) {
      console.error('Lỗi lấy danh sách yêu thích:', error);
      setFavorites([]);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  };

  const handlePageChange = (newPage) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setPage(newPage);
    }
  };

  return (
    <div className="container mt-4 mb-5">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
          <li className="breadcrumb-item"><Link to="/profile" className="text-secondary text-decoration-none">Tài khoản</Link></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Sản phẩm yêu thích</li>
        </ol>
      </nav>

      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="h3 font-weight-bold" style={{ color: '#002664' }}>
          Sản phẩm yêu thích <span className="badge badge-danger ml-2" style={{ backgroundColor: '#CF102D', fontSize: '1rem', verticalAlign: 'middle' }}>{favoriteCount}</span>
        </h2>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-danger" role="status"></div>
          <p className="mt-3 text-muted">Đang tải danh sách yêu thích...</p>
        </div>
      ) : favorites.length === 0 ? (
        <div className="text-center py-5 bg-white rounded shadow-sm border border-light">
          <i className="fa-solid fa-heart-crack text-muted mb-3" style={{ fontSize: '4rem', opacity: 0.5 }}></i>
          <h4 className="font-weight-bold text-dark">Chưa có sản phẩm yêu thích</h4>
          <p className="text-muted">Bạn chưa lưu sản phẩm nào vào danh sách yêu thích.</p>
          <Link to="/products" className="btn btn-danger mt-3 px-4 rounded-pill">
            Tiếp tục mua sắm
          </Link>
        </div>
      ) : (
        <>
          <div className="row">
            {favorites.map((product) => (
              <div key={product.productId} className="col-6 col-md-4 col-lg-3 mb-4">
                <ProductCard 
                  item={{ 
                    ...product, 
                    id: product.productId // ProductCard dùng id
                  }} 
                />
              </div>
            ))}
          </div>

          {/* Phân trang */}
          {totalPages > 1 && (
            <div className="d-flex justify-content-center mt-4">
              <nav aria-label="Page navigation">
                <ul className="pagination mb-0">
                  <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                    <button className="page-link" onClick={() => handlePageChange(page - 1)}>
                      <i className="fa-solid fa-chevron-left"></i>
                    </button>
                  </li>
                  {[...Array(totalPages)].map((_, idx) => {
                    const p = idx + 1;
                    return (
                      <li key={p} className={`page-item ${page === p ? 'active' : ''}`}>
                        <button className="page-link" onClick={() => handlePageChange(p)}>
                          {p}
                        </button>
                      </li>
                    );
                  })}
                  <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
                    <button className="page-link" onClick={() => handlePageChange(page + 1)}>
                      <i className="fa-solid fa-chevron-right"></i>
                    </button>
                  </li>
                </ul>
              </nav>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default FavoriteProductsPage;
