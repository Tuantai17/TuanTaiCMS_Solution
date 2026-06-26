import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import productService from '../services/productService';
import { getMediaUrl } from '../utils/mediaUrl';
import '../assets/css/Cart.css';

const Cart = () => {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState([]);
  const [selectedItemIds, setSelectedItemIds] = useState([]);
  const [loading, setLoading] = useState(true);

  // Tải giỏ hàng từ localStorage và gọi API để lấy giá mới nhất & tồn kho
  useEffect(() => {
    const loadAndSyncCart = async () => {
      setLoading(true);
      try {
        const storedCart = localStorage.getItem('cart');
        let currentCart = [];
        if (storedCart) {
          currentCart = JSON.parse(storedCart);
        }

        if (currentCart.length === 0) {
          setCartItems([]);
          setLoading(false);
          return;
        }

        // Lấy thông tin mới nhất từ Product Service
        const updatedCartPromises = currentCart.map(async (item) => {
          try {
            const productInfo = await productService.getProductDetail(item.id);
            // Tính giá hiển thị
            const isSale = productInfo.isSale === true && productInfo.salePrice > 0;
            const displayPrice = isSale ? productInfo.salePrice : productInfo.price;
            const originalPrice = productInfo.price;
            
            // Xử lý số lượng: Không được vượt quá tồn kho hiện tại
            let safeQuantity = item.quantity;
            if (safeQuantity > productInfo.stockQuantity) {
              safeQuantity = productInfo.stockQuantity;
            }
            if (safeQuantity < 1 && productInfo.stockQuantity > 0) {
              safeQuantity = 1;
            } else if (productInfo.stockQuantity === 0) {
              safeQuantity = 0; // Hết hàng
            }

            return {
              ...item,
              price: displayPrice,
              originalPrice: originalPrice,
              isSale: isSale,
              stockQuantity: productInfo.stockQuantity,
              quantity: safeQuantity,
              imageUrl: getMediaUrl(productInfo.imageUrl)
            };
          } catch (err) {
            // Nếu sản phẩm không tồn tại (bị xóa) -> đánh dấu tồn kho = 0
            return { ...item, stockQuantity: 0, quantity: 0, price: item.price };
          }
        });

        const updatedCart = await Promise.all(updatedCartPromises);
        
        // Cập nhật lại vào LocalStorage để đồng bộ (Header sẽ nhận giá mới)
        setCartItems(updatedCart);
        localStorage.setItem('cart', JSON.stringify(updatedCart));
        window.dispatchEvent(new Event('cartChange'));
        
        // Mặc định chọn tất cả các sản phẩm còn hàng
        const availableItemIds = updatedCart.filter(item => item.stockQuantity > 0).map(i => i.id);
        setSelectedItemIds(availableItemIds);

      } catch (e) {
        console.error("Lỗi khi đồng bộ giỏ hàng:", e);
      } finally {
        setLoading(false);
      }
    };

    loadAndSyncCart();
  }, []);

  // Hàm lưu trạng thái giỏ hàng vào local storage
  const saveCartToStorage = (updatedCart) => {
    setCartItems(updatedCart);
    localStorage.setItem('cart', JSON.stringify(updatedCart));
    window.dispatchEvent(new Event('cartChange'));
  };

  // Hàm thay đổi số lượng sản phẩm (tăng/giảm)
  const handleQuantityChange = (id, change) => {
    const updatedCart = cartItems.map(item => {
      if (item.id === id) {
        let newQty = item.quantity + change;
        if (newQty < 1) newQty = 1;
        if (newQty > item.stockQuantity) {
          alert(`Sản phẩm này chỉ còn tối đa ${item.stockQuantity} sản phẩm trong kho!`);
          newQty = item.stockQuantity;
        }
        return { ...item, quantity: newQty };
      }
      return item;
    });
    saveCartToStorage(updatedCart);
  };

  // Hàm xóa sản phẩm khỏi giỏ
  const handleRemoveItem = (id) => {
    const updatedCart = cartItems.filter(item => item.id !== id);
    saveCartToStorage(updatedCart);
    setSelectedItemIds(prev => prev.filter(selectedId => selectedId !== id));
  };

  // Hàm chọn 1 sản phẩm
  const handleSelectItem = (id) => {
    setSelectedItemIds(prev => {
      if (prev.includes(id)) {
        return prev.filter(item => item !== id);
      } else {
        return [...prev, id];
      }
    });
  };

  // Hàm chọn / bỏ chọn tất cả
  const handleSelectAll = () => {
    const availableItems = cartItems.filter(item => item.stockQuantity > 0);
    if (selectedItemIds.length === availableItems.length && availableItems.length > 0) {
      // Đã chọn tất cả -> bỏ chọn tất cả
      setSelectedItemIds([]);
    } else {
      // Chọn tất cả
      setSelectedItemIds(availableItems.map(i => i.id));
    }
  };

  // Hàm xóa các mục đã chọn
  const handleRemoveSelected = () => {
    if (selectedItemIds.length === 0) return;
    if (window.confirm('Bạn có chắc chắn muốn xóa các sản phẩm đã chọn khỏi giỏ hàng?')) {
      const updatedCart = cartItems.filter(item => !selectedItemIds.includes(item.id));
      saveCartToStorage(updatedCart);
      setSelectedItemIds([]);
    }
  };

  // Tiến hành thanh toán
  const handleCheckout = () => {
    if (selectedItemIds.length === 0) {
      alert("Vui lòng chọn ít nhất 1 sản phẩm để thanh toán!");
      return;
    }
    
    // Lưu danh sách sản phẩm được chọn vào sessionStorage để Checkout lấy
    const checkoutItems = cartItems.filter(item => selectedItemIds.includes(item.id));
    sessionStorage.setItem('checkoutItems', JSON.stringify(checkoutItems));
    
    navigate('/checkout');
  };

  // Tính toán Tóm tắt đơn hàng (chỉ tính các sản phẩm được chọn)
  const selectedCartItems = cartItems.filter(item => selectedItemIds.includes(item.id));
  const subTotal = selectedCartItems.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  const totalQuantity = selectedCartItems.reduce((acc, item) => acc + item.quantity, 0);

  if (loading) {
    return (
      <div className="cart-page-bg">
        <div className="container cart-loading-state">
          <div className="spinner-border text-danger" role="status" style={{ width: '3rem', height: '3rem' }}>
            <span className="sr-only">Đang đồng bộ...</span>
          </div>
          <h5 className="mt-4 text-dark font-weight-bold">Đang đồng bộ giỏ hàng...</h5>
          <p className="text-secondary">Vui lòng chờ trong giây lát, chúng tôi đang kiểm tra giá và tồn kho mới nhất.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-page-bg animate--fade-in">
      <div className="container">
        {/* Breadcrumb */}
        <div className="cart-breadcrumb">
          <Link to="/">Trang chủ</Link>
          <span className="mx-2">/</span>
          <span>Giỏ hàng của bạn</span>
        </div>

        <h1 className="cart-page-title">
          <i className="fa-solid fa-basket-shopping"></i>
          Giỏ hàng của bạn
          <span className="cart-count-text">({cartItems.length} sản phẩm)</span>
        </h1>

        {cartItems.length === 0 ? (
          <div className="cart-empty-state">
            <i className="fa-solid fa-cart-shopping main-icon"></i>
            <h3>Giỏ hàng của bạn đang trống!</h3>
            <p>Hãy nhanh tay khám phá vương quốc đồ chơi và chọn cho mình sản phẩm ưng ý nhé.</p>
            <Link to="/products" className="cart-empty-btn">
              Khám phá ngay <i className="fa-solid fa-arrow-right ml-2"></i>
            </Link>
          </div>
        ) : (
          <div className="cart-layout">
            {/* CỘT TRÁI: BẢNG SẢN PHẨM */}
            <div className="cart-left-col">
              <div className="cart-table-card">
                {/* Header Actions */}
                <div className="cart-table-header">
                  <label className="cart-checkbox-wrapper mb-0">
                    <input 
                      type="checkbox" 
                      className="cart-custom-checkbox" 
                      checked={selectedItemIds.length > 0 && selectedItemIds.length === cartItems.filter(i => i.stockQuantity > 0).length}
                      ref={input => {
                        if (input) {
                          input.indeterminate = selectedItemIds.length > 0 && selectedItemIds.length < cartItems.filter(i => i.stockQuantity > 0).length;
                        }
                      }}
                      onChange={handleSelectAll}
                    />
                    <span>Chọn tất cả ({cartItems.filter(i => i.stockQuantity > 0).length})</span>
                  </label>
                  
                  <div className="cart-header-actions">
                    <button 
                      className="cart-header-btn delete-all"
                      onClick={handleRemoveSelected}
                      disabled={selectedItemIds.length === 0}
                    >
                      <i className="fa-regular fa-trash-can"></i> Xóa mục đã chọn
                    </button>
                  </div>
                </div>

                {/* Table Header Columns */}
                <div className="cart-columns-head cart-grid-row">
                  <div className="col-check"></div>
                  <div className="col-product">Sản phẩm</div>
                  <div className="col-price">Đơn giá</div>
                  <div className="col-qty">Số lượng</div>
                  <div className="col-total">Thành tiền</div>
                  <div className="col-action"></div>
                </div>

                {/* Table Body */}
                <div className="cart-table-body">
                  {cartItems.map((item) => {
                    const isOutOfStock = item.stockQuantity === 0;
                    const isSelected = selectedItemIds.includes(item.id);
                    const itemTotal = item.price * item.quantity;

                    return (
                      <div className={`cart-item-row cart-grid-row ${isOutOfStock ? 'is-disabled' : ''}`} key={item.id}>
                        {/* Checkbox */}
                        <div className="cart-item-checkbox">
                          <input 
                            type="checkbox" 
                            className="cart-custom-checkbox" 
                            checked={isSelected}
                            onChange={() => handleSelectItem(item.id)}
                            disabled={isOutOfStock}
                          />
                        </div>

                        {/* Thông tin sản phẩm */}
                        <div className="cart-item-info">
                          <Link to={`/products/${item.id}`}>
                            <img src={item.imageUrl || "https://placehold.co/150x150?text=No+Image"} alt={item.name} className="cart-item-img" />
                          </Link>
                          <div className="cart-item-details">
                            <Link to={`/products/${item.id}`} className="cart-item-name" title={item.name}>
                              {item.name}
                            </Link>
                            <span className="cart-item-sku">{item.sku}</span>
                            {isOutOfStock ? (
                              <span className="cart-item-stock stock-out"><i className="fa-solid fa-circle-xmark"></i> Hết hàng</span>
                            ) : item.stockQuantity < 5 ? (
                              <span className="cart-item-stock stock-low"><i className="fa-solid fa-triangle-exclamation"></i> Sắp hết (còn {item.stockQuantity})</span>
                            ) : (
                              <span className="cart-item-stock stock-ok"><i className="fa-solid fa-check"></i> Còn hàng</span>
                            )}
                          </div>
                        </div>

                        {/* Đơn giá */}
                        <div className="cart-item-unit-price">
                          <span className="cart-price-current">
                            {new Intl.NumberFormat('vi-VN').format(item.price)}₫
                          </span>
                          {item.isSale && (
                            <span className="cart-price-original">
                              {new Intl.NumberFormat('vi-VN').format(item.originalPrice)}₫
                            </span>
                          )}
                        </div>

                        {/* Số lượng */}
                        <div className="cart-item-quantity">
                          {isOutOfStock ? (
                            <span className="badge badge-secondary py-2 px-3">0</span>
                          ) : (
                            <div className="cart-qty-control">
                              <button 
                                className="cart-qty-btn" 
                                onClick={() => handleQuantityChange(item.id, -1)}
                                disabled={item.quantity <= 1}
                              >
                                <i className="fa-solid fa-minus"></i>
                              </button>
                              <input type="text" className="cart-qty-input" value={item.quantity} readOnly />
                              <button 
                                className="cart-qty-btn" 
                                onClick={() => handleQuantityChange(item.id, 1)}
                                disabled={item.quantity >= item.stockQuantity}
                              >
                                <i className="fa-solid fa-plus"></i>
                              </button>
                            </div>
                          )}
                        </div>

                        {/* Thành tiền */}
                        <div className="cart-item-total">
                          {new Intl.NumberFormat('vi-VN').format(itemTotal)}₫
                        </div>

                        {/* Thao tác xóa */}
                        <div className="cart-item-actions">
                          <button 
                            className="cart-delete-btn" 
                            title="Xóa sản phẩm"
                            onClick={() => handleRemoveItem(item.id)}
                          >
                            <i className="fa-regular fa-trash-can"></i>
                          </button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>

            {/* CỘT PHẢI: TÓM TẮT ĐƠN HÀNG */}
            <div className="cart-summary-col">
              <div className="cart-summary-card">
                <h3 className="cart-summary-title">Tóm tắt đơn hàng</h3>
                
                <div className="cart-summary-row">
                  <span>Sản phẩm đã chọn:</span>
                  <span className="bold">{totalQuantity}</span>
                </div>
                
                <div className="cart-summary-row">
                  <span>Tạm tính hàng hóa:</span>
                  <span className="bold">{new Intl.NumberFormat('vi-VN').format(subTotal)} ₫</span>
                </div>
                
                <div className="cart-summary-row">
                  <span>Phí vận chuyển:</span>
                  <span><i>(Tính ở bước thanh toán)</i></span>
                </div>
                
                <div className="cart-summary-row total">
                  <span className="label">Tổng cộng thanh toán:</span>
                  <span className="value">{new Intl.NumberFormat('vi-VN').format(subTotal)} ₫</span>
                </div>
                
                <button 
                  className="cart-checkout-btn" 
                  onClick={handleCheckout}
                  disabled={selectedItemIds.length === 0}
                >
                  Tiến hành thanh toán <i className="fa-solid fa-arrow-right"></i>
                </button>
                
                <Link to="/products" className="cart-continue-btn">
                  Tiếp tục mua sắm
                </Link>
              </div>
            </div>
          </div>
        )}

        {/* Cam kết mua hàng */}
        <div className="cart-service-bar rounded-lg px-4 mb-4">
          <div className="row">
            <div className="col-12 col-md-4">
              <div className="service-item">
                <div className="service-icon"><i className="fa-solid fa-shield-halved"></i></div>
                <div className="service-text">
                  <h5>Sản phẩm chính hãng</h5>
                  <p>Cam kết 100% hàng chính hãng, an toàn tuyệt đối cho bé.</p>
                </div>
              </div>
            </div>
            <div className="col-12 col-md-4">
              <div className="service-item">
                <div className="service-icon"><i className="fa-solid fa-truck-fast"></i></div>
                <div className="service-text">
                  <h5>Giao hàng hỏa tốc</h5>
                  <p>Miễn phí giao hàng cho đơn từ 500k, giao hỏa tốc nội thành.</p>
                </div>
              </div>
            </div>
            <div className="col-12 col-md-4">
              <div className="service-item">
                <div className="service-icon"><i className="fa-solid fa-right-left"></i></div>
                <div className="service-text">
                  <h5>Đổi trả dễ dàng</h5>
                  <p>Hỗ trợ đổi trả miễn phí trong 7 ngày nếu lỗi nhà sản xuất.</p>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Cart;

