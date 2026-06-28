import React from 'react';
import { Link } from 'react-router-dom';
import ProductCard from './ProductCard';

const DEFAULT_HOME_PRODUCTS_PER_PAGE = 8;

const ProductSection = ({
  title,
  iconClass,
  outlineClass,
  products,
  emptyMessage = 'Chưa có sản phẩm nào.',
  currentPage = 1,
  onPageChange,
  pageSize = DEFAULT_HOME_PRODUCTS_PER_PAGE,
  enablePagination = false
}) => {
  const totalPages = Math.ceil(products.length / pageSize);
  const startIndex = enablePagination ? (currentPage - 1) * pageSize : 0;
  const visibleProducts = products.slice(startIndex, startIndex + pageSize);

  return (
    <div className="container mt-5">
      <div className="d-flex justify-content-between align-items-end mb-4 border-bottom pb-2">
        <h4 className="text-uppercase font-weight-bold text-dark mb-0">
          <i className={`${iconClass} mr-2`}></i> {title}
        </h4>
        <Link to="/products" className={`btn btn-sm ${outlineClass} font-weight-bold rounded-pill px-3`}>
          Xem tất cả
        </Link>
      </div>

      <div className="row">
        {visibleProducts.length === 0 ? (
          <div className="col-12 text-center py-4 text-muted">{emptyMessage}</div>
        ) : (
          visibleProducts.map((item) => (
            <div className="col-6 col-md-3 mb-4" key={item.id}>
              <ProductCard item={item} />
            </div>
          ))
        )}
      </div>

      {enablePagination && totalPages > 1 && (
        <nav aria-label={`${title} pagination`} className="mt-2">
          <ul className="pagination justify-content-center align-items-center mb-0">
            <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}>
              <button
                type="button"
                className="page-link shadow-none border-0 font-weight-bold"
                onClick={() => onPageChange(Math.max(currentPage - 1, 1))}
                style={{ color: '#0d6efd', backgroundColor: 'transparent' }}
              >
                <i className="fa-solid fa-angles-left mr-1"></i> Trước
              </button>
            </li>

            {[...Array(totalPages).keys()].map((page) => {
              const pageNumber = page + 1;
              const isActive = currentPage === pageNumber;

              return (
                <li key={pageNumber} className={`page-item ${isActive ? 'active' : ''}`}>
                  <button
                    type="button"
                    className="page-link shadow-none border-0 mx-1 rounded-circle font-weight-bold"
                    onClick={() => onPageChange(pageNumber)}
                    style={{
                      width: '38px',
                      height: '38px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      backgroundColor: isActive ? '#0d6efd' : '#f8f9fa',
                      color: isActive ? '#ffffff' : '#555555'
                    }}
                  >
                    {pageNumber}
                  </button>
                </li>
              );
            })}

            <li className={`page-item ${currentPage === totalPages ? 'disabled' : ''}`}>
              <button
                type="button"
                className="page-link shadow-none border-0 font-weight-bold"
                onClick={() => onPageChange(Math.min(currentPage + 1, totalPages))}
                style={{ color: '#0d6efd', backgroundColor: 'transparent' }}
              >
                Sau <i className="fa-solid fa-angles-right ml-1"></i>
              </button>
            </li>
          </ul>
        </nav>
      )}
    </div>
  );
};

export default ProductSection;
