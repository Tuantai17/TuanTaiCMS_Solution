import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import productService from '../services/productService';

const ProductDetail = () => {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProductDetail = async () => {
      try {
        setLoading(true);
        const data = await productService.getProductDetail(id);
        setProduct(data);
      } catch (error) {
        console.error("Lỗi khi tải chi tiết sản phẩm:", error);
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

  const brands = ["LEGO", "SCOOTER", "TANGLE", "FISHER PRICE", "HOT WHEELS", "BARBIE"];

  return (
    <div className="container mt-4">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item"><a href="/products" className="text-secondary text-decoration-none">Sản phẩm</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold text-truncate" aria-current="page" style={{ maxWidth: '300px' }}>{product.name}</li>
        </ol>
      </nav>

      <div className="row bg-white p-4 rounded-lg shadow-sm border border-light">
        {/* CỘT TRÁI: ẢNH SẢN PHẨM LỚN */}
        <div className="col-12 col-md-6 text-center mb-4 mb-md-0 bg-light rounded-lg d-flex align-items-center justify-content-center p-4" style={{ minHeight: '350px' }}>
          <img 
            src={product.imageUrl || "https://placehold.co/400x300/e9ecef/6c757d?text=No+Image"} 
            className="img-fluid object-fit-contain transition-all" 
            alt={product.name} 
            style={{ maxHeight: '320px', objectFit: 'contain' }}
          />
        </div>

        {/* CỘT PHẢI: THÔNG TIN CHI TIẾT SẢN PHẨM */}
        <div className="col-12 col-md-6 pl-md-5 d-flex flex-column justify-content-between">
          <div>
            {/* Nhãn hiệu & Tên sản phẩm */}
            <span className="badge badge-danger px-3 py-2 rounded-pill font-weight-bold mb-3 text-uppercase" style={{ fontSize: '0.75rem', letterSpacing: '0.5px' }}>
              {brands[product.id % brands.length]} CHÍNH HÃNG
            </span>
            <h2 className="h3 font-weight-bold text-dark mb-2" style={{ lineHeight: '1.3' }}>
              {product.name}
            </h2>
            <p className="text-muted small mb-3">Mã SKU sản phẩm: <strong className="text-dark">#{(1000 + product.id)}</strong></p>

            {/* Đơn giá */}
            <div className="price-tag-wrapper py-3 px-4 bg-light rounded-lg mb-4">
              <span className="text-secondary small font-weight-bold mr-2">Giá niêm yết:</span>
              <span className="h3 text-danger font-weight-bold mb-0">
                {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(product.price)}
              </span>
            </div>

            {/* Thông số & Tình trạng tồn kho */}
            <div className="mb-4" style={{ fontSize: '0.9rem' }}>
              <p className="mb-2"><i className="fa-solid fa-square-check text-success mr-2"></i> Trạng thái: 
                {product.stockQuantity > 0 ? (
                  <span className="badge bg-success text-white ml-2 px-2 py-1 rounded">Còn hàng (Tồn: {product.stockQuantity})</span>
                ) : (
                  <span className="badge bg-danger text-white ml-2 px-2 py-1 rounded">Hết hàng tạm thời</span>
                )}
              </p>
              <p className="mb-2"><i className="fa-solid fa-cube text-primary mr-2"></i> Danh mục phân loại: <strong className="text-dark">{product.categoryProduct?.name || 'Đồ chơi phát triển trí tuệ'}</strong></p>
              <p className="mb-2"><i className="fa-solid fa-shield-halved text-warning mr-2"></i> Bảo hành chính hãng: <strong className="text-dark">Đổi trả miễn phí 3 ngày lỗi NSX</strong></p>
            </div>

            {/* Mô tả chi tiết */}
            <div className="border-top pt-3 mt-3">
              <h6 className="font-weight-bold text-dark mb-2"><i className="fa-solid fa-circle-info mr-2 text-secondary"></i> Mô tả sản phẩm:</h6>
              <p className="text-secondary small lh-lg mb-0" style={{ fontSize: '0.85rem' }}>
                {product.description || "Đang tiến hành bổ sung nội dung mô tả chi tiết chất liệu, phom dáng và hướng dẫn cách chơi cặn kẽ cho bộ sản phẩm đồ chơi thông minh cao cấp này."}
              </p>
            </div>
          </div>

          {/* Cụm nút hành động */}
          <div className="border-top pt-4 mt-4">
            <div className="row">
              <div className="col-12 col-sm-6 mb-2 mb-sm-0">
                <button className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3" disabled={product.stockQuantity === 0}>
                  <i className="fa-solid fa-cart-plus mr-2"></i> Thêm vào giỏ
                </button>
              </div>
              <div className="col-12 col-sm-6">
                <Link to="/products" className="btn btn-outline-secondary btn-block rounded-pill font-weight-bold text-uppercase py-3">
                  Quay lại chọn thêm
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;
