import React from 'react';
import { Link } from 'react-router-dom';

const ProductCard = ({ item }) => {
  const brands = ["LEGO", "SCOOTER", "TANGLE", "FISHER PRICE", "HOT WHEELS", "BARBIE"];
  
  return (
    <div className="card h-100 shadow-sm border border-light product-card-hover rounded-lg overflow-hidden d-flex flex-column" style={{ transition: 'all 0.3s' }}>
      {/* Thẻ chứa ảnh lớn ở giữa */}
      <div className="position-relative p-3 bg-light text-center d-flex align-items-center justify-content-center" style={{ height: '180px' }}>
        <img 
          src={item.imageUrl || "https://placehold.co/200x150/e9ecef/6c757d?text=No+Image"} 
          className="img-fluid object-fit-contain" 
          alt={item.name} 
          style={{ maxHeight: '150px', maxWidth: '100%', objectFit: 'contain' }} 
        />
        {item.stockQuantity <= 5 && item.stockQuantity > 0 && (
          <span className="position-absolute badge bg-warning text-dark rounded-pill px-2 py-1" style={{ top: '10px', left: '10px', fontSize: '0.7rem' }}>SẮP HẾT</span>
        )}
        {item.stockQuantity === 0 && (
          <span className="position-absolute badge bg-danger text-white rounded-pill px-2 py-1" style={{ top: '10px', left: '10px', fontSize: '0.7rem' }}>HẾT HÀNG</span>
        )}
      </div>
      
      {/* Phần thân thông tin sản phẩm */}
      <div className="card-body p-3 d-flex flex-column justify-content-between flex-grow-1">
        <div>
          {/* Tên thương hiệu ngẫu nhiên */}
          <span className="text-uppercase text-secondary font-weight-bold" style={{ fontSize: '0.75rem', letterSpacing: '0.5px' }}>
            {brands[item.id % brands.length]}
          </span>
          <h6 className="card-title font-weight-bold text-dark mt-1 text-truncate-2" style={{ fontSize: '0.9rem', height: '38px', overflow: 'hidden' }}>{item.name}</h6>
        </div>
        <div>
          {/* Đơn giá định dạng chuẩn */}
          <p className="card-text text-danger font-weight-bold mb-1" style={{ fontSize: '1rem' }}>
            {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.price)}
          </p>
          <div className="d-flex justify-content-between align-items-center text-muted small mt-2">
            <span>SKU: #{1000 + item.id}</span>
            {item.stockQuantity > 0 ? (
              <span className="badge bg-success-light text-success border border-success rounded px-2" style={{ fontSize: '0.7rem' }}>Tồn: {item.stockQuantity}</span>
            ) : (
              <span className="badge bg-danger-light text-danger border border-danger rounded px-2" style={{ fontSize: '0.7rem' }}>Hết hàng</span>
            )}
          </div>
        </div>
      </div>

      {/* Chân thẻ sản phẩm chứa nút */}
      <div className="card-footer bg-transparent border-top-0 p-3 pt-0">
        <div className="d-flex gap-2">
          <Link to={`/products/${item.id}`} className="btn btn-outline-danger btn-sm rounded-pill font-weight-bold text-uppercase py-2 flex-grow-1 mr-2" style={{ fontSize: '0.75rem' }}>
            Chi tiết
          </Link>
          <button className="btn btn-danger btn-sm rounded-pill font-weight-bold text-uppercase py-2 flex-grow-1" style={{ fontSize: '0.75rem' }} disabled={item.stockQuantity === 0}>
            <i className="fa-solid fa-cart-shopping mr-1"></i> Mua ngay
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
