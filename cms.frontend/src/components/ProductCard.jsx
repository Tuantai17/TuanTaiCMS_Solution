import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { getMediaUrl } from '../utils/mediaUrl';

const ProductCard = ({ item }) => {
  const [isFavorite, setIsFavorite] = useState(false);

  // Định dạng đơn giá VND chuẩn Việt Nam
  const formatCurrency = (value) => {
    return new Intl.NumberFormat('vi-VN').format(value) + " VND";
  };

  // Sử dụng dữ liệu thực từ API: discountPercent, salePrice, isNew, isSale
  const isSale = item.isSale === true && item.salePrice > 0;
  const isNew = item.isNew === true;
  const discountPercent = item.discountPercent || 0;
  const displayPrice = isSale ? item.salePrice : item.price;
  const originalPrice = item.price;
  const imageUrl = getMediaUrl(item.imageUrl, "https://placehold.co/200x200/e9ecef/6c757d?text=No+Image");

  // Xác định nhãn tag bên trái
  const tagLabel = isNew ? 'NEW' : (item.isBestSelling ? 'HOT' : (item.tagLabel || (item.price > 500000 ? 'LEGO' : 'SẢN PHẨM')));
  const tagColor = isNew ? '#28a745' : (item.isBestSelling ? '#e0a800' : '#002664');

  return (
    <div className="card h-100 shadow-sm border border-light product-card-hover rounded-lg overflow-hidden d-flex flex-column" style={{ transition: 'all 0.3s', borderRadius: '16px' }}>
      {/* Khối chứa ảnh & các nhãn Tag */}
      <div className="position-relative p-3 text-center d-flex align-items-center justify-content-center bg-white" style={{ height: '200px' }}>
        {/* Nhãn Tag bên trái (NEW / OUTLET / LEGO) */}
        <span className="position-absolute badge text-white px-2 py-1" style={{ 
          top: '12px', 
          left: '12px', 
          fontSize: '0.75rem', 
          fontWeight: 'bold',
          backgroundColor: tagColor,
          borderRadius: '4px',
          textTransform: 'uppercase',
          zIndex: 2
        }}>
          {tagLabel}
        </span>

        {/* Nhãn phần trăm giảm giá bên phải - chỉ hiển thị khi có Sale */}
        {isSale && discountPercent > 0 && (
          <span className="position-absolute badge text-white px-2 py-1" style={{ 
            top: '12px', 
            right: '12px', 
            fontSize: '0.75rem', 
            fontWeight: 'bold',
            backgroundColor: '#CF102D',
            borderRadius: '4px',
            zIndex: 2
          }}>
            -{discountPercent}%
          </span>
        )}

        <Link to={`/products/${item.id}`} className="d-block w-100 h-100 d-flex align-items-center justify-content-center">
          <img 
            src={imageUrl} 
            className="img-fluid" 
            alt={item.name} 
            style={{ maxHeight: '160px', maxWidth: '100%', objectFit: 'contain', transition: 'transform 0.4s ease' }} 
          />
        </Link>
      </div>

      {/* Nội dung chi tiết thông tin */}
      <div className="card-body p-3 d-flex flex-column justify-content-between flex-grow-1" style={{ borderTop: '1px solid #f2f2f2' }}>
        <div>
          {/* Cột trên: Thương hiệu & SKU */}
          <div className="d-flex justify-content-between align-items-center mb-1">
            <span className="text-uppercase text-muted font-weight-bold" style={{ fontSize: '0.7rem', letterSpacing: '0.5px' }}>
              {item.brandName || (item.id % 3 === 0 ? 'BEYBLADE 6' : (item.id % 2 === 0 ? 'YOYO 22' : 'LEGO CITY'))}
            </span>
            <span className="text-muted" style={{ fontSize: '0.7rem' }}>
              SKU: {item.sku || `SKU${120000 + item.id}`}
            </span>
          </div>

          {/* Cột giữa: Tiêu đề sản phẩm */}
          <h6 className="card-title font-weight-bold mt-1 text-truncate-2" style={{ 
            fontSize: '0.85rem', 
            height: '38px', 
            overflow: 'hidden',
            color: '#002664',
            lineHeight: '1.4'
          }}>
            <Link to={`/products/${item.id}`} className="text-decoration-none hover-danger" style={{ color: '#002664' }}>
              {item.name}
            </Link>
          </h6>
        </div>

        {/* Cột dưới: Giá bán thực tế & Giá gốc cũ */}
        <div className="mt-2">
          <div className="d-flex align-items-baseline flex-wrap">
            <span className="text-danger font-weight-bold mr-2" style={{ fontSize: '1rem', color: '#CF102D' }}>
              {formatCurrency(displayPrice)}
            </span>
            {isSale && (
              <span className="text-muted text-decoration-line-through small" style={{ fontSize: '0.75rem', textDecoration: 'line-through', opacity: 0.6 }}>
                {formatCurrency(originalPrice)}
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Khối chân thẻ chứa nút bấm mua và tim yêu thích */}
      <div className="card-footer bg-white border-top-0 p-3 pt-0">
        <div className="d-flex align-items-center justify-content-between">
          <button 
            onClick={(e) => {
              e.preventDefault();
              const cart = JSON.parse(localStorage.getItem('cart') || '[]');
              const index = cart.findIndex(c => c.id === item.id);
              if (index > -1) {
                cart[index].quantity += 1;
              } else {
                cart.push({
                  id: item.id,
                  name: item.name,
                  price: displayPrice,
                  quantity: 1,
                  imageUrl,
                  sku: item.sku || `SKU${120000 + item.id}`
                });
              }
              localStorage.setItem('cart', JSON.stringify(cart));
              window.dispatchEvent(new Event('cartChange'));
              alert(`Đã thêm "${item.name}" vào giỏ hàng thành công!`);
            }}
            className="btn font-weight-bold text-uppercase py-2 flex-grow-1 mr-2" 
            style={{ 
              fontSize: '0.75rem',
              backgroundColor: '#CF102D',
              borderColor: '#CF102D',
              color: '#ffffff',
              borderRadius: '25px',
              borderWidth: '1px',
              borderStyle: 'solid',
              transition: 'all 0.2s'
            }}
          >
            Thêm Vào Giỏ Hàng
          </button>
          
          <button 
            onClick={(e) => { e.preventDefault(); setIsFavorite(!isFavorite); }}
            className="btn btn-outline-danger d-flex align-items-center justify-content-center" 
            style={{ 
              width: '36px', 
              height: '36px', 
              borderRadius: '50%', 
              padding: '0',
              borderColor: '#CF102D',
              color: isFavorite ? '#ffffff' : '#CF102D',
              backgroundColor: isFavorite ? '#CF102D' : 'transparent',
              transition: 'all 0.3s ease'
            }}
          >
            <i className={`fa-${isFavorite ? 'solid' : 'regular'} fa-heart`} style={{ fontSize: '0.9rem' }}></i>
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
