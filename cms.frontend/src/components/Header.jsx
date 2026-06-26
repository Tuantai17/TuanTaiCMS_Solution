import React, { useState, useEffect } from 'react';
import { Link, NavLink, useNavigate, useSearchParams } from 'react-router-dom';
import './Header.css';
import categoryProductService from '../services/categoryProductService';
import menuService from '../services/menuService';
import notificationService from '../services/notificationService';
import { getMediaUrl } from '../utils/mediaUrl';

// ==========================================
// HELPER CHUYỂN ĐỔI DỮ LIỆU PHẲNG THÀNH CÂY DANH MỤC
// ==========================================
const buildCategoryTree = (categories) => {
  if (!categories || !Array.isArray(categories)) return [];
  const map = {};
  const tree = [];

  // Tạo bản đồ các danh mục bằng ID
  categories.forEach(cat => {
    map[cat.id] = { ...cat, children: [] };
  });

  // Xây dựng cấu trúc cây dựa trên parentId / ParentId
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

const Header = () => {
  const [customer, setCustomer] = useState(null);
  const [cartCount, setCartCount] = useState(0);
  const [cartItems, setCartItems] = useState([]);
  
  // Trạng thái kết nối API Backend
  const [isApiOnline, setIsApiOnline] = useState(true);
  
  // Trạng thái mở/đóng dropdown tài khoản
  const [dropdownOpen, setDropdownOpen] = useState(false);
  
  // State Danh mục lấy từ API & Cây danh mục động
  const [apiCategories, setApiCategories] = useState([]);
  const [activeProductCat, setActiveProductCat] = useState(null);

  // State Menu động lấy từ Database (có sẵn dữ liệu mặc định để tránh Header trống khi API đang tải)
  const [menus, setMenus] = useState([
    { id: 1, title: 'Trang chủ', url: '/', order: 1, children: [] },
    { id: 2, title: 'Sản phẩm', url: '/products', order: 2, children: [] },
    { id: 3, title: 'Tin tức', url: '/blog', order: 3, children: [] }
  ]);
  // Lưu trạng thái đóng/mở (On/Off) của các menu con phụ trên Mobile Drawer
  const [openMobileSubmenus, setOpenMobileSubmenus] = useState({});

  // Hàm xử lý accordion toggle đóng/mở menu con trên thiết bị di động
  const toggleMobileSubmenu = (menuId) => {
    setOpenMobileSubmenus(prev => ({
      ...prev,
      [menuId]: !prev[menuId]
    }));
  };

  // Mobile Drawer State
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [mobileMenuLevel, setMobileMenuLevel] = useState(0); // 0: Cấp 1, 1: Cấp 2, 2: Cấp 3
  const [mobileActiveCat, setMobileActiveCat] = useState(null);   // Đối tượng danh mục con cấp 2 đang active
  const [searchQuery, setSearchQuery] = useState(''); // State từ khóa tìm kiếm
  const [searchParams] = useSearchParams();
  const urlSearch = searchParams.get('search');

  // Đồng bộ ô tìm kiếm với tham số search trên URL khi URL thay đổi hoặc tải trang
  useEffect(() => {
    setSearchQuery(urlSearch || '');
  }, [urlSearch]);

  // Cart Drawer State
  const [cartDrawerOpen, setCartDrawerOpen] = useState(false);
  const [termsAgreed, setTermsAgreed] = useState({
    privacy: false,
    payment: false,
    member: false
  });

  // Notifications State
  const [notifications, setNotifications] = useState([]);
  const [unreadNotifCount, setUnreadNotifCount] = useState(0);
  const [notificationOpen, setNotificationOpen] = useState(false);

  const navigate = useNavigate();

  // Khách hàng hiển thị thực tế (chỉ hiển thị khi API online để bảo vệ thông tin khi sập API/mất kết nối DB)
  const displayCustomer = isApiOnline ? customer : null;

  // Hàm tải dữ liệu trạng thái khách hàng & giỏ hàng từ localStorage
  const loadHeaderState = () => {
    // 1. Tải thông tin khách hàng
    const storedCustomer = localStorage.getItem('customer');
    if (storedCustomer) {
      try {
        setCustomer(JSON.parse(storedCustomer));
      } catch (e) {
        setCustomer(null);
      }
    } else {
      setCustomer(null);
    }

    // 2. Tải giỏ hàng chi tiết
    const storedCart = localStorage.getItem('cart');
    if (storedCart) {
      try {
        const items = JSON.parse(storedCart);
        setCartItems(items);
        const count = items.reduce((acc, item) => acc + item.quantity, 0);
        setCartCount(count);
      } catch (e) {
        setCartItems([]);
        setCartCount(0);
      }
    } else {
      setCartItems([]);
      setCartCount(0);
    }
  };

  // Cập nhật số lượng sản phẩm trong giỏ hàng ngay trên Cart Drawer
  const handleUpdateQty = (id, change) => {
    const updated = cartItems.map(item => {
      if (item.id === id) {
        const newQty = item.quantity + change;
        return { ...item, quantity: newQty > 0 ? newQty : 1 };
      }
      return item;
    });
    localStorage.setItem('cart', JSON.stringify(updated));
    setCartItems(updated);
    window.dispatchEvent(new Event('cartChange'));
  };

  // Xóa sản phẩm khỏi giỏ hàng
  const handleRemoveItem = (id) => {
    const updated = cartItems.filter(item => item.id !== id);
    localStorage.setItem('cart', JSON.stringify(updated));
    setCartItems(updated);
    window.dispatchEvent(new Event('cartChange'));
  };

  // Xây dựng cây danh mục từ API categories bằng useMemo để tránh tạo tham chiếu mới mỗi lần render
  const treeCategories = React.useMemo(() => buildCategoryTree(apiCategories), [apiCategories]);

  // Khai báo ref để lưu trữ giá trị mới nhất của customer và cartItems nhằm tránh stale closure
  const customerRef = React.useRef(customer);
  const cartItemsRef = React.useRef(cartItems);

  // Cập nhật ref mỗi khi state thay đổi
  useEffect(() => {
    customerRef.current = customer;
    cartItemsRef.current = cartItems;
  }, [customer, cartItems]);

  // 1. Tải danh mục sản phẩm & Menu động từ API Backend khi component mount
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const res = await categoryProductService.getAllCategoryProducts();
        setApiCategories(res);
        setIsApiOnline(true);
      } catch (err) {
        console.error("Lỗi khi tải danh mục ở Header:", err.message);
        setApiCategories([]);
        // Nếu sập API / Mất kết nối mạng / Không kết nối được DB
        if (!err.response || err.code === 'ERR_NETWORK' || (err.message && err.message.includes('Network Error'))) {
          setIsApiOnline(false);
        }
      }
    };

    // Tải danh sách cấu trúc cây Menu điều hướng động từ Database
    const loadMenus = async () => {
      try {
        const res = await menuService.getMenuHierarchy();
        if (res) {
          setMenus(res);
        }
        setIsApiOnline(true);
      } catch (err) {
        console.error("Lỗi khi tải danh sách menu ở Header:", err.message);
        if (!err.response || err.code === 'ERR_NETWORK' || (err.message && err.message.includes('Network Error'))) {
          setIsApiOnline(false);
        }
      }
    };

    loadCategories();
    loadMenus();
  }, []);

  // 2. Thiết lập activeProductCat mặc định là danh mục cha (cấp 1) đầu tiên khi vừa load xong danh mục
  useEffect(() => {
    if (treeCategories.length > 0 && !activeProductCat) {
      setActiveProductCat(treeCategories[0]);
    }
  }, [treeCategories, activeProductCat]);

  // 3. Quản lý trạng thái đăng nhập & giỏ hàng từ localStorage (Chạy 1 lần duy nhất khi mount)
  useEffect(() => {
    loadHeaderState();

    // Lắng nghe sự kiện storage khi có thay đổi từ tab khác hoặc DevTools
    const handleStorageChange = (e) => {
      if (e.key === 'customer' || e.key === 'cart' || !e.key) {
        loadHeaderState();
      }
    };
    window.addEventListener('storage', handleStorageChange);

    // Đăng ký lắng nghe các sự kiện tùy biến trong ứng dụng
    window.addEventListener('customerLoginStateChange', loadHeaderState);
    window.addEventListener('cartChange', loadHeaderState);

    // Tự động kiểm tra localStorage định kỳ mỗi 1 giây để đồng bộ lập tức khi dev xóa trong DevTools
    const intervalId = setInterval(() => {
      const storedCustomer = localStorage.getItem('customer');
      const storedCart = localStorage.getItem('cart');
      
      const currentCustStr = customerRef.current ? JSON.stringify(customerRef.current) : null;
      if (storedCustomer !== currentCustStr) {
        loadHeaderState();
      }
      
      const currentCartStr = JSON.stringify(cartItemsRef.current);
      if (storedCart !== currentCartStr) {
        loadHeaderState();
      }
    }, 1000);

    return () => {
      window.removeEventListener('storage', handleStorageChange);
      window.removeEventListener('customerLoginStateChange', loadHeaderState);
      window.removeEventListener('cartChange', loadHeaderState);
      clearInterval(intervalId);
    };
  }, []);

  // Fetch Notifications
  useEffect(() => {
    if (customer && isApiOnline) {
      const fetchNotifications = async () => {
        try {
          const res = await notificationService.getNotifications(1, 5); // get top 5
          setNotifications(res.items || []);
          const countRes = await notificationService.getUnreadCount();
          setUnreadNotifCount(countRes.count || 0);
        } catch (err) {
          console.error("Lỗi khi tải thông báo", err);
        }
      };
      fetchNotifications();
    } else {
      setNotifications([]);
      setUnreadNotifCount(0);
    }
  }, [customer, isApiOnline]);

  // Tự động đóng dropdown tài khoản/thông báo khi người dùng click ra ngoài khu vực menu
  useEffect(() => {
    const handleOutsideClick = (e) => {
      if (!e.target.closest('.dropdown') && !e.target.closest('.notification-dropdown-container')) {
        setDropdownOpen(false);
        setNotificationOpen(false);
      }
    };
    if (dropdownOpen || notificationOpen) {
      document.addEventListener('click', handleOutsideClick);
    }
    return () => {
      document.removeEventListener('click', handleOutsideClick);
    };
  }, [dropdownOpen, notificationOpen]);

  const handleLogout = () => {
    localStorage.removeItem('customer');
    setCustomer(null);
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const handleSearch = () => {
    if (searchQuery.trim()) {
      navigate(`/products?search=${encodeURIComponent(searchQuery.trim())}`);
    } else {
      navigate('/products');
    }
  };

  const handleSearchKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const toggleTerms = (key) => {
    setTermsAgreed(prev => ({
      ...prev,
      [key]: !prev[key]
    }));
  };

  const isAllTermsAgreed = termsAgreed.privacy && termsAgreed.payment && termsAgreed.member;

  return (
    <>
      <div className="sticky-header-container">
        <header className="main-header-wrapper shadow-sm">
          {/* 1. TOP BAR (Navy Blue) */}
          <div className="top-bar-navy py-2 text-white">
            <div className="container d-flex justify-content-between align-items-center flex-wrap">
              <div className="top-bar-group">
                <a href="/pages/delivery" className="top-bar-item">
                  <i className="fa-solid fa-truck-fast top-bar-icon"></i>
                  <span>Giao hàng hỏa tốc 4 tiếng</span>
                </a>
                <a href="/pages/member-benefits" className="top-bar-item">
                  <i className="fa-solid fa-users top-bar-icon"></i>
                  <span>Chương trình thành viên</span>
                </a>
              </div>
              <div className="top-bar-group">
                <a href="/pages/installment" className="top-bar-item">
                  <i className="fa-solid fa-hand-holding-dollar top-bar-icon"></i>
                  <span>Mua hàng trả góp</span>
                </a>
                <a href="/pages/stores" className="top-bar-item">
                  <i className="fa-solid fa-store top-bar-icon"></i>
                  <span>Hệ thống 200 cửa hàng</span>
                </a>
              </div>
            </div>
          </div>

          {/* 2. MAIN HEADER (Red Bar) */}
          <div className="main-header-red py-3">
            <div className="container">
              <div className="row align-items-center">
                {/* Hamburger (Mobile/Tablet) */}
                <div className="col-2 d-lg-none">
                  <button 
                    className="btn btn-link text-white p-0 shadow-none" 
                    onClick={() => { setMobileMenuOpen(true); setMobileMenuLevel(0); }}
                  >
                    <i className="fa-solid fa-bars fs-3"></i>
                  </button>
                </div>

                {/* Logo MyKingdom */}
                <div className="col-8 col-lg-3 text-center text-lg-left mb-0">
                  <Link to="/" className="d-inline-block">
                    <img 
                      src="https://www.mykingdom.com.vn/cdn/shop/files/logo-254x76_1.png?v=1697473116&width=600" 
                      alt="MyKingdom Logo" 
                      className="header-logo-img"
                    />
                  </Link>
                </div>

                {/* Search Bar custom */}
                <div className="col-12 col-lg-6 order-3 order-lg-2 mt-2 mt-lg-0">
                  <div className="search-bar-wrapper">
                    <input
                      type="text"
                      className="search-input-custom"
                      placeholder="Nhập từ khóa để tìm kiếm (ví dụ: lắp ráp, mô hình, ba lô...)"
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      onKeyDown={handleSearchKeyDown}
                    />
                    <button className="search-button-custom" aria-label="Tìm kiếm" onClick={handleSearch}>
                      <i className="fa-solid fa-magnifying-glass fs-5"></i>
                    </button>
                  </div>
                </div>

                {/* Icons: Account, Cart & Language */}
                <div className="col-2 col-lg-3 order-2 order-lg-3 header-utilities text-white">
                  {/* Account display - Desktop */}
                  <div className="d-none d-md-block">
                    {displayCustomer ? (
                      <div className="dropdown">
                        <button 
                          className="btn btn-link text-white text-decoration-none dropdown-toggle p-0 d-flex align-items-center shadow-none" 
                          type="button" 
                          id="customerDropdown" 
                          onClick={(e) => {
                            e.stopPropagation();
                            setDropdownOpen(!dropdownOpen);
                          }}
                          aria-haspopup="true" 
                          aria-expanded={dropdownOpen ? "true" : "false"}
                        >
                          <i className="fa-solid fa-user-check fs-5 mr-1"></i>
                          <span className="font-weight-bold text-truncate" style={{ maxWidth: '90px' }}>{displayCustomer.fullName}</span>
                        </button>
                        <div className={`dropdown-menu dropdown-menu-right ${dropdownOpen ? 'show' : ''}`} aria-labelledby="customerDropdown" style={{ fontSize: '0.85rem' }}>
                          <Link className="dropdown-item font-weight-bold" to="/profile" onClick={() => setDropdownOpen(false)}>
                            <i className="fa-solid fa-id-card mr-2 text-danger"></i> Hồ sơ cá nhân
                          </Link>
                          <Link className="dropdown-item font-weight-bold" to="/my-orders" onClick={() => setDropdownOpen(false)}>
                            <i className="fa-solid fa-box-open mr-2 text-danger"></i> Đơn hàng của tôi
                          </Link>
                          <div className="dropdown-divider"></div>
                          <button className="dropdown-item font-weight-bold text-danger" type="button" onClick={() => { handleLogout(); setDropdownOpen(false); }}>
                            <i className="fa-solid fa-right-from-bracket mr-2"></i> Đăng xuất
                          </button>
                        </div>
                      </div>
                    ) : (
                      <div className="d-flex align-items-center" style={{ fontSize: '0.85rem', gap: '8px' }}>
                        <Link to="/login" className="utility-link">Đăng nhập</Link>
                        <span className="text-white-50">|</span>
                        <Link to="/register" className="utility-link">Đăng ký</Link>
                      </div>
                    )}
                  </div>

                  {/* Icon Tài khoản thu gọn trên mobile */}
                  <div className="d-block d-md-none">
                    {displayCustomer ? (
                      <div className="dropdown">
                        <button 
                          className="btn btn-link text-white text-decoration-none p-0 d-flex align-items-center shadow-none" 
                          type="button" 
                          onClick={(e) => {
                            e.stopPropagation();
                            setDropdownOpen(!dropdownOpen);
                          }}
                          aria-haspopup="true" 
                          aria-expanded={dropdownOpen ? "true" : "false"}
                        >
                          <i className="fa-solid fa-user-check fs-4"></i>
                        </button>
                        <div className={`dropdown-menu dropdown-menu-right ${dropdownOpen ? 'show' : ''}`} style={{ fontSize: '0.85rem', position: 'absolute' }}>
                          <Link className="dropdown-item font-weight-bold" to="/profile" onClick={() => setDropdownOpen(false)}>
                            <i className="fa-solid fa-id-card mr-2 text-danger"></i> Hồ sơ cá nhân
                          </Link>
                          <Link className="dropdown-item font-weight-bold" to="/my-orders" onClick={() => setDropdownOpen(false)}>
                            <i className="fa-solid fa-box-open mr-2 text-danger"></i> Đơn hàng của tôi
                          </Link>
                          <div className="dropdown-divider"></div>
                          <button className="dropdown-item font-weight-bold text-danger" type="button" onClick={() => { handleLogout(); setDropdownOpen(false); }}>
                            <i className="fa-solid fa-right-from-bracket mr-2"></i> Đăng xuất
                          </button>
                        </div>
                      </div>
                    ) : (
                      <Link to="/login" className="utility-link" title="Đăng nhập">
                        <i className="fa-solid fa-user fs-4"></i>
                      </Link>
                    )}
                  </div>

                  {/* Giỏ hàng với Cart Drawer */}
                  <button 
                    className="btn btn-link text-white p-0 shadow-none cart-icon-wrapper" 
                    onClick={() => setCartDrawerOpen(true)}
                    title="Giỏ hàng"
                  >
                    <i className="fa-solid fa-bag-shopping fs-4"></i>
                    {cartCount > 0 && (
                      <span className="cart-badge">{cartCount}</span>
                    )}
                  </button>

                  {/* Notifications */}
                  <div className="notification-dropdown-container d-none d-sm-flex align-items-center ml-3" style={{ position: 'relative' }}>
                    <button 
                      className="btn btn-link text-white p-0 shadow-none position-relative" 
                      title="Thông báo"
                      onClick={(e) => {
                        e.stopPropagation();
                        if(customer) setNotificationOpen(!notificationOpen);
                        else navigate('/login');
                      }}
                    >
                      <i className="fa-regular fa-bell fs-4"></i>
                      {unreadNotifCount > 0 && (
                        <span className="cart-badge bg-warning" style={{ right: '-5px', top: '-5px' }}>{unreadNotifCount}</span>
                      )}
                    </button>

                    {/* Dropdown Menu */}
                    {notificationOpen && customer && (
                      <div className="dropdown-menu dropdown-menu-right show" style={{ position: 'absolute', top: '100%', right: 0, width: '320px', padding: 0, marginTop: '10px', boxShadow: '0 4px 12px rgba(0,0,0,0.15)', borderRadius: '8px', overflow: 'hidden' }}>
                        <div className="dropdown-header bg-light border-bottom d-flex justify-content-between align-items-center" style={{ padding: '10px 15px' }}>
                          <span className="font-weight-bold text-dark mb-0" style={{ fontSize: '1rem' }}>Thông báo mới</span>
                        </div>
                        <div className="notification-list" style={{ maxHeight: '350px', overflowY: 'auto' }}>
                          {notifications.length > 0 ? (
                            notifications.map(n => (
                              <Link 
                                to={n.referenceType === 'Order' && n.referenceId ? `/account/orders/${n.referenceId}` : '/notifications'} 
                                key={n.id} 
                                className={`dropdown-item border-bottom d-flex flex-column align-items-start ${!n.isRead ? 'bg-light' : ''}`}
                                style={{ padding: '12px 15px', whiteSpace: 'normal' }}
                                onClick={() => setNotificationOpen(false)}
                              >
                                <div className="d-flex w-100 justify-content-between align-items-center mb-1">
                                  <strong className={`mb-0 ${!n.isRead ? 'text-danger' : 'text-dark'}`} style={{ fontSize: '0.9rem' }}>{n.title}</strong>
                                  {!n.isRead && <span className="badge badge-danger" style={{ width: '8px', height: '8px', borderRadius: '50%', padding: 0 }}></span>}
                                </div>
                                <span className="text-muted text-truncate w-100" style={{ fontSize: '0.8rem' }}>{n.message}</span>
                                <small className="text-secondary mt-1">{new Date(n.createdAt).toLocaleString('vi-VN')}</small>
                              </Link>
                            ))
                          ) : (
                            <div className="text-center p-4 text-muted">
                              <i className="fa-regular fa-bell-slash fs-2 mb-2"></i>
                              <p className="mb-0" style={{ fontSize: '0.9rem' }}>Không có thông báo nào</p>
                            </div>
                          )}
                        </div>
                        <div className="dropdown-footer border-top text-center" style={{ padding: '10px' }}>
                          <Link to="/notifications" className="font-weight-bold text-danger text-decoration-none" onClick={() => setNotificationOpen(false)}>
                            Xem tất cả thông báo <i className="fa-solid fa-arrow-right ml-1"></i>
                          </Link>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* 3. NAVIGATION BAR (Desktop Dynamic Menu - Chuẩn màu đỏ MyKingdom) */}
          <nav className="navbar-custom d-none d-lg-block">
            <div className="container">
              <ul className="nav-menu-list">
                {menus.map((menu) => {
                  const isProducts = menu.url === '/products' || menu.title.toLowerCase().includes('sản phẩm');
                  
                  if (isProducts) {
                    return (
                      <li key={menu.id} className="nav-menu-item dropdown-products">
                        <NavLink 
                          to="/products" 
                          className={({ isActive }) => isActive ? "nav-menu-link active" : "nav-menu-link"}
                        >
                          {menu.title.toLowerCase().includes('trang chủ') && <i className="fa-solid fa-house mr-1"></i>}
                          {menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-newspaper mr-1"></i>}
                          <span>{menu.title}</span>
                          <i className="fa-solid fa-caret-down ml-1 text-white-50" style={{ fontSize: '0.8rem' }}></i>
                        </NavLink>

                        {/* Mega Menu động cấp 1 & cấp 2 (Tương thích danh mục 2 cấp và sản phẩm nổi bật) */}
                        {treeCategories.length > 0 && (
                          <div className="mega-menu-dropdown">
                            <div className="mega-menu-inner">
                              {/* Cột trái: Danh mục cấp 1 */}
                              <div className="mega-menu-left">
                                <ul className="mega-menu-cat-list">
                                  {treeCategories.map((subCat) => (
                                    <li 
                                      key={subCat.id} 
                                      className={`mega-menu-cat-item ${activeProductCat?.id === subCat.id ? 'active' : ''}`}
                                      onMouseEnter={() => {
                                        setActiveProductCat(subCat);
                                      }}
                                    >
                                      <div className="mega-menu-cat-info">
                                        {subCat.imageUrl ? (
                                          <img 
                                            src={getMediaUrl(subCat.imageUrl)} 
                                            alt={subCat.name} 
                                            className="mega-menu-cat-icon-img"
                                            style={{
                                              width: '20px',
                                              height: '20px',
                                              objectFit: 'contain',
                                              marginRight: '8px',
                                              flexShrink: 0
                                            }}
                                          />
                                        ) : (
                                          <i className="fa-solid fa-puzzle-piece text-danger" style={{ fontSize: '16px', marginRight: '8px', width: '20px', textAlign: 'center', flexShrink: 0 }}></i>
                                        )}
                                        <span>{subCat.name}</span>
                                      </div>
                                      {subCat.children && subCat.children.length > 0 && (
                                        <i className="fa-solid fa-chevron-right fs-7 text-muted"></i>
                                      )}
                                    </li>
                                  ))}
                                </ul>
                              </div>

                              {/* Cột giữa: Danh mục con cấp 2 */}
                              <div className="mega-menu-middle" style={{ flex: '1', paddingLeft: '24px', paddingRight: '24px' }}>
                                {activeProductCat && (
                                  <div className="mega-submenu-column" style={{ width: '100%' }}>
                                    <h4 className="mega-submenu-title">
                                      <i className="fa-solid fa-shapes mr-2 text-danger"></i>
                                      {activeProductCat.name}
                                    </h4>
                                    <ul className="mega-submenu-list" style={{ listStyle: 'none', padding: '0', margin: '0', display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '12px' }}>
                                      {activeProductCat.children && activeProductCat.children.length > 0 ? (
                                        activeProductCat.children.map((childCat) => (
                                          <li key={childCat.id} className="menu-lv3">
                                            <Link 
                                              to={`/products?category=${childCat.id}`} 
                                              className="mega-submenu-item-card"
                                              onClick={() => setMobileMenuOpen(false)}
                                            >
                                              <span className="d-flex align-items-center">
                                                {childCat.imageUrl ? (
                                                  <img 
                                                    src={getMediaUrl(childCat.imageUrl)} 
                                                    alt={childCat.name} 
                                                    className="subcat-icon-img"
                                                    style={{
                                                      width: '20px',
                                                      height: '20px',
                                                      objectFit: 'contain',
                                                      marginRight: '8px'
                                                    }}
                                                  />
                                                ) : (
                                                  <i className="fa-solid fa-puzzle-piece subcat-icon"></i>
                                                )}
                                                <span>{childCat.name}</span>
                                              </span>
                                              <i className="fa-solid fa-chevron-right subcat-chevron"></i>
                                            </Link>
                                          </li>
                                        ))
                                      ) : (
                                        <li className="menu-lv3" style={{ gridColumn: '1 / -1' }}>
                                          <Link 
                                            to={`/products?category=${activeProductCat.id}`} 
                                            className="mega-submenu-item-card"
                                            style={{ justifyContent: 'center', backgroundColor: 'rgba(207, 16, 45, 0.02)', borderColor: 'rgba(207, 16, 45, 0.1)' }}
                                            onClick={() => setMobileMenuOpen(false)}
                                          >
                                            <span className="d-flex align-items-center">
                                              {activeProductCat.imageUrl ? (
                                                <img 
                                                  src={getMediaUrl(activeProductCat.imageUrl)} 
                                                  alt={activeProductCat.name} 
                                                  className="subcat-icon-img"
                                                  style={{
                                                    width: '20px',
                                                    height: '20px',
                                                    objectFit: 'contain',
                                                    marginRight: '8px'
                                                  }}
                                                />
                                              ) : (
                                                <i className="fa-solid fa-arrow-right-to-bracket subcat-icon"></i>
                                              )}
                                              <span>Xem tất cả {activeProductCat.name}</span>
                                            </span>
                                            <i className="fa-solid fa-chevron-right subcat-chevron"></i>
                                          </Link>
                                        </li>
                                      )}
                                    </ul>
                                  </div>
                                )}
                              </div>

                            </div>
                          </div>
                        )}
                      </li>
                    );
                  }

                  const hasChildren = menu.children && menu.children.length > 0;
                  
                  if (hasChildren) {
                    return (
                      <li key={menu.id} className="nav-menu-item dropdown-standard">
                        <NavLink 
                          to={menu.url} 
                          className={({ isActive }) => isActive ? "nav-menu-link active" : "nav-menu-link"}
                        >
                          {menu.title.toLowerCase().includes('trang chủ') && <i className="fa-solid fa-house mr-1"></i>}
                          {menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-newspaper mr-1"></i>}
                          <span>{menu.title}</span>
                          <i className="fa-solid fa-caret-down ml-1 text-white-50" style={{ fontSize: '0.8rem' }}></i>
                        </NavLink>
                        <ul className="dropdown-standard-menu">
                          {menu.children.map((child) => (
                            <li key={child.id}>
                              <Link to={child.url} className="dropdown-standard-item">
                                {child.title}
                              </Link>
                            </li>
                          ))}
                        </ul>
                      </li>
                    );
                  }

                  return (
                    <li key={menu.id} className="nav-menu-item">
                      <NavLink 
                        to={menu.url} 
                        end={menu.url === '/'}
                        className={({ isActive }) => isActive ? "nav-menu-link active" : "nav-menu-link"}
                      >
                        {menu.title.toLowerCase().includes('trang chủ') && <i className="fa-solid fa-house mr-1"></i>}
                        {menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-newspaper mr-1"></i>}
                        <span>{menu.title}</span>
                      </NavLink>
                    </li>
                  );
                })}
              </ul>
            </div>
          </nav>
        </header>
      </div>

      {/* ==========================================
          MOBILE MENU DRAWER (Động 100% từ API)
      ========================================== */}
      <div 
        className={`drawer-backdrop ${mobileMenuOpen ? 'open' : ''}`}
        onClick={() => setMobileMenuOpen(false)}
      ></div>
      
      <div className={`custom-drawer left-drawer ${mobileMenuOpen ? 'open' : ''}`}>
        <div className="drawer-header">
          <h3 className="drawer-title">Menu</h3>
          <button className="drawer-close-btn" onClick={() => setMobileMenuOpen(false)}>
            <i className="fa-solid fa-xmark"></i>
          </button>
        </div>

        <div className="mobile-menu-container">
          <div 
            className="mobile-menu-slider"
            style={{ transform: `translateX(-${mobileMenuLevel * 33.333}%)` }}
          >
            {/* LEVEL 1: Menu chính */}
            <div className="mobile-menu-level">
              <ul className="mobile-menu-list">
                {menus.map((menu) => {
                  const isProducts = menu.url === '/products' || menu.title.toLowerCase().includes('sản phẩm');
                  
                  if (isProducts) {
                    return (
                      <li key={menu.id}>
                        <div 
                          className="mobile-menu-item-row" 
                          onClick={() => setMobileMenuLevel(1)}
                        >
                          <div className="mobile-menu-link-only">
                            <i className="fa-solid fa-box-open mr-2"></i>
                            <span>{menu.title}</span>
                          </div>
                          <i className="fa-solid fa-chevron-right fs-6 text-muted"></i>
                        </div>
                      </li>
                    );
                  }

                  const hasChildren = menu.children && menu.children.length > 0;
                  if (hasChildren) {
                    const isOpen = !!openMobileSubmenus[menu.id];
                    return (
                      <li key={menu.id}>
                        <div 
                          className="mobile-menu-item-row" 
                          onClick={() => toggleMobileSubmenu(menu.id)}
                        >
                          <div className="mobile-menu-link-only">
                            {menu.title.toLowerCase().includes('trang chủ') && <i className="fa-solid fa-house mr-2"></i>}
                            {menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-newspaper mr-2"></i>}
                            {!menu.title.toLowerCase().includes('trang chủ') && !menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-link mr-2"></i>}
                            <span>{menu.title}</span>
                          </div>
                          <i className={`fa-solid ${isOpen ? 'fa-chevron-down' : 'fa-chevron-right'} fs-6 text-muted`}></i>
                        </div>
                        {isOpen && (
                          <ul className="mobile-submenu-list-nested" style={{ listStyle: 'none', paddingLeft: '24px', backgroundColor: 'var(--mk-gray-100)', margin: 0 }}>
                            {menu.children.map((child) => (
                              <li key={child.id}>
                                <Link 
                                  to={child.url} 
                                  className="mobile-menu-item-row" 
                                  style={{ borderBottom: '1px solid var(--mk-gray-200)' }}
                                  onClick={() => setMobileMenuOpen(false)}
                                >
                                  <span>{child.title}</span>
                                </Link>
                              </li>
                            ))}
                          </ul>
                        )}
                      </li>
                    );
                  }

                  return (
                    <li key={menu.id}>
                      <Link 
                        to={menu.url} 
                        className="mobile-menu-item-row" 
                        onClick={() => setMobileMenuOpen(false)}
                      >
                        <div className="mobile-menu-link-only">
                          {menu.title.toLowerCase().includes('trang chủ') && <i className="fa-solid fa-house mr-2"></i>}
                          {menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-newspaper mr-2"></i>}
                          {!menu.title.toLowerCase().includes('trang chủ') && !menu.title.toLowerCase().includes('tin tức') && <i className="fa-solid fa-link mr-2"></i>}
                          <span>{menu.title}</span>
                        </div>
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>

            {/* LEVEL 2: Danh mục sản phẩm */}
            <div className="mobile-menu-level">
              <div className="mobile-back-row" onClick={() => setMobileMenuLevel(0)}>
                <i className="fa-solid fa-chevron-left mr-2"></i> Quay lại
              </div>
              <ul className="mobile-menu-list">
                <li>
                  <Link 
                    to="/products" 
                    className="mobile-menu-item-row"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    <div className="mobile-menu-link-only">
                      <span>Tất cả sản phẩm</span>
                    </div>
                  </Link>
                </li>

                {treeCategories.map((rootCat) => {
                  const hasSubChildren = rootCat.children && rootCat.children.length > 0;
                  return (
                    <li key={rootCat.id}>
                      {hasSubChildren ? (
                        <div 
                          className="mobile-menu-item-row"
                          onClick={() => { setMobileActiveCat(rootCat); setMobileMenuLevel(2); }}
                        >
                          <div className="mobile-menu-link-only">
                            {rootCat.imageUrl ? (
                              <img 
                                src={getMediaUrl(rootCat.imageUrl)} 
                                alt={rootCat.name} 
                                className="mobile-menu-icon"
                                style={{
                                  width: '24px',
                                  height: '24px',
                                  objectFit: 'contain',
                                  marginRight: '8px'
                                }}
                              />
                            ) : (
                              <i className="fa-solid fa-puzzle-piece text-danger" style={{ fontSize: '1.1rem', marginRight: '8px', width: '24px', textAlign: 'center' }}></i>
                            )}
                            <span>{rootCat.name}</span>
                          </div>
                          <i className="fa-solid fa-chevron-right fs-6 text-muted"></i>
                        </div>
                      ) : (
                        <Link 
                          to={`/products?category=${rootCat.id}`} 
                          className="mobile-menu-item-row"
                          onClick={() => setMobileMenuOpen(false)}
                        >
                          <div className="mobile-menu-link-only">
                            {rootCat.imageUrl ? (
                              <img 
                                src={getMediaUrl(rootCat.imageUrl)} 
                                alt={rootCat.name} 
                                className="mobile-menu-icon"
                                style={{
                                  width: '24px',
                                  height: '24px',
                                  objectFit: 'contain',
                                  marginRight: '8px'
                                }}
                              />
                            ) : (
                              <i className="fa-solid fa-puzzle-piece text-danger" style={{ fontSize: '1.1rem', marginRight: '8px', width: '24px', textAlign: 'center' }}></i>
                            )}
                            <span>{rootCat.name}</span>
                          </div>
                        </Link>
                      )}
                    </li>
                  );
                })}
              </ul>
            </div>

            {/* LEVEL 3: Danh mục con */}
            <div className="mobile-menu-level">
              <div className="mobile-back-row" onClick={() => setMobileMenuLevel(1)}>
                <i className="fa-solid fa-chevron-left mr-2"></i> Quay lại {mobileActiveCat?.name}
              </div>
              <ul className="mobile-menu-list">
                {mobileActiveCat && (
                  <li>
                    <Link 
                      to={`/products?category=${mobileActiveCat.id}`} 
                      className="mobile-menu-item-row"
                      onClick={() => setMobileMenuOpen(false)}
                    >
                      <div className="mobile-menu-link-only">
                        {mobileActiveCat.imageUrl ? (
                          <img 
                            src={getMediaUrl(mobileActiveCat.imageUrl)} 
                            alt={mobileActiveCat.name} 
                            className="mobile-menu-icon"
                            style={{
                              width: '24px',
                              height: '24px',
                              objectFit: 'contain',
                              marginRight: '8px'
                            }}
                          />
                        ) : (
                          <i className="fa-solid fa-puzzle-piece text-danger" style={{ fontSize: '1.1rem', marginRight: '8px', width: '24px', textAlign: 'center' }}></i>
                        )}
                        <span>Xem tất cả {mobileActiveCat.name}</span>
                      </div>
                    </Link>
                  </li>
                )}

                {mobileActiveCat && mobileActiveCat.children.map((childCat) => (
                  <li key={childCat.id}>
                    <Link 
                      to={`/products?category=${childCat.id}`} 
                      className="mobile-menu-item-row"
                      onClick={() => setMobileMenuOpen(false)}
                    >
                      <div className="mobile-menu-link-only">
                        {childCat.imageUrl ? (
                          <img 
                            src={getMediaUrl(childCat.imageUrl)} 
                            alt={childCat.name} 
                            className="mobile-menu-icon"
                            style={{
                              width: '24px',
                              height: '24px',
                              objectFit: 'contain',
                              marginRight: '8px'
                            }}
                          />
                        ) : (
                          <i className="fa-solid fa-puzzle-piece text-danger" style={{ fontSize: '1.1rem', marginRight: '8px', width: '24px', textAlign: 'center' }}></i>
                        )}
                        <span>{childCat.name}</span>
                      </div>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </div>

      {/* ==========================================
          CART DRAWER (Giỏ Hàng Slide Cạnh Phải)
      ========================================== */}
      <div 
        className={`drawer-backdrop ${cartDrawerOpen ? 'open' : ''}`}
        onClick={() => setCartDrawerOpen(false)}
      ></div>

      <div className={`custom-drawer right-drawer ${cartDrawerOpen ? 'open' : ''}`}>
        <div className="drawer-header">
          <h3 className="drawer-title">Giỏ hàng của bạn ({cartCount})</h3>
          <button className="drawer-close-btn" onClick={() => setCartDrawerOpen(false)}>
            <i className="fa-solid fa-xmark"></i>
          </button>
        </div>

        <div className="cart-drawer-content">


          {cartItems.length === 0 ? (
            <div className="text-center py-5 text-muted">
              <i className="fa-solid fa-bag-shopping fs-1 mb-3 text-black-50"></i>
              <p className="font-weight-bold">Giỏ hàng trống</p>
              <Link to="/products" className="btn btn-danger btn-sm px-4 rounded-pill mt-2" onClick={() => setCartDrawerOpen(false)}>Mua sắm ngay</Link>
            </div>
          ) : (
            <div className="cart-items-table">
              {cartItems.map((item) => (
                <div key={item.id} className="cart-drawer-item">
                  <img src={item.imageUrl} alt={item.name || item.productName} className="cart-item-img" />
                  <div className="cart-item-info">
                    <Link to={`/products/${item.id}`} className="cart-item-name font-weight-bold" onClick={() => setCartDrawerOpen(false)}>
                      {item.name || item.productName}
                    </Link>
                    <span className="cart-item-price">{(item.price).toLocaleString('vi-VN')}₫</span>
                    
                    <div className="cart-item-actions">
                      <div className="qty-control">
                        <button className="qty-btn" onClick={() => handleUpdateQty(item.id, -1)}>-</button>
                        <span className="qty-val">{item.quantity}</span>
                        <button className="qty-btn" onClick={() => handleUpdateQty(item.id, 1)}>+</button>
                      </div>
                      <button className="cart-item-remove" onClick={() => handleRemoveItem(item.id)} title="Xóa">
                        <i className="fa-regular fa-trash-can"></i>
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {cartItems.length > 0 && (
          <div className="cart-drawer-footer">


            <div className="cart-total-row">
              <span>Tổng cộng:</span>
              <span className="cart-total-price">
                {(cartItems.reduce((acc, item) => acc + item.price * item.quantity, 0)).toLocaleString('vi-VN')}₫
              </span>
            </div>

            <div className="cart-action-btns">
              <Link to="/cart" className="cart-drawer-btn cart-drawer-btn-view" onClick={() => setCartDrawerOpen(false)}>
                Xem giỏ hàng
              </Link>
              <button 
                className="cart-drawer-btn cart-drawer-btn-checkout"
                onClick={() => {
                  setCartDrawerOpen(false);
                  navigate('/checkout');
                }}
              >
                Thanh toán ngay
              </button>
            </div>
          </div>
        )}
      </div>
    </>
  );
};

export default Header;
