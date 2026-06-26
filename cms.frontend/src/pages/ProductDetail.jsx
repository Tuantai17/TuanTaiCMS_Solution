import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import productService from '../services/productService';
import favoriteService from '../services/favoriteService';
import { getMediaUrl } from '../utils/mediaUrl';
import ProductCard from '../components/ProductCard';
import { useFavorite } from '../contexts/FavoriteContext';

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeImageIndex, setActiveImageIndex] = useState(0);
  const [quantity, setQuantity] = useState(1);
  const [thumbnailStartIndex, setThumbnailStartIndex] = useState(0);
  const [isExpanded, setIsExpanded] = useState(false);
  const [relatedProducts, setRelatedProducts] = useState([]);
  const [relatedStartIndex, setRelatedStartIndex] = useState(0);
  const [isFavorite, setIsFavorite] = useState(false);
  const { toggleFavorite } = useFavorite();

  useEffect(() => {
    const fetchProductDetail = async () => {
      try {
        setLoading(true);
        const data = await productService.getProductDetail(id);
        setProduct(data);
        setActiveImageIndex(0); // Reset ảnh chính về ảnh đầu tiên khi chuyển sản phẩm
        setThumbnailStartIndex(0); // Reset vị trí thumbnail
        setQuantity(1); // Reset số lượng về 1
        setIsExpanded(false); // Reset trạng thái mở rộng mô tả
        setRelatedStartIndex(0); // Reset trượt sản phẩm liên quan

        if (localStorage.getItem('customer')) {
          favoriteService.checkStatus(id).then(res => setIsFavorite(res.isFavorite)).catch(() => {});
        } else {
          setIsFavorite(false);
        }

        // Lấy sản phẩm liên quan
        const catId = data.categoryProductId || data.categoryProduct?.id;
        let relatedData = [];
        if (catId) {
          relatedData = await productService.getProductsByCategory(catId);
        } else {
          relatedData = await productService.getBestSellingProducts(8);
        }
        // Lọc bỏ sản phẩm hiện tại
        const filtered = relatedData.filter(p => p.id !== data.id);
        setRelatedProducts(filtered);
      } catch (error) {
        console.error("Lỗi khi tải chi tiết sản phẩm và sản phẩm liên quan:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchProductDetail();
  }, [id]);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang tải thông tin sản phẩm cao cấp...</p>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="container text-center my-5 py-5">
        <i className="fa-solid fa-triangle-exclamation text-warning fs-1 mb-3"></i>
        <h4 className="font-weight-bold">Không tìm thấy sản phẩm này!</h4>
        <p className="text-secondary">Sản phẩm có thể đã ngừng kinh doanh hoặc đường dẫn không đúng.</p>
        <Link to="/products" className="btn btn-danger rounded-pill px-4 mt-3">Quay lại Cửa hàng</Link>
      </div>
    );
  }

  // Lấy toàn bộ album ảnh (ảnh chính ở vị trí số 0)
  const images = [];
  if (product.imageUrl) {
    images.push(product.imageUrl);
  }
  if (product.productImages && product.productImages.length > 0) {
    product.productImages.forEach(img => {
      if (img.imageUrl && !images.includes(img.imageUrl)) {
        images.push(img.imageUrl);
      }
    });
  }
  if (images.length === 0) {
    images.push("https://placehold.co/400x300/e9ecef/6c757d?text=No+Image");
  }

  const activeImageUrl = getMediaUrl(images[activeImageIndex], "https://placehold.co/400x300/e9ecef/6c757d?text=No+Image");
  const isSale = product.isSale === true && product.salePrice > 0;
  const displayPrice = isSale ? product.salePrice : product.price;

  const handleAddToCart = () => {
    const qtyToUse = parseInt(quantity, 10) || 1;
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const index = cart.findIndex(c => c.id === product.id);
    const currentQtyInCart = index > -1 ? cart[index].quantity : 0;

    if (currentQtyInCart + qtyToUse > product.stockQuantity) {
      alert(`Số lượng sản phẩm trong kho không đủ! (Tồn kho: ${product.stockQuantity})`);
      return;
    }

    if (index > -1) {
      cart[index].quantity += qtyToUse;
      cart[index].price = displayPrice; // Cập nhật lại giá mới nhất
    } else {
      cart.push({
        id: product.id,
        name: product.name,
        price: displayPrice,
        quantity: qtyToUse,
        imageUrl: getMediaUrl(product.imageUrl),
        sku: product.sku || `#${1000 + product.id}`
      });
    }
    localStorage.setItem('cart', JSON.stringify(cart));
    window.dispatchEvent(new Event('cartChange'));
    alert(`Đã thêm ${qtyToUse} sản phẩm "${product.name}" vào giỏ hàng thành công!`);
  };

  const handleBuyNow = () => {
    const qtyToUse = parseInt(quantity, 10) || 1;
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const index = cart.findIndex(c => c.id === product.id);
    const currentQtyInCart = index > -1 ? cart[index].quantity : 0;

    if (currentQtyInCart + qtyToUse > product.stockQuantity) {
      alert(`Số lượng sản phẩm trong kho không đủ! (Tồn kho: ${product.stockQuantity})`);
      return;
    }

    if (index > -1) {
      cart[index].quantity += qtyToUse;
      cart[index].price = displayPrice;
    } else {
      cart.push({
        id: product.id,
        name: product.name,
        price: displayPrice,
        quantity: qtyToUse,
        imageUrl: getMediaUrl(product.imageUrl),
        sku: product.sku || `#${1000 + product.id}`
      });
    }
    localStorage.setItem('cart', JSON.stringify(cart));
    window.dispatchEvent(new Event('cartChange'));
    navigate('/cart');
  };

  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
          <li className="breadcrumb-item"><Link to="/products" className="text-secondary text-decoration-none">Sản phẩm</Link></li>
          <li className="breadcrumb-item active text-danger font-weight-bold text-truncate" aria-current="page" style={{ maxWidth: '300px' }}>{product.name}</li>
        </ol>
      </nav>

      <div className="row bg-white p-4 rounded-lg shadow-sm border border-light">
        {/* CỘT TRÁI: ẢNH SẢN PHẨM LỚN & GALLERY */}
        <div className="col-12 col-md-6 mb-4 mb-md-0 d-flex flex-column align-items-center">
          <div className="w-100 text-center bg-light rounded-lg d-flex align-items-center justify-content-center p-4" style={{ minHeight: '350px', height: '350px' }}>
            <img 
              src={activeImageUrl} 
              className="img-fluid object-fit-contain transition-all" 
              alt={product.name} 
              style={{ maxHeight: '320px', objectFit: 'contain', transition: 'all 0.3s ease' }}
            />
          </div>
          
          {/* Thumbnails list */}
          {images.length > 1 && (
            <div className="d-flex align-items-center justify-content-center mt-3 w-100">
              {/* Nút Prev */}
              <button
                className="thumb-nav-btn mr-2"
                onClick={() => setThumbnailStartIndex(prev => Math.max(0, prev - 1))}
                disabled={thumbnailStartIndex === 0}
              >
                <i className="fa-solid fa-chevron-left" style={{ fontSize: '0.8rem' }}></i>
              </button>

              {/* Các thumbnail hiển thị */}
              <div className="d-flex">
                {images.slice(thumbnailStartIndex, thumbnailStartIndex + 4).map((imgUrl, idx) => {
                  const absoluteIdx = thumbnailStartIndex + idx;
                  const isCurrent = absoluteIdx === activeImageIndex;
                  return (
                    <div
                      key={absoluteIdx}
                      onClick={() => setActiveImageIndex(absoluteIdx)}
                      className="rounded overflow-hidden border mx-1"
                      style={{
                        width: '60px',
                        height: '60px',
                        cursor: 'pointer',
                        border: isCurrent ? '2px solid #CF102D' : '1px solid #dee2e6',
                        opacity: isCurrent ? 1 : 0.6,
                        transition: 'all 0.2s',
                        boxShadow: isCurrent ? '0 0 4px rgba(207, 16, 45, 0.4)' : 'none'
                      }}
                    >
                      <img src={getMediaUrl(imgUrl)} alt="" style={{ width: '100%', height: '100%', objectFit: 'contain', backgroundColor: '#fff' }} />
                    </div>
                  );
                })}
              </div>

              {/* Nút Next */}
              <button
                className="thumb-nav-btn ml-2"
                onClick={() => setThumbnailStartIndex(prev => Math.min(images.length - 4, prev + 1))}
                disabled={thumbnailStartIndex + 4 >= images.length}
              >
                <i className="fa-solid fa-chevron-right" style={{ fontSize: '0.8rem' }}></i>
              </button>
            </div>
          )}
        </div>

        {/* CỘT PHẢI: THÔNG TIN CHI TIẾT SẢN PHẨM */}
        <div className="col-12 col-md-6 pl-md-5 d-flex flex-column justify-content-between">
          <div>
            {/* Tên sản phẩm */}
            <h2 className="h3 font-weight-bold text-dark mb-2" style={{ lineHeight: '1.3' }}>
              {product.name}
            </h2>

            {/* Mã sản phẩm */}
            <div className="d-flex align-items-center mb-3" style={{ fontSize: '0.9rem' }}>
              <span className="text-secondary">Mã sản phẩm:</span>
              <span className="font-weight-bold ml-1">#{(1000 + product.id)}</span>
            </div>

            {/* Đơn giá */}
            <div className="mb-4">
              <div className="d-flex align-items-baseline flex-wrap">
                <span className="text-secondary mr-2" style={{ fontSize: '1rem' }}>Giá bán</span>
                <span className="h3 text-danger font-weight-bold mb-0 mr-3">
                  {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(displayPrice)}
                </span>
                {isSale && (
                  <>
                    <span className="text-muted text-decoration-line-through mr-3" style={{ fontSize: '1rem' }}>
                      {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(product.price)}
                    </span>
                    <span className="badge badge-danger px-2 py-1" style={{ fontSize: '0.75rem', borderRadius: '4px', backgroundColor: '#CF102D' }}>
                      -{Math.round((1 - product.salePrice / product.price) * 100)}%
                    </span>
                  </>
                )}
              </div>
              <p className="text-muted small mt-2 font-italic mb-0" style={{ lineHeight: '1.4' }}>
                Đã bao gồm VAT và các khoản thuế/phí theo quy định
                <br />
                (Chưa bao gồm phí vận chuyển)
              </p>
            </div>

            {/* Chính sách checkmark */}
            <ul className="list-unstyled mb-4 text-dark" style={{ fontSize: '0.9rem', lineHeight: '2' }}>
              <li className="d-flex align-items-start mb-2">
                <i className="fa-regular fa-circle-check text-success mr-2 mt-1" style={{ fontSize: '1.05rem' }}></i>
                <span>Sản phẩm chính hãng 100%</span>
              </li>
              <li className="d-flex align-items-start mb-2">
                <i className="fa-regular fa-circle-check text-success mr-2 mt-1" style={{ fontSize: '1.05rem' }}></i>
                <span>Chất liệu an toàn cho trẻ em</span>
              </li>

              <li className="d-flex align-items-start mb-2">
                <i className="fa-regular fa-circle-check text-success mr-2 mt-1" style={{ fontSize: '1.05rem' }}></i>
                <span>Giao hàng hỏa tốc 4 tiếng. <a href="#!" className="text-danger font-weight-bold ml-1 text-decoration-underline" onClick={e => e.preventDefault()}>Xem chi tiết</a></span>
              </li>
              <li className="d-flex align-items-start mb-2">
                <i className="fa-regular fa-circle-check text-success mr-2 mt-1" style={{ fontSize: '1.05rem' }}></i>
                <span>Hỗ trợ trả góp đơn hàng từ 3 triệu. <a href="#!" className="text-danger font-weight-bold ml-1 text-decoration-underline" onClick={e => e.preventDefault()}>Xem chi tiết</a></span>
              </li>
            </ul>

            {/* Trạng thái tồn kho pill */}
            <div className="mb-4">
              {product.stockQuantity > 0 ? (
                <div 
                  className="d-inline-flex align-items-center rounded-pill px-3 py-1 font-weight-bold border" 
                  style={{ 
                    fontSize: '0.85rem', 
                    backgroundColor: '#E8F5E9', 
                    borderColor: '#C8E6C9', 
                    color: '#2E7D32' 
                  }}
                >
                  <span className="rounded-circle mr-2" style={{ width: '8px', height: '8px', backgroundColor: '#2E7D32', display: 'inline-block' }}></span>
                  Còn {product.stockQuantity} sản phẩm
                  <i className="fa-solid fa-chevron-right ml-2 text-muted" style={{ fontSize: '0.75rem' }}></i>
                </div>
              ) : (
                <div 
                  className="d-inline-flex align-items-center rounded-pill px-3 py-1 font-weight-bold border" 
                  style={{ 
                    fontSize: '0.85rem', 
                    backgroundColor: '#FFEBEE', 
                    borderColor: '#FFCDD2', 
                    color: '#C62828' 
                  }}
                >
                  <span className="rounded-circle mr-2" style={{ width: '8px', height: '8px', backgroundColor: '#C62828', display: 'inline-block' }}></span>
                  Hết hàng tạm thời
                </div>
              )}
            </div>

            {/* Chọn số lượng */}
            <div className="mb-4">
              <div className="font-weight-bold mb-2 text-dark" style={{ fontSize: '0.95rem' }}>Số lượng</div>
              <div className="d-flex align-items-center">
                <button 
                  onClick={() => setQuantity(prev => Math.max(1, (parseInt(prev, 10) || 1) - 1))}
                  className="btn btn-outline-secondary d-flex align-items-center justify-content-center"
                  style={{ width: '38px', height: '38px', borderRadius: '4px', border: '1px solid #dee2e6' }}
                  disabled={product.stockQuantity === 0}
                >
                  <i className="fa-solid fa-minus"></i>
                </button>
                <input 
                  type="text" 
                  value={quantity} 
                  onChange={(e) => {
                    const val = e.target.value;
                    if (val === '') {
                      setQuantity('');
                      return;
                    }
                    const parsed = parseInt(val, 10);
                    if (isNaN(parsed)) return;
                    if (parsed < 1) {
                      setQuantity(1);
                    } else if (parsed > product.stockQuantity) {
                      setQuantity(product.stockQuantity);
                    } else {
                      setQuantity(parsed);
                    }
                  }}
                  onBlur={() => {
                    if (quantity === '' || quantity < 1) {
                      setQuantity(1);
                    }
                  }}
                  className="form-control text-center mx-2" 
                  style={{ width: '55px', height: '38px', borderRadius: '4px', border: '1px solid #dee2e6', fontWeight: 'bold' }}
                />
                <button 
                  onClick={() => setQuantity(prev => Math.min(product.stockQuantity, (parseInt(prev, 10) || 1) + 1))}
                  className="btn btn-outline-secondary d-flex align-items-center justify-content-center"
                  style={{ width: '38px', height: '38px', borderRadius: '4px', border: '1px solid #dee2e6' }}
                  disabled={product.stockQuantity === 0 || Number(quantity) >= product.stockQuantity}
                >
                  <i className="fa-solid fa-plus"></i>
                </button>
              </div>
            </div>
          </div>

          {/* Cụm nút hành động */}
          <div className="d-flex gap-3 mt-4 w-100 flex-wrap flex-sm-nowrap">
            <button 
              onClick={handleAddToCart}
              className="btn font-weight-bold text-uppercase py-3 px-2 flex-grow-1 d-flex align-items-center justify-content-center shadow-sm"
              style={{
                border: '2px solid',
                color: product.stockQuantity === 0 ? '#6c757d' : '#CF102D',
                backgroundColor: product.stockQuantity === 0 ? '#e9ecef' : '#FFF0F2',
                borderColor: product.stockQuantity === 0 ? '#ced4da' : '#CF102D',
                borderRadius: '8px',
                fontSize: '0.88rem',
                transition: 'all 0.2s',
                cursor: product.stockQuantity === 0 ? 'not-allowed' : 'pointer'
              }}
              disabled={product.stockQuantity === 0}
            >
              <i className="fa-solid fa-cart-shopping mr-2"></i> {product.stockQuantity === 0 ? 'Hết Hàng' : 'Thêm Vào Giỏ Hàng'}
            </button>
            <button 
              onClick={handleBuyNow}
              className={`btn ${product.stockQuantity === 0 ? 'btn-secondary' : 'btn-danger'} font-weight-bold text-uppercase py-3 px-2 flex-grow-1 d-flex align-items-center justify-content-center shadow-sm`}
              style={{
                backgroundColor: product.stockQuantity === 0 ? '#6c757d' : '#CF102D',
                borderColor: product.stockQuantity === 0 ? '#6c757d' : '#CF102D',
                color: '#fff',
                borderRadius: '8px',
                fontSize: '0.88rem',
                transition: 'all 0.2s',
                cursor: product.stockQuantity === 0 ? 'not-allowed' : 'pointer'
              }}
              disabled={product.stockQuantity === 0}
            >
              {product.stockQuantity === 0 ? 'Hết Hàng' : 'Mua Ngay'}
            </button>
            <button 
              onClick={async (e) => {
                e.preventDefault();
                const newStatus = await toggleFavorite(product.id, isFavorite);
                setIsFavorite(newStatus);
              }}
              className="btn font-weight-bold text-uppercase py-3 px-3 d-flex align-items-center justify-content-center shadow-sm"
              style={{
                backgroundColor: isFavorite ? '#CF102D' : '#ffffff',
                borderColor: '#CF102D',
                color: isFavorite ? '#ffffff' : '#CF102D',
                border: '2px solid',
                borderRadius: '8px',
                fontSize: '1.2rem',
                transition: 'all 0.2s'
              }}
            >
              <i className={`fa-${isFavorite ? 'solid' : 'regular'} fa-heart`}></i>
            </button>
          </div>
        </div>
      </div>

      {/* CỘT DƯỚI: MÔ TẢ CHI TIẾT SẢN PHẨM */}
      <div className="row mt-5 border-top pt-4">
        <div className="col-12">
          {/* Header */}
          <div className="mb-4 border-bottom pb-2">
            <h4 className="font-weight-bold text-danger text-uppercase" style={{ fontSize: '1rem', letterSpacing: '0.5px' }}>
              Mô tả sản phẩm
            </h4>
          </div>

          {/* Content Container */}
          <div className="bg-white p-4 rounded-lg shadow-sm border border-light mb-5">
            <div>
              <h4 className="font-weight-bold text-dark mb-4 text-center text-md-left" style={{ fontSize: '1.25rem' }}>
                {product.name}
              </h4>
              <div 
                style={{ 
                  position: 'relative', 
                  maxHeight: isExpanded ? 'none' : '200px', 
                  overflow: 'hidden',
                  transition: 'max-height 0.4s ease'
                }}
              >
                <p className="text-secondary lh-lg mb-0 font-weight-normal" style={{ fontSize: '0.92rem', whiteSpace: 'pre-line' }}>
                  {product.description || "Đang tiến hành bổ sung nội dung mô tả chi tiết chất liệu, phom dáng và hướng dẫn cách chơi cặn kẽ cho bộ sản phẩm đồ chơi thông minh cao cấp này."}
                </p>
                {!isExpanded && (
                  <div style={{
                    position: 'absolute',
                    bottom: 0,
                    left: 0,
                    right: 0,
                    height: '80px',
                    background: 'linear-gradient(to bottom, rgba(255, 255, 255, 0), rgba(255, 255, 255, 1))'
                  }}></div>
                )}
              </div>
              
              {/* Expand / Collapse Button */}
              <div className="text-center mt-4 border-top pt-2">
                <button 
                  onClick={() => setIsExpanded(!isExpanded)}
                  className="btn btn-link text-danger font-weight-bold text-decoration-none"
                  style={{ fontSize: '0.95rem' }}
                >
                  {isExpanded ? 'Thu gọn' : 'Xem thêm thông tin'}
                  <i className={`fa-solid fa-chevron-${isExpanded ? 'up' : 'down'} ml-2`}></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* SẢN PHẨM LIÊN QUAN */}
      {relatedProducts && relatedProducts.length > 0 && (
        <div className="my-5">
          <h3 className="font-weight-bold text-center mb-4" style={{ color: '#002664', fontSize: '1.5rem' }}>
            Sản Phẩm Liên Quan
          </h3>
          
          <div className="position-relative d-flex align-items-center justify-content-center px-0 px-md-4">
            {relatedProducts.length > 4 && (
              <button
                className="thumb-nav-btn position-absolute d-none d-md-flex"
                style={{ left: '-20px', zIndex: 10, width: '40px', height: '40px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}
                onClick={() => setRelatedStartIndex(prev => Math.max(0, prev - 1))}
                disabled={relatedStartIndex === 0}
              >
                <i className="fa-solid fa-chevron-left" style={{ fontSize: '1rem' }}></i>
              </button>
            )}

            <div className="w-100 overflow-hidden">
              <div className="row justify-content-center">
                {relatedProducts
                  .slice(relatedStartIndex, relatedProducts.length > 4 ? relatedStartIndex + 4 : relatedProducts.length)
                  .map((item) => (
                    <div key={item.id} className="col-12 col-sm-6 col-md-4 col-lg-3 mb-4">
                      <ProductCard item={item} />
                    </div>
                  ))}
              </div>
            </div>

            {relatedProducts.length > 4 && (
              <button
                className="thumb-nav-btn position-absolute d-none d-md-flex"
                style={{ right: '-20px', zIndex: 10, width: '40px', height: '40px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}
                onClick={() => setRelatedStartIndex(prev => Math.min(relatedProducts.length - 4, prev + 1))}
                disabled={relatedStartIndex + 4 >= relatedProducts.length}
              >
                <i className="fa-solid fa-chevron-right" style={{ fontSize: '1rem' }}></i>
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductDetail;

