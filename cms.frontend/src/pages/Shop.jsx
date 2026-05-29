import React, { useState, useEffect } from 'react';
import categoryProductService from '../services/categoryProductService';
import productService from '../services/productService';
import ProductCard from '../components/ProductCard';

const Shop = () => {
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [prodLoading, setProdLoading] = useState(false);

  // 1. Tải toàn bộ danh mục sản phẩm khi component mount
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        setLoading(true);
        const data = await categoryProductService.getAllCategoryProducts();
        setCategories(data);
      } catch (error) {
        console.error("Lỗi khi tải danh mục sản phẩm:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchCategories();
  }, []);

  // 2. Tải danh sách sản phẩm (chạy lại mỗi khi selectedCategoryId thay đổi)
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setProdLoading(true);
        let data = [];
        if (selectedCategoryId === null) {
          data = await productService.getAllProducts();
        } else {
          data = await productService.getProductsByCategory(selectedCategoryId);
        }
        setProducts(data);
      } catch (error) {
        console.error("Lỗi khi tải sản phẩm:", error);
      } finally {
        setProdLoading(false);
      }
    };
    fetchProducts();
  }, [selectedCategoryId]);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang tải cấu trúc danh mục và cửa hàng...</p>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      {/* breadcrumb nhỏ xinh xắn */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tất cả sản phẩm</li>
        </ol>
      </nav>

      <div className="row">
        {/* SIDEBAR BÊN TRÁI: DANH MỤC LỌC SẢN PHẨM */}
        <div className="col-12 col-md-3 mb-4">
          <div className="card shadow-sm border border-light rounded-lg overflow-hidden">
            <div className="card-header bg-danger text-white py-3 px-4 d-flex align-items-center">
              <h6 className="card-title font-weight-bold text-uppercase mb-0" style={{ letterSpacing: '0.5px', fontSize: '0.95rem' }}>
                <i className="fa-solid fa-filter mr-2"></i> Danh Mục Đồ Chơi
              </h6>
            </div>
            <div className="list-group list-group-flush">
              <button
                type="button"
                className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center px-4 py-3 font-weight-bold text-uppercase border-bottom ${
                  selectedCategoryId === null ? 'bg-light text-danger' : 'text-secondary'
                }`}
                onClick={() => setSelectedCategoryId(null)}
                style={{ fontSize: '0.85rem', transition: 'all 0.2s' }}
              >
                <span>Tất cả sản phẩm</span>
                <span className="badge badge-pill badge-danger font-weight-bold">{products.length}</span>
              </button>
              
              {categories.length === 0 ? (
                <div className="p-4 text-center text-muted small">Không có danh mục nào.</div>
              ) : (
                categories.map((item) => (
                  <button
                    key={item.id}
                    type="button"
                    className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center px-4 py-3 border-bottom transition-all ${
                      selectedCategoryId === item.id ? 'bg-danger text-white font-weight-bold' : 'text-secondary'
                    }`}
                    onClick={() => setSelectedCategoryId(item.id)}
                    style={{ fontSize: '0.9rem', transition: 'all 0.2s' }}
                  >
                    <span>{item.name}</span>
                    <i className={`fa-solid fa-chevron-right small ${selectedCategoryId === item.id ? 'text-white' : 'text-muted-50'}`} style={{ fontSize: '0.75rem', opacity: 0.6 }}></i>
                  </button>
                ))
              )}
            </div>
          </div>
        </div>

        {/* LƯỚI DANH SÁCH SẢN PHẨM BÊN PHẢI */}
        <div className="col-12 col-md-9">
          <div className="d-flex justify-content-between align-items-center mb-4 bg-light border p-3 rounded-lg shadow-sm">
            <span className="text-secondary font-weight-medium small" style={{ fontSize: '0.9rem' }}>
              Hiển thị: <strong>{products.length}</strong> sản phẩm phù hợp
            </span>
            <div className="d-flex align-items-center" style={{ fontSize: '0.85rem' }}>
              <span className="text-muted mr-2">Sắp xếp:</span>
              <select className="form-control form-control-sm border-secondary shadow-none rounded-pill px-3" style={{ width: '130px' }}>
                <option>Mặc định</option>
                <option>Giá tăng dần</option>
                <option>Giá giảm dần</option>
              </select>
            </div>
          </div>

          {prodLoading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-danger" role="status">
                <span className="sr-only">Đang tải sản phẩm...</span>
              </div>
              <p className="mt-3 text-secondary">Đang cập nhật danh sách đồ chơi...</p>
            </div>
          ) : (
            <div className="row">
              {products.length === 0 ? (
                <div className="col-12 text-center py-5 border rounded-lg bg-light">
                  <i className="fa-solid fa-box-open text-muted-50 fs-1 mb-3"></i>
                  <p className="text-secondary font-weight-medium mb-0">Chưa có sản phẩm nào thuộc danh mục này.</p>
                </div>
              ) : (
                products.map((item) => (
                  <div className="col-12 col-sm-6 col-md-4 mb-4" key={item.id}>
                    <ProductCard item={item} />
                  </div>
                ))
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Shop;
