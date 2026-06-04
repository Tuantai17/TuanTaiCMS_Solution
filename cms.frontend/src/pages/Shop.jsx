import React, { useState, useEffect } from 'react';
import ProductCard from '../components/ProductCard';
import categoryProductService from '../services/categoryProductService';
import productService from '../services/productService';

// 12 Sản phẩm demo chất lượng cao dùng làm dữ liệu dự phòng (Fallback) khi API chưa khởi động
const demoProducts = [
  // Nhóm 1: Đồ chơi lắp ghép (3 sản phẩm Lego)
  {
    id: 101,
    name: "LEGO Classic 10696 - Thùng Gạch Trung Sáng Tạo Cao Cấp",
    price: 799000,
    originalPrice: 999000,
    discountPercent: 20,
    brandName: "LEGO",
    sku: "10696",
    tagLabel: "LEGO",
    imageUrl: "https://images.unsplash.com/photo-1587654780291-39c9404d746b?w=500&auto=format&fit=crop&q=80",
    categoryId: 3,
    categoryName: "Đồ chơi lắp ghép"
  },
  {
    id: 102,
    name: "LEGO City 60312 - Xe Cảnh Sát Đuổi Bắt Tốc Độ Cao",
    price: 279000,
    originalPrice: 399000,
    discountPercent: 30,
    brandName: "LEGO",
    sku: "60312",
    tagLabel: "LEGO",
    imageUrl: "https://images.unsplash.com/photo-1560169897-fc0cdbdfa4d5?w=500&auto=format&fit=crop&q=80",
    categoryId: 3,
    categoryName: "Đồ chơi lắp ghép"
  },
  {
    id: 103,
    name: "LEGO Creator 31058 - Khủng Long Gầm Vang 3 Trong 1",
    price: 399000,
    originalPrice: 499000,
    discountPercent: 20,
    brandName: "LEGO",
    sku: "31058",
    tagLabel: "LEGO",
    imageUrl: "https://images.unsplash.com/photo-1558060370-d644479cb6f7?w=500&auto=format&fit=crop&q=80",
    categoryId: 3,
    categoryName: "Đồ chơi lắp ghép"
  },

  // Nhóm 2: Đồ chơi sáng tạo (3 sản phẩm Yoyo, Beyblade)
  {
    id: 201,
    name: "Con Quay B-180 Booster Dynamite Belial.Nx.Vn-2 BEYBLADE 6173670",
    price: 189500,
    originalPrice: 379000,
    discountPercent: 50,
    brandName: "BEYBLADE 6",
    sku: "173670",
    tagLabel: "OUTLET",
    imageUrl: "https://images.unsplash.com/photo-1596461404969-9ae70f2830c1?w=500&auto=format&fit=crop&q=80",
    categoryId: 4,
    categoryName: "Đồ chơi sáng tạo"
  },
  {
    id: 202,
    name: "Con Quay B-192 Booster Greatest Raphael.Ov.HXt+ BEYBLADE 6173779",
    price: 229500,
    originalPrice: 459000,
    discountPercent: 50,
    brandName: "BEYBLADE 6",
    sku: "173779",
    tagLabel: "OUTLET",
    imageUrl: "https://images.unsplash.com/photo-1515488042361-404e9250afef?w=500&auto=format&fit=crop&q=80",
    categoryId: 4,
    categoryName: "Đồ chơi sáng tạo"
  },
  {
    id: 203,
    name: "Yoyo Chiến Binh Huyền Thoại YOYO 22 EU677118R Cực Đỉnh",
    price: 47800,
    originalPrice: 239000,
    discountPercent: 80,
    brandName: "YOYO 22",
    sku: "EU677118R",
    tagLabel: "OUTLET",
    imageUrl: "https://images.unsplash.com/photo-1531256379416-9f000e90aacc?w=500&auto=format&fit=crop&q=80",
    categoryId: 4,
    categoryName: "Đồ chơi sáng tạo"
  },

  // Nhóm 3: Đồ thời trang (3 sản phẩm Clever Hippo)
  {
    id: 301,
    name: "Ba Lô Chống Gù Clever Hippo Easy Go Dino - Khủng Long Xanh",
    price: 499000,
    originalPrice: 799000,
    discountPercent: 37,
    brandName: "CLEVER HIPPO",
    sku: "EG1101",
    tagLabel: "NEW",
    imageUrl: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500&auto=format&fit=crop&q=80",
    categoryId: 5,
    categoryName: "Đồ thời trang"
  },
  {
    id: 302,
    name: "Ba Lô Học Đường Siêu Nhẹ Clever Hippo Fancy Unicorn Hồng",
    price: 599000,
    originalPrice: 899000,
    discountPercent: 33,
    brandName: "CLEVER HIPPO",
    sku: "FC1202",
    tagLabel: "TRENDING",
    imageUrl: "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3?w=500&auto=format&fit=crop&q=80",
    categoryId: 5,
    categoryName: "Đồ thời trang"
  },
  {
    id: 303,
    name: "Bình Nước Thể Thao Clever Hippo Active Cách Nhiệt Tốt",
    price: 149000,
    originalPrice: 199000,
    discountPercent: 25,
    brandName: "CLEVER HIPPO",
    sku: "AT1303",
    tagLabel: "SALE",
    imageUrl: "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=500&auto=format&fit=crop&q=80",
    categoryId: 5,
    categoryName: "Đồ thời trang"
  },

  // Nhóm 4: Thế giới động vật (3 mô hình Schleich Đức)
  {
    id: 401,
    name: "Mô Hình Khủng Long Bạo Chúa T-Rex Schleich Độc Quyền Đức",
    price: 299000,
    originalPrice: 399000,
    discountPercent: 25,
    brandName: "SCHLEICH",
    sku: "14525",
    tagLabel: "ANIMAL",
    imageUrl: "https://images.unsplash.com/photo-1525869916826-972885c91c1e?w=500&auto=format&fit=crop&q=80",
    categoryId: 6,
    categoryName: "Thế giới động vật"
  },
  {
    id: 402,
    name: "Mô Hình Voi Châu Á Trưởng Thành Schleich Sống Động",
    price: 199000,
    originalPrice: 249000,
    discountPercent: 20,
    brandName: "SCHLEICH",
    sku: "14754",
    tagLabel: "ANIMAL",
    imageUrl: "https://images.unsplash.com/photo-1581888227599-779811939961?w=500&auto=format&fit=crop&q=80",
    categoryId: 6,
    categoryName: "Thế giới động vật"
  },
  {
    id: 403,
    name: "Mô Hình Sư Tử Đực Dũng Mãnh Schleich Chi Tiết Sắc Nét",
    price: 149000,
    originalPrice: 199000,
    discountPercent: 25,
    brandName: "SCHLEICH",
    sku: "14812",
    tagLabel: "ANIMAL",
    imageUrl: "https://images.unsplash.com/photo-1614027164847-1b2809eb7b9b?w=500&auto=format&fit=crop&q=80",
    categoryId: 6,
    categoryName: "Thế giới động vật"
  }
];

// Danh sách danh mục tĩnh dùng làm Fallback
const staticCategories = [
  { id: 3, name: "Đồ chơi lắp ghép", count: 3, hasChevron: true },
  { id: 4, name: "Đồ chơi sáng tạo", count: 3, hasChevron: true },
  { id: 5, name: "Đồ thời trang", count: 3, hasChevron: true },
  { id: 6, name: "Thế giới động vật", count: 3, hasChevron: true }
];

// Các khoảng giá lọc
const priceFilters = [
  { id: 'p1', label: "Dưới 200.000đ", count: 5, min: 0, max: 200000 },
  { id: 'p2', label: "200.000đ - 500.000đ", count: 5, min: 200000, max: 500000 },
  { id: 'p3', label: "500.000đ - 1.000.000đ", count: 2, min: 500000, max: 1000000 }
];

const Shop = () => {
  const [categories, setCategories] = useState(staticCategories);
  const [products, setProducts] = useState(demoProducts);
  const [selectedCategoryId, setSelectedCategoryId] = useState(null);
  const [selectedPriceFilterId, setSelectedPriceFilterId] = useState(null);
  const [sortOption, setSortOption] = useState('Mặc định');
  const [viewMode, setViewMode] = useState('grid');
  const [filteredProducts, setFilteredProducts] = useState(demoProducts);
  const [loading, setLoading] = useState(true);

  // Gọi API lấy dữ liệu thực tế từ Database SQL Server (Real-time Connection)
  useEffect(() => {
    const loadRealData = async () => {
      try {
        setLoading(true);
        // 1. Tải danh mục động từ DB
        const apiCategories = await categoryProductService.getAllCategoryProducts();
        
        // 2. Tải sản phẩm động từ DB
        const apiProducts = await productService.getAllProducts();
        
        if (apiCategories && apiCategories.length > 0) {
          // Chỉ lấy các danh mục đồ chơi
          const toyCategories = apiCategories.filter(c => 
            c.name.includes("lắp ghép") || 
            c.name.includes("sáng tạo") || 
            c.name.includes("thời trang") || 
            c.name.includes("động vật")
          );
          
          if (toyCategories.length > 0) {
            setCategories(toyCategories.map(c => ({
              id: c.id,
              name: c.name,
              count: 3,
              hasChevron: true
            })));
          } else {
            setCategories(staticCategories);
          }
        }

        if (apiProducts && apiProducts.length > 0) {
          // Bản đồ hóa dữ liệu từ database, tự động bù đắp các nhãn MyKingdom cao cấp
          const mappedProducts = apiProducts.map(p => {
            // Tìm sản phẩm demo tương đương dựa trên tên để gán hình ảnh đẹp sắc nét
            const demoEquivalent = demoProducts.find(dp => 
              p.name.toLowerCase().includes(dp.name.toLowerCase().substring(0, 10))
            );

            const discount = demoEquivalent ? demoEquivalent.discountPercent : 25;
            return {
              id: p.id,
              name: p.name,
              price: p.price,
              originalPrice: demoEquivalent ? demoEquivalent.originalPrice : Math.round(p.price / (1 - discount / 100)),
              discountPercent: discount,
              brandName: demoEquivalent ? demoEquivalent.brandName : "MYKINGDOM",
              sku: demoEquivalent ? demoEquivalent.sku : `SKU${120000 + p.id}`,
              tagLabel: demoEquivalent ? demoEquivalent.tagLabel : (p.id % 2 === 0 ? 'OUTLET' : 'LEGO'),
              imageUrl: p.imageUrl || (demoEquivalent ? demoEquivalent.imageUrl : "https://placehold.co/200x200/e9ecef/6c757d?text=No+Image"),
              categoryId: p.categoryProductId,
              categoryName: p.categoryProduct ? p.categoryProduct.name : ""
            };
          });
          setProducts(mappedProducts);
        } else {
          setProducts(demoProducts);
        }
      } catch (error) {
        console.log(">>> Hệ thống đang chạy ở chế độ Demo dự phòng (Fallback):", error.message);
        setCategories(staticCategories);
        setProducts(demoProducts);
      } finally {
        setLoading(false);
      }
    };

    loadRealData();
  }, []);

  // Xử lý bộ lọc sản phẩm real-time
  useEffect(() => {
    let result = [...products];

    // Lọc theo danh mục
    if (selectedCategoryId !== null) {
      result = result.filter(p => p.categoryId === selectedCategoryId);
    }

    // Lọc theo khoảng giá
    if (selectedPriceFilterId !== null) {
      const activeFilter = priceFilters.find(f => f.id === selectedPriceFilterId);
      if (activeFilter) {
        result = result.filter(p => p.price >= activeFilter.min && p.price <= activeFilter.max);
      }
    }

    // Sắp xếp sản phẩm
    if (sortOption === 'Giá tăng dần') {
      result.sort((a, b) => a.price - b.price);
    } else if (sortOption === 'Giá giảm dần') {
      result.sort((a, b) => b.price - a.price);
    }

    setFilteredProducts(result);
  }, [selectedCategoryId, selectedPriceFilterId, sortOption, products]);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary font-weight-bold">Đang tải không gian đồ chơi MyKingdom động...</p>
      </div>
    );
  }

  return (
    <div className="shop-page-container py-4" style={{ backgroundColor: '#ffffff', minHeight: '100vh' }}>
      <div className="container">
        {/* Đường dẫn Breadcrumb thanh lịch */}
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
            <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
            <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Tất cả sản phẩm</li>
          </ol>
        </nav>

        <div className="row">
          {/* SIDEBAR BÊN TRÁI - BỘ LỌC CHUẨN MYKINGDOM */}
          <div className="col-12 col-md-3 mb-4">
            {/* 1. KHỐI DANH MỤC */}
            <div className="card border-0 mb-4">
              <div className="d-flex align-items-center justify-content-between border-bottom pb-2 mb-3 cursor-pointer" style={{ cursor: 'pointer' }}>
                <h5 className="font-weight-bold text-dark text-uppercase mb-0" style={{ fontSize: '1rem', letterSpacing: '0.5px' }}>
                  Danh Mục
                </h5>
                <i className="fa-solid fa-chevron-down text-dark" style={{ fontSize: '0.9rem' }}></i>
              </div>

              <div className="list-group list-group-flush border-0">
                {/* Nút hiển thị Tất cả */}
                <button
                  type="button"
                  className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center border-0 px-0 py-2 font-weight-bold ${
                    selectedCategoryId === null ? 'text-danger' : 'text-secondary'
                  }`}
                  onClick={() => setSelectedCategoryId(null)}
                  style={{ fontSize: '0.9rem', backgroundColor: 'transparent' }}
                >
                  <span>Tất cả sản phẩm ({products.length})</span>
                  <i className="fa-solid fa-chevron-right small text-muted opacity-50" style={{ fontSize: '0.7rem' }}></i>
                </button>

                {/* Danh sách danh mục lọc */}
                {categories.map((cat) => {
                  const isSelected = selectedCategoryId === cat.id;
                  return (
                    <button
                      key={cat.id}
                      type="button"
                      className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center border-0 px-0 py-2 transition-all ${
                        isSelected ? 'text-danger font-weight-bold' : 'text-secondary'
                      }`}
                      onClick={() => setSelectedCategoryId(cat.id)}
                      style={{ fontSize: '0.9rem', backgroundColor: 'transparent' }}
                    >
                      <span>
                        {cat.name} <span className="text-muted ml-1" style={{ fontSize: '0.85rem', opacity: 0.7 }}>({cat.count})</span>
                      </span>
                      {cat.hasChevron && (
                        <i className={`fa-solid fa-chevron-right small ${isSelected ? 'text-danger' : 'text-muted'} opacity-50`} style={{ fontSize: '0.7rem' }}></i>
                      )}
                    </button>
                  );
                })}
              </div>
            </div>

            {/* 2. KHỐI LỌC GIÁ */}
            <div className="card border-0">
              <div className="d-flex align-items-center justify-content-between border-bottom pb-2 mb-3">
                <h5 className="font-weight-bold text-dark text-uppercase mb-0" style={{ fontSize: '1rem', letterSpacing: '0.5px' }}>
                  Giá (đ)
                </h5>
                <i className="fa-solid fa-chevron-down text-dark" style={{ fontSize: '0.9rem' }}></i>
              </div>

              <div className="d-flex flex-column gap-2">
                {priceFilters.map((filter) => {
                  const isChecked = selectedPriceFilterId === filter.id;
                  return (
                    <div 
                      key={filter.id} 
                      className="d-flex align-items-center py-1 cursor-pointer" 
                      onClick={() => setSelectedPriceFilterId(isChecked ? null : filter.id)}
                      style={{ cursor: 'pointer' }}
                    >
                      <input 
                        type="checkbox" 
                        checked={isChecked}
                        onChange={() => {}} // Đã được xử lý bởi onClick của div bao ngoài
                        className="mr-2 cursor-pointer" 
                        style={{ 
                          accentColor: '#CF102D',
                          width: '16px',
                          height: '16px',
                          cursor: 'pointer'
                        }}
                      />
                      <span className="text-secondary small cursor-pointer" style={{ fontSize: '0.85rem' }}>
                        {filter.label} <span className="text-muted opacity-70">({filter.count})</span>
                      </span>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          {/* LƯỚI SẢN PHẨM BÊN PHẢI */}
          <div className="col-12 col-md-9">
            {/* Thanh Toolbar sắp xếp và chế độ xem */}
            <div className="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom flex-wrap gap-3">
              {/* Bên trái: Chế độ xem & Số lượng */}
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
                  <strong>{filteredProducts.length}</strong> products
                </span>
              </div>

              {/* Bên phải: Dropdown Sắp xếp */}
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

            {/* Hiển thị danh sách sản phẩm lọc được */}
            <div className="row">
              {filteredProducts.length === 0 ? (
                <div className="col-12 text-center py-5 border rounded-lg bg-light my-3">
                  <i className="fa-solid fa-box-open text-muted mb-3" style={{ fontSize: '3rem', opacity: 0.5 }}></i>
                  <h6 className="text-secondary font-weight-bold">Không tìm thấy sản phẩm nào!</h6>
                  <p className="small text-muted mb-0">Vui lòng điều chỉnh lại bộ lọc giá hoặc chọn danh mục khác.</p>
                </div>
              ) : (
                filteredProducts.map((item) => (
                  <div 
                    className={`mb-4 transition-all ${
                      viewMode === 'grid' ? 'col-12 col-sm-6 col-md-4' : 'col-12'
                    }`} 
                    key={item.id}
                  >
                    {viewMode === 'grid' ? (
                      <ProductCard item={item} />
                    ) : (
                      /* Kiểu hiển thị danh sách dòng (List view) */
                      <div className="card h-100 shadow-sm border border-light rounded-lg overflow-hidden d-flex flex-row p-3" style={{ borderRadius: '16px' }}>
                        <div className="d-flex align-items-center justify-content-center bg-white rounded" style={{ width: '150px', height: '150px', flexShrink: 0 }}>
                          <img src={item.imageUrl} alt={item.name} className="img-fluid" style={{ maxHeight: '130px', objectFit: 'contain' }} />
                        </div>
                        <div className="pl-4 d-flex flex-column justify-content-between flex-grow-1">
                          <div>
                            <div className="d-flex justify-content-between">
                              <span className="badge text-white px-2 py-1" style={{ backgroundColor: '#002664', fontSize: '0.7rem', textTransform: 'uppercase' }}>{item.tagLabel}</span>
                              <span className="text-muted small">SKU: {item.sku}</span>
                            </div>
                            <h6 className="font-weight-bold mt-2" style={{ color: '#002664', fontSize: '0.95rem' }}>{item.name}</h6>
                            <span className="text-uppercase text-muted font-weight-bold small d-block" style={{ fontSize: '0.7rem' }}>Thương hiệu: {item.brandName}</span>
                          </div>
                          <div className="d-flex align-items-center justify-content-between mt-3 flex-wrap">
                            <div className="d-flex align-items-baseline">
                              <span className="text-danger font-weight-bold fs-5 mr-3" style={{ color: '#CF102D', fontSize: '1.1rem' }}>
                                {new Intl.NumberFormat('vi-VN').format(item.price)} VND
                              </span>
                              <span className="text-muted text-decoration-line-through small" style={{ textDecoration: 'line-through', opacity: 0.6 }}>
                                {new Intl.NumberFormat('vi-VN').format(Math.round(item.price / (1 - item.discountPercent / 100)))} VND
                              </span>
                            </div>
                            <button 
                              onClick={() => {
                                const cart = JSON.parse(localStorage.getItem('cart') || '[]');
                                const index = cart.findIndex(c => c.id === item.id);
                                if (index > -1) {
                                  cart[index].quantity += 1;
                                } else {
                                  cart.push({
                                    id: item.id,
                                    name: item.name,
                                    price: item.price,
                                    quantity: 1,
                                    imageUrl: item.imageUrl,
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
          </div>
        </div>
      </div>
    </div>
  );
};

export default Shop;
