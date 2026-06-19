import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import ProductCard from '../components/ProductCard';
import categoryProductService from '../services/categoryProductService';
import productService from '../services/productService';
import { getMediaUrl } from '../utils/mediaUrl';

// Các khoảng giá lọc mẫu
const priceFilters = [
  { id: 'p1', label: "Dưới 200.000đ", min: 0, max: 200000 },
  { id: 'p2', label: "200.000đ - 500.000đ", min: 200000, max: 500000 },
  { id: 'p3', label: "500.000đ - 1.000.000đ", min: 500000, max: 1000000 },
];

// Helper to convert flat category array into hierarchical tree
const buildCategoryTree = (categories) => {
  if (!categories || !Array.isArray(categories)) return [];
  const map = {};
  const tree = [];

  categories.forEach(cat => {
    map[cat.id] = { ...cat, children: [] };
  });

  categories.forEach(cat => {
    const parentId = cat.parentId !== undefined ? cat.parentId : cat.ParentId;
    if (parentId !== null && parentId !== undefined && map[parentId]) {
      map[parentId].children.push(map[cat.id]);
    } else {
      tree.push(map[cat.id]);
    }
  });

  return tree;
};

const Shop = () => {
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [filteredProducts, setFilteredProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  // Search parameters từ URL
  const [searchParams, setSearchParams] = useSearchParams();
  const urlCategory = searchParams.get('category');
  const urlSearch = searchParams.get('search');

  // Lọc theo Category & Expandable sidebar
  const [selectedCategoryId, setSelectedCategoryId] = useState(null);
  const [expandedCategoryIds, setExpandedCategoryIds] = useState([]);

  // Lọc theo Giá
  const [selectedPriceFilterId, setSelectedPriceFilterId] = useState(null);
  const [minPriceInput, setMinPriceInput] = useState('');
  const [maxPriceInput, setMaxPriceInput] = useState('');
  const [minPrice, setMinPrice] = useState(null);
  const [maxPrice, setMaxPrice] = useState(null);

  // Sắp xếp và giao diện
  const [sortOption, setSortOption] = useState('Mặc định');
  const [viewMode, setViewMode] = useState('grid');

  // Phân trang
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 6; // Hiển thị 6 sản phẩm/trang để dễ thử nghiệm phân trang

  // Đồng bộ category từ URL vào state
  useEffect(() => {
    if (urlCategory) {
      setSelectedCategoryId(parseInt(urlCategory));
    } else {
      setSelectedCategoryId(null);
    }
    setCurrentPage(1);
  }, [urlCategory]);

  // Đồng bộ từ khóa tìm kiếm khi thay đổi URL
  useEffect(() => {
    setCurrentPage(1);
  }, [urlSearch]);

  // 1. Tải danh mục một lần khi mount
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const apiCategories = await categoryProductService.getAllCategoryProducts();
        if (apiCategories) {
          setCategories(apiCategories);
        }
      } catch (error) {
        console.error(">>> Lỗi tải danh mục:", error.message);
      }
    };
    loadCategories();
  }, []);

  // 2. Tải sản phẩm từ Backend API (Lọc ngầm từ cơ sở dữ liệu)
  useEffect(() => {
    const loadFilteredData = async () => {
      try {
        setLoading(true);
        const params = {};
        if (urlSearch) params.search = urlSearch;
        if (selectedCategoryId) params.categoryId = selectedCategoryId;
        if (minPrice !== null) params.minPrice = minPrice;
        if (maxPrice !== null) params.maxPrice = maxPrice;

        const apiProducts = await productService.getAllProducts(params);

        if (apiProducts) {
          const mappedProducts = apiProducts.map(p => ({
            id: p.id,
            name: p.name,
            price: p.price,
            originalPrice: p.price,
            salePrice: p.salePrice || 0,
            isSale: p.isSale || false,
            isNew: p.isNew || false,
            discountPercent: p.discountPercent || 0,
            brandName: p.brandName || "MYKINGDOM",
            sku: p.sku || `SKU${120000 + p.id}`,
            tagLabel: p.isNew ? 'NEW' : (p.tagLabel || (p.price > 500000 ? 'LEGO' : 'SẢN PHẨM')),
            imageUrl: p.imageUrl || "https://placehold.co/200x200/e9ecef/6c757d?text=No+Image",
            categoryId: p.categoryProductId,
            stockQuantity: p.stockQuantity || 0
          }));
          setProducts(mappedProducts);
        } else {
          setProducts([]);
        }
      } catch (error) {
        console.error(">>> Lỗi gọi API lọc sản phẩm:", error.message);
        setProducts([]);
      } finally {
        setLoading(false);
      }
    };

    loadFilteredData();
  }, [urlSearch, selectedCategoryId, minPrice, maxPrice]);

  // 3. Sắp xếp kết quả tại Client
  useEffect(() => {
    let result = [...products];

    if (sortOption === 'Giá tăng dần') {
      result.sort((a, b) => a.price - b.price);
    } else if (sortOption === 'Giá giảm dần') {
      result.sort((a, b) => b.price - a.price);
    }

    setFilteredProducts(result);
    setCurrentPage(1);
  }, [products, sortOption]);

  // Tự động mở rộng danh mục cha nếu danh mục con được chọn
  useEffect(() => {
    if (selectedCategoryId && categories.length > 0) {
      const selectedCat = categories.find(c => c.id === selectedCategoryId);
      if (selectedCat) {
        const parentId = selectedCat.parentId !== undefined ? selectedCat.parentId : selectedCat.ParentId;
        if (parentId) {
          setExpandedCategoryIds(prev => 
            prev.includes(parentId) ? prev : [...prev, parentId]
          );
        }
      }
    }
  }, [selectedCategoryId, categories]);

  // Click chọn danh mục cha ở sidebar
  const handleParentClick = (catId, hasChildren) => {
    handleCategoryClick(catId);
    if (hasChildren) {
      setExpandedCategoryIds(prev =>
        prev.includes(catId) ? prev.filter(id => id !== catId) : [...prev, catId]
      );
    }
  };

  // Click chọn danh mục ở sidebar
  const handleCategoryClick = (catId) => {
    const params = {};
    if (urlSearch) params.search = urlSearch;
    if (catId) params.category = catId.toString();
    setSearchParams(params);
  };

  // Click chọn checkbox khoảng giá
  const handleCheckboxChange = (filter) => {
    if (selectedPriceFilterId === filter.id) {
      setSelectedPriceFilterId(null);
      setMinPrice(null);
      setMaxPrice(null);
      setMinPriceInput('');
      setMaxPriceInput('');
    } else {
      setSelectedPriceFilterId(filter.id);
      setMinPrice(filter.min);
      setMaxPrice(filter.max);
      setMinPriceInput(filter.min.toString());
      setMaxPriceInput(filter.max.toString());
    }
    setCurrentPage(1);
  };

  // Áp dụng khoảng giá tự nhập
  const handleApplyPriceRange = () => {
    const min = minPriceInput ? parseFloat(minPriceInput) : null;
    const max = maxPriceInput ? parseFloat(maxPriceInput) : null;
    setMinPrice(min);
    setMaxPrice(max);
    setSelectedPriceFilterId(null); // Bỏ chọn checkbox mẫu
    setCurrentPage(1);
  };

  // Xóa bộ lọc giá
  const handleClearPriceRange = () => {
    setMinPriceInput('');
    setMaxPriceInput('');
    setMinPrice(null);
    setMaxPrice(null);
    setSelectedPriceFilterId(null);
    setCurrentPage(1);
  };

  // Hàm xử lý đặt lại (reset) toàn bộ các bộ lọc và từ khóa tìm kiếm về mặc định
  const handleResetFilters = () => {
    setSelectedCategoryId(null); // Xóa lọc theo danh mục
    setSelectedPriceFilterId(null); // Xóa lọc theo khoảng giá checkbox
    setMinPrice(null); // Xóa lọc giá tối thiểu
    setMaxPrice(null); // Xóa lọc giá tối đa
    setMinPriceInput(''); // Reset ô nhập giá tối thiểu
    setMaxPriceInput(''); // Reset ô nhập giá tối đa
    setSearchParams({}); // Xóa sạch các tham số truy vấn (search, category...) trên URL
    setCurrentPage(1); // Trở lại trang đầu tiên
  };

  // Tính toán phân trang
  const totalPages = Math.ceil(filteredProducts.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const paginatedProducts = filteredProducts.slice(startIndex, startIndex + pageSize);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary font-weight-bold">Đang kết nối API lọc sản phẩm...</p>
      </div>
    );
  }

  return (
    <div className="shop-page-container py-4" style={{ backgroundColor: '#ffffff', minHeight: '100vh' }}>
      <div className="container">
        {/* Breadcrumb */}
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
            <li className="breadcrumb-item"><Link to="/" className="text-secondary text-decoration-none">Trang chủ</Link></li>
            <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tất cả sản phẩm</li>
          </ol>
        </nav>

        {urlSearch && (
          <div className="alert alert-info py-2" role="alert" style={{ fontSize: '0.9rem' }}>
            Kết quả tìm kiếm cho từ khóa: <strong>"{urlSearch}"</strong>
          </div>
        )}

        <div className="row">
          {/* SIDEBAR BỘ LỌC */}
          <div className="col-12 col-md-3 mb-4">
            {/* DANH MỤC */}
            <div className="card border-0 mb-4 shadow-sm p-3 rounded-4">
              <div className="d-flex align-items-center justify-content-between border-bottom pb-2 mb-3">
                <h5 className="font-weight-bold text-dark text-uppercase mb-0" style={{ fontSize: '0.95rem', letterSpacing: '0.5px' }}>
                  Danh Mục
                </h5>
                <i className="fa-solid fa-chevron-down text-dark" style={{ fontSize: '0.8rem' }}></i>
              </div>

              <div className="list-group list-group-flush border-0">
                <button
                  type="button"
                  className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center border-0 px-0 py-2 font-weight-bold ${
                    selectedCategoryId === null ? 'text-danger' : 'text-secondary'
                  }`}
                  onClick={() => handleCategoryClick(null)}
                  style={{ fontSize: '0.9rem', backgroundColor: 'transparent' }}
                >
                  <span>Tất cả danh mục</span>
                </button>

                {buildCategoryTree(categories).map((parentCat) => {
                  const isParentSelected = selectedCategoryId === parentCat.id;
                  const hasChildren = parentCat.children && parentCat.children.length > 0;
                  const isExpanded = expandedCategoryIds.includes(parentCat.id);
                  
                  // Check if any child of this parent is selected
                  const isAnyChildSelected = hasChildren && parentCat.children.some(child => child.id === selectedCategoryId);

                  return (
                    <div key={parentCat.id} className="w-100">
                      {/* Parent category */}
                      <button
                        type="button"
                        className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center border-0 px-0 py-2 transition-all ${
                          isParentSelected || isAnyChildSelected ? 'text-danger font-weight-bold' : 'text-secondary'
                        }`}
                        onClick={() => handleParentClick(parentCat.id, hasChildren)}
                        style={{ fontSize: '0.9rem', backgroundColor: 'transparent', outline: 'none', shadow: 'none' }}
                      >
                        <span>{parentCat.name}</span>
                        {hasChildren && (
                          <i 
                            className={`fa-solid ${isExpanded ? 'fa-chevron-down' : 'fa-chevron-right'} small ${
                              isParentSelected || isAnyChildSelected ? 'text-danger' : 'text-muted'
                            } opacity-75`} 
                            style={{ fontSize: '0.75rem', transition: 'transform 0.2s' }}
                          ></i>
                        )}
                      </button>

                      {/* Sub-categories */}
                      {hasChildren && isExpanded && (
                        <div className="pl-3 py-1 d-flex flex-column gap-2" style={{ borderLeft: '1px solid #e9ecef', marginLeft: '6px' }}>
                          {parentCat.children.map((childCat) => {
                            const isChildSelected = selectedCategoryId === childCat.id;
                            return (
                              <div 
                                key={childCat.id} 
                                className="d-flex align-items-center py-1"
                                onClick={() => handleCategoryClick(childCat.id)}
                                style={{ cursor: 'pointer' }}
                              >
                                <input 
                                  type="checkbox" 
                                  checked={isChildSelected}
                                  onChange={() => {}} 
                                  className="mr-2" 
                                  style={{ 
                                    accentColor: '#CF102D',
                                    width: '15px',
                                    height: '15px',
                                    cursor: 'pointer'
                                  }}
                                />
                                <span className={`small ${isChildSelected ? 'text-danger font-weight-bold' : 'text-secondary'}`} style={{ fontSize: '0.85rem' }}>
                                  {childCat.name}
                                </span>
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>

            {/* BỘ LỌC GIÁ */}
            <div className="card border-0 shadow-sm p-3 rounded-4">
              <div className="d-flex align-items-center justify-content-between border-bottom pb-2 mb-3">
                <h5 className="font-weight-bold text-dark text-uppercase mb-0" style={{ fontSize: '0.95rem', letterSpacing: '0.5px' }}>
                  Lọc theo giá
                </h5>
                <i className="fa-solid fa-chevron-down text-dark" style={{ fontSize: '0.8rem' }}></i>
              </div>

              {/* Các khoảng giá có sẵn */}
              <div className="d-flex flex-column gap-2 mb-3">
                {priceFilters.map((filter) => {
                  const isChecked = selectedPriceFilterId === filter.id;
                  return (
                    <div 
                      key={filter.id} 
                      className="d-flex align-items-center py-1" 
                      onClick={() => handleCheckboxChange(filter)}
                      style={{ cursor: 'pointer' }}
                    >
                      <input 
                        type="checkbox" 
                        checked={isChecked}
                        onChange={() => {}} 
                        className="mr-2" 
                        style={{ 
                          accentColor: '#CF102D',
                          width: '16px',
                          height: '16px',
                          cursor: 'pointer'
                        }}
                      />
                      <span className="text-secondary small" style={{ fontSize: '0.85rem' }}>
                        {filter.label}
                      </span>
                    </div>
                  );
                })}
              </div>

              {/* Ô nhập Min - Max */}
              <div className="border-top pt-3">
                <h6 className="font-weight-bold text-dark mb-2" style={{ fontSize: '0.85rem' }}>
                  Tự nhập khoảng giá (đ)
                </h6>
                <div className="d-flex align-items-center gap-1 mb-2">
                  <input 
                    type="number" 
                    className="form-control form-control-sm px-2" 
                    placeholder="Min" 
                    value={minPriceInput}
                    onChange={(e) => setMinPriceInput(e.target.value)}
                    style={{ borderRadius: '6px', fontSize: '0.8rem', height: '32px' }}
                  />
                  <span className="text-muted px-1">-</span>
                  <input 
                    type="number" 
                    className="form-control form-control-sm px-2" 
                    placeholder="Max" 
                    value={maxPriceInput}
                    onChange={(e) => setMaxPriceInput(e.target.value)}
                    style={{ borderRadius: '6px', fontSize: '0.8rem', height: '32px' }}
                  />
                </div>
                <div className="d-flex gap-2">
                  <button 
                    type="button" 
                    className="btn btn-sm btn-danger w-100 font-weight-bold py-1" 
                    onClick={handleApplyPriceRange}
                    style={{ borderRadius: '15px', backgroundColor: '#CF102D', fontSize: '0.75rem', height: '30px' }}
                  >
                    Áp dụng
                  </button>
                  {(minPrice !== null || maxPrice !== null) && (
                    <button 
                      type="button" 
                      className="btn btn-sm btn-outline-secondary w-100 font-weight-bold py-1" 
                      onClick={handleClearPriceRange}
                      style={{ borderRadius: '15px', fontSize: '0.75rem', height: '30px' }}
                    >
                      Xóa lọc
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* LƯỚI SẢN PHẨM */}
          <div className="col-12 col-md-9">
            {/* Toolbar */}
            <div className="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom flex-wrap gap-3">
              <div className="d-flex align-items-center gap-3">
                <span className="text-muted small mr-2">Kiểu xem:</span>
                <button 
                  onClick={() => setViewMode('list')}
                  className={`btn btn-sm p-1 border-0 mr-1 ${viewMode === 'list' ? 'text-danger' : 'text-muted'}`}
                  style={{ backgroundColor: 'transparent' }}
                >
                  <i className="fa-solid fa-list fs-5"></i>
                </button>
                <button 
                  onClick={() => setViewMode('grid')}
                  className={`btn btn-sm p-1 border-0 mr-3 ${viewMode === 'grid' ? 'text-danger' : 'text-muted'}`}
                  style={{ backgroundColor: 'transparent' }}
                >
                  <i className="fa-solid fa-table-cells-large fs-5"></i>
                </button>
                
                <span className="text-muted small" style={{ fontSize: '0.85rem' }}>
                  Hiển thị <strong>{filteredProducts.length}</strong> sản phẩm
                </span>
              </div>

              <div className="d-flex align-items-center" style={{ fontSize: '0.85rem' }}>
                <span className="text-muted mr-2">Sắp xếp theo:</span>
                <select 
                  value={sortOption}
                  onChange={(e) => setSortOption(e.target.value)}
                  className="form-control form-control-sm shadow-none rounded-pill px-3" 
                  style={{ 
                    width: '140px',
                    borderColor: '#dddddd',
                    height: '32px',
                    cursor: 'pointer'
                  }}
                >
                  <option>Mặc định</option>
                  <option>Giá tăng dần</option>
                  <option>Giá giảm dần</option>
                </select>
              </div>
            </div>

            {/* Danh sách */}
            <div className="row">
              {filteredProducts.length === 0 ? (
                <div className="col-12 text-center py-5 border rounded-4 bg-light my-3 shadow-sm d-flex flex-column align-items-center" style={{ borderStyle: 'dashed' }}>
                  <svg width="150" height="150" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ marginBottom: '1.5rem', animation: 'floatEmptyBox 3s ease-in-out infinite' }}>
                    <style>{`
                      @keyframes floatEmptyBox {
                        0% { transform: translateY(0px) rotate(0deg); }
                        50% { transform: translateY(-10px) rotate(2deg); }
                        100% { transform: translateY(0px) rotate(0deg); }
                      }
                    `}</style>
                    <rect x="8" y="22" width="48" height="34" rx="8" fill="#F8F9FA" stroke="#D1D5DB" strokeWidth="2.5" />
                    <path d="M12 22L32 36L52 22" stroke="#D1D5DB" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" />
                    <circle cx="32" cy="40" r="7" stroke="#CF102D" strokeWidth="2.5" strokeDasharray="3 3" fill="none" />
                    <path d="M37 45L43 51" stroke="#CF102D" strokeWidth="2.5" strokeLinecap="round" />
                    <path d="M18 14H46" stroke="#E5E7EB" strokeWidth="3" strokeLinecap="round" />
                  </svg>
                  <h6 className="font-weight-bold fs-5" style={{ color: '#002060' }}>Không tìm thấy sản phẩm nào phù hợp với bộ lọc</h6>
                  <p className="small text-muted mb-3">Vui lòng điều chỉnh lại bộ lọc giá, xóa từ khóa tìm kiếm hoặc chọn danh mục khác.</p>
                  <button 
                    onClick={handleResetFilters}
                    className="btn btn-danger font-weight-bold px-4 py-2 rounded-pill shadow-sm transition-all"
                    style={{ backgroundColor: '#CF102D', border: 'none', fontSize: '0.85rem' }}
                  >
                    <i className="fa-solid fa-rotate-left mr-2"></i> Đặt lại tất cả bộ lọc
                  </button>
                </div>
              ) : (
                paginatedProducts.map((item) => (
                  <div 
                    className={`mb-4 transition-all ${
                      viewMode === 'grid' ? 'col-12 col-sm-6 col-md-4' : 'col-12'
                    }`} 
                    key={item.id}
                  >
                    {viewMode === 'grid' ? (
                      <ProductCard item={item} />
                    ) : (
                      /* List view */
                      <div className="card h-100 shadow-sm border border-light rounded-4 overflow-hidden d-flex flex-row p-3">
                        <div className="d-flex align-items-center justify-content-center bg-white rounded" style={{ width: '150px', height: '150px', flexShrink: 0 }}>
                          <img src={getMediaUrl(item.imageUrl, "https://placehold.co/200x200/e9ecef/6c757d?text=No+Image")} alt={item.name} className="img-fluid" style={{ maxHeight: '130px', objectFit: 'contain' }} />
                        </div>
                        <div className="pl-4 d-flex flex-column justify-content-between flex-grow-1">
                          <div>
                            <div className="d-flex justify-content-between">
                              <span className="badge text-white px-2 py-1" style={{ backgroundColor: '#002664', fontSize: '0.7rem', textTransform: 'uppercase' }}>{item.tagLabel}</span>
                              <span className="text-muted small">SKU: {item.sku}</span>
                            </div>
                            <h6 className="font-weight-bold mt-2" style={{ color: '#002664', fontSize: '0.95rem' }}>{item.name}</h6>
                            <span className="text-muted small d-block">Tồn kho: {item.stockQuantity} sản phẩm</span>
                          </div>
                          <div className="d-flex align-items-center justify-content-between mt-3 flex-wrap">
                            <div className="d-flex align-items-baseline">
                              <span className="text-danger font-weight-bold fs-5 mr-3" style={{ color: '#CF102D', fontSize: '1.1rem' }}>
                                {new Intl.NumberFormat('vi-VN').format(item.price)}₫
                              </span>
                            </div>
                            <button 
                              onClick={() => {
                                const cart = JSON.parse(localStorage.getItem('cart') || '[]');
                                const index = cart.findIndex(c => c.id === item.id);
                                const currentQtyInCart = index > -1 ? cart[index].quantity : 0;

                                if (currentQtyInCart + 1 > item.stockQuantity) {
                                  alert(`Số lượng sản phẩm trong kho không đủ! (Tồn kho: ${item.stockQuantity})`);
                                  return;
                                }

                                if (index > -1) {
                                  cart[index].quantity += 1;
                                } else {
                                  cart.push({
                                    id: item.id,
                                    name: item.name,
                                    price: item.price,
                                    quantity: 1,
                                    imageUrl: getMediaUrl(item.imageUrl, "https://placehold.co/200x200/e9ecef/6c757d?text=No+Image"),
                                    sku: item.sku || `SKU${120000 + item.id}`
                                  });
                                }
                                localStorage.setItem('cart', JSON.stringify(cart));
                                window.dispatchEvent(new Event('cartChange'));
                                alert(`Đã thêm "${item.name}" vào giỏ hàng thành công!`);
                              }}
                              className="btn btn-sm font-weight-bold text-white px-4 py-2" 
                              style={{ backgroundColor: '#CF102D', borderRadius: '25px' }}
                            >
                              Thêm Vào Giỏ Hàng
                            </button>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                ))
              )}
            </div>

            {/* Phân trang */}
            {totalPages > 1 && (
              <nav aria-label="Page navigation" className="mt-5">
                <ul className="pagination justify-content-center gap-1">
                  <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}>
                    <button 
                      className="page-link shadow-none border-0 font-weight-bold" 
                      onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                      style={{ color: '#CF102D', backgroundColor: 'transparent' }}
                    >
                      <i className="fa-solid fa-angles-left"></i> Trước
                    </button>
                  </li>
                  {[...Array(totalPages).keys()].map(page => (
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
                          backgroundColor: currentPage === page + 1 ? '#CF102D' : '#f8f9fa',
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
                      onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                      style={{ color: '#CF102D', backgroundColor: 'transparent' }}
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

export default Shop;
