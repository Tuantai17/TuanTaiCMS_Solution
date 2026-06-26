import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import '../assets/css/Profile.css';
import '../assets/css/OrderDetail.css';
import orderService from '../services/orderService';
import authService from '../services/authService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import { getMediaUrl } from '../utils/mediaUrl';
import { formatOrderCode, parseOrderNotes } from '../utils/orderStatus';

const FALLBACK_IMAGE = 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image';

// Mapping for the timeline
const ORDER_STATUS_CONFIG = {
  0: { label: "Chờ duyệt", step: 1, icon: "fa-regular fa-hourglass-half" },
  1: { label: "Đã duyệt", step: 2, icon: "fa-solid fa-clipboard-check" },
  2: { label: "Đang chuẩn bị", step: 3, icon: "fa-solid fa-box-open" },
  3: { label: "Đang giao hàng", step: 4, icon: "fa-solid fa-truck-fast" },
  4: { label: "Hoàn thành", step: 5, icon: "fa-regular fa-circle-check" },
  5: { label: "Đã hủy", step: -1, icon: "fa-regular fa-circle-xmark" }
};

function OrderDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();

  const [customer, setCustomer] = useState(null);
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [fetchingProfile, setFetchingProfile] = useState(true);
  const [error, setError] = useState('');

  // Cancel Modal states
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [otherReason, setOtherReason] = useState('');
  const [cancelling, setCancelling] = useState(false);

  const redirectToLogin = useCallback((message) => {
    const staleCustomer = getStoredCustomer();
    const email = staleCustomer?.email || '';
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));

    navigate('/login', {
      replace: true,
      state: {
        message,
        email,
        from: { pathname: `/account/orders/${id}` },
      },
    });
  }, [id, navigate]);

  useEffect(() => {
    const storedCustomer = getStoredCustomer();
    if (!storedCustomer?.customerId) {
      redirectToLogin('Vui lòng đăng nhập để xem chi tiết đơn hàng.');
      return;
    }

    if (!storedCustomer?.accessToken) {
      redirectToLogin('Phiên đăng nhập cũ không còn phù hợp với bảo mật mới. Vui lòng đăng nhập lại để tiếp tục.');
      return;
    }

    const loadCustomer = async () => {
      try {
        const profile = await authService.getProfile(storedCustomer.customerId);
        setCustomer({ ...storedCustomer, ...profile });
      } catch {
        setCustomer(storedCustomer);
      } finally {
        setFetchingProfile(false);
      }
    };

    loadCustomer();
  }, [redirectToLogin]);

  useEffect(() => {
    if (fetchingProfile || !customer || !id) {
      return;
    }

    const loadOrder = async () => {
      setLoading(true);
      setError('');

      try {
        const response = await orderService.getMyOrderDetail(id);
        setOrder(response);
      } catch (requestError) {
        if (requestError?.response?.status === 401) {
          clearStoredCustomer();
          window.dispatchEvent(new Event('customerLoginStateChange'));
          redirectToLogin('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại để xem chi tiết đơn hàng.');
          return;
        }

        if (requestError?.response?.status === 404) {
          setOrder(null);
          setError('Không tìm thấy đơn hàng.');
        } else {
          setError(requestError?.response?.data?.message || 'Không thể tải chi tiết đơn hàng. Vui lòng thử lại.');
        }
      } finally {
        setLoading(false);
      }
    };

    loadOrder();
  }, [customer, fetchingProfile, id, redirectToLogin]);

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const handleReorder = () => {
    if (!order || !order.items) return;
    
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    let addedCount = 0;

    order.items.forEach(item => {
      const index = cart.findIndex(c => c.id === item.productId);
      const qtyToAdd = item.quantity || 1;

      if (index > -1) {
        cart[index].quantity += qtyToAdd;
        cart[index].price = item.unitPrice; // Use latest historical price
      } else {
        cart.push({
          id: item.productId,
          name: item.productName,
          price: item.unitPrice,
          quantity: qtyToAdd,
          imageUrl: getMediaUrl(item.productImageUrl),
          sku: `#${1000 + item.productId}` // mock SKU since backend doesn't return it in OrderDetailItemDto
        });
      }
      addedCount += qtyToAdd;
    });

    localStorage.setItem('cart', JSON.stringify(cart));
    window.dispatchEvent(new Event('cartChange'));
    
    alert(`Đã thêm ${addedCount} sản phẩm từ đơn hàng vào giỏ!`);
    navigate('/cart');
  };

  const handleCancelOrder = async () => {
    const finalReason = cancelReason === 'other' ? otherReason : cancelReason;
    if (!finalReason.trim()) {
      alert('Vui lòng chọn hoặc nhập lý do hủy đơn hàng.');
      return;
    }

    setCancelling(true);
    try {
      await orderService.cancelMyOrder(order.id, finalReason);
      alert('Đã hủy đơn hàng thành công!');
      setShowCancelModal(false);
      // Update local state to reflect cancellation immediately
      setOrder(prev => ({ ...prev, status: 5 }));
    } catch (err) {
      alert(err?.response?.data?.message || 'Không thể hủy đơn hàng. Vui lòng thử lại.');
    } finally {
      setCancelling(false);
    }
  };

  if (fetchingProfile) {
    return (
      <div className="account-order-page">
        <div className="account-order-layout">
          <div className="profile-skeleton-sidebar">
            <div className="skeleton-block" style={{ height: '320px' }}></div>
          </div>
          <div className="order-detail-main-card">
            <div className="order-detail-skeleton">
              <div className="skel-header"></div>
              <div className="skel-timeline"></div>
              <div className="skel-grid">
                <div className="skel-box"></div>
                <div className="skel-box"></div>
                <div className="skel-box"></div>
                <div className="skel-box"></div>
              </div>
              <div className="skel-row"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const parsedNotes = order ? parseOrderNotes(order.notes) : { deliveryInfo: '', paymentMethod: '', customerNotes: '' };
  
  // Parse detailed delivery info if present
  let deliveryDetails = { name: '', phone: '', email: '', address: '' };
  if (parsedNotes.deliveryInfo) {
    const parts = parsedNotes.deliveryInfo.split(' - ');
    deliveryDetails.name = parts[0] || '';
    deliveryDetails.phone = parts[1] || '';
    deliveryDetails.email = parts[2] || '';
    deliveryDetails.address = parts.slice(3).join(' - ') || '';
  }

  const currentStatusConfig = order ? (ORDER_STATUS_CONFIG[order.status] || ORDER_STATUS_CONFIG[0]) : ORDER_STATUS_CONFIG[0];
  const isCancelled = order?.status === 5;

  return (
    <div className="account-order-page">
      <div className="account-order-layout">
        <AccountSidebar activeKey="order-history" customer={customer} onLogout={handleLogout} />

        <section className="order-detail-main-card animate--fade-in">
          {loading ? (
            <div className="order-detail-skeleton">
              <div className="skel-header"></div>
              <div className="skel-timeline"></div>
              <div className="skel-grid">
                <div className="skel-box"></div>
                <div className="skel-box"></div>
                <div className="skel-box"></div>
                <div className="skel-box"></div>
              </div>
              <div className="skel-row"></div>
            </div>
          ) : error ? (
            <div className="order-history-not-found" style={{ textAlign: 'center', padding: '60px 0' }}>
              <div style={{ fontSize: '48px', color: '#dc2626', marginBottom: '16px' }}>
                <i className="fa-regular fa-circle-xmark"></i>
              </div>
              <h3 style={{ fontSize: '20px', fontWeight: 'bold' }}>{error}</h3>
              <p style={{ color: '#6b7280', marginBottom: '24px' }}>Đơn hàng này có thể không tồn tại hoặc không thuộc quyền truy cập của tài khoản hiện tại.</p>
              <Link to="/account/orders" className="order-back-btn" style={{ background: '#d71920', color: '#fff', border: 'none' }}>
                Về lịch sử mua hàng
              </Link>
            </div>
          ) : (
            <>
              {/* Header */}
              <div className="order-detail-header">
                <div>
                  <h1 className="order-detail-title">Chi tiết đơn hàng</h1>
                  <p className="order-detail-description">Theo dõi thông tin, trạng thái và sản phẩm trong đơn hàng của bạn.</p>
                </div>
                <Link to="/account/orders" className="order-back-btn">
                  <i className="fa-solid fa-chevron-left" style={{ fontSize: '12px' }}></i> Quay lại lịch sử
                </Link>
              </div>

              {/* Timeline */}
              {isCancelled ? (
                <div className="order-timeline-cancelled">
                  <i className="fa-regular fa-circle-xmark"></i>
                  Đơn hàng đã bị hủy
                </div>
              ) : (
                <div className="order-status-timeline">
                  {/* Step 1: Chờ duyệt */}
                  <div className={`order-status-step ${currentStatusConfig.step > 1 ? 'completed' : currentStatusConfig.step === 1 ? 'active' : ''}`}>
                    <div className="order-step-icon">
                      <i className="fa-regular fa-hourglass-half"></i>
                    </div>
                    <span className="order-step-label">Chờ duyệt</span>
                    {currentStatusConfig.step >= 1 && (
                      <span className="order-step-time">
                        {new Date(order.orderDate).toLocaleDateString('vi-VN')}
                      </span>
                    )}
                  </div>

                  {/* Step 2: Đã duyệt */}
                  <div className={`order-status-step ${currentStatusConfig.step > 2 ? 'completed' : currentStatusConfig.step === 2 ? 'active' : ''}`}>
                    <div className="order-step-icon">
                      <i className="fa-solid fa-clipboard-check"></i>
                    </div>
                    <span className="order-step-label">Đã duyệt</span>
                  </div>

                  {/* Step 3: Đang chuẩn bị */}
                  <div className={`order-status-step ${currentStatusConfig.step > 3 ? 'completed' : currentStatusConfig.step === 3 ? 'active' : ''}`}>
                    <div className="order-step-icon">
                      <i className="fa-solid fa-box-open"></i>
                    </div>
                    <span className="order-step-label">Đang chuẩn bị</span>
                  </div>

                  {/* Step 4: Đang giao hàng */}
                  <div className={`order-status-step ${currentStatusConfig.step > 4 ? 'completed' : currentStatusConfig.step === 4 ? 'active' : ''}`}>
                    <div className="order-step-icon">
                      <i className="fa-solid fa-truck-fast"></i>
                    </div>
                    <span className="order-step-label">Đang giao hàng</span>
                  </div>

                  {/* Step 5: Hoàn thành */}
                  <div className={`order-status-step ${currentStatusConfig.step === 5 ? 'completed' : ''}`}>
                    <div className="order-step-icon">
                      <i className="fa-regular fa-circle-check"></i>
                    </div>
                    <span className="order-step-label">Hoàn thành</span>
                  </div>
                </div>
              )}

              {/* 4-Card Overview */}
              <div className="order-overview-grid">
                <div className="order-overview-card">
                  <div className="overview-lbl"><i className="fa-solid fa-bag-shopping"></i> Mã đơn hàng</div>
                  <div className="overview-val">{formatOrderCode(order.id)}</div>
                </div>
                <div className="order-overview-card">
                  <div className="overview-lbl"><i className="fa-regular fa-calendar-days"></i> Ngày đặt</div>
                  <div className="overview-val">
                    {new Date(order.orderDate).toLocaleDateString('vi-VN')}, {new Date(order.orderDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
                <div className="order-overview-card">
                  <div className="overview-lbl"><i className="fa-solid fa-hourglass-end"></i> Trạng thái</div>
                  <div className="overview-val" style={{ color: currentStatusConfig.step === -1 ? '#dc2626' : currentStatusConfig.step === 5 ? '#16a34a' : '#d97706' }}>
                    {currentStatusConfig.label}
                  </div>
                </div>
                <div className="order-overview-card">
                  <div className="overview-lbl"><i className="fa-solid fa-money-bill-wave"></i> Tổng tiền</div>
                  <div className="overview-val price">
                    {Number(order.totalAmount || 0).toLocaleString('vi-VN')}đ
                  </div>
                </div>
              </div>

              {/* 2-Column Information */}
              <div className="order-information-grid">
                {/* Delivery Info */}
                <div className="info-card">
                  <div className="info-card-title">
                    <i className="fa-solid fa-location-dot"></i> Thông tin giao hàng
                  </div>
                  {deliveryDetails.name ? (
                    <>
                      <div className="shipping-info-row">
                        <div className="info-lbl">Người nhận</div>
                        <div className="info-val">{deliveryDetails.name}</div>
                      </div>
                      <div className="shipping-info-row">
                        <div className="info-lbl">SĐT</div>
                        <div className="info-val">{deliveryDetails.phone}</div>
                      </div>
                      <div className="shipping-info-row">
                        <div className="info-lbl">Email</div>
                        <div className="info-val">{deliveryDetails.email}</div>
                      </div>
                      <div className="shipping-info-row">
                        <div className="info-lbl">Địa chỉ</div>
                        <div className="info-val">{deliveryDetails.address}</div>
                      </div>
                    </>
                  ) : (
                    <div className="info-val">Không xác định</div>
                  )}
                </div>

                {/* Payment Info */}
                <div className="info-card" style={{ display: 'flex', flexDirection: 'column' }}>
                  <div className="info-card-title">
                    <i className="fa-solid fa-credit-card"></i> Thông tin thanh toán
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Phương thức</div>
                    <div className="info-val">{parsedNotes.paymentMethod || 'Thanh toán khi nhận hàng (COD)'}</div>
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Trạng thái</div>
                    <div className="info-val">
                      {order?.status === 4 ? (
                        <span className="payment-badge paid">Đã thanh toán</span>
                      ) : (
                        <span className="payment-badge unpaid">Chưa thanh toán</span>
                      )}
                    </div>
                  </div>

                  {parsedNotes.customerNotes && (
                    <div className="order-note-card">
                      <i className="fa-solid fa-pen-clip"></i>
                      <div>
                        <div style={{ fontSize: '12px', fontWeight: 'bold', color: '#b45309', marginBottom: '2px' }}>Ghi chú giao hàng</div>
                        <p className="order-note-text">{parsedNotes.customerNotes}</p>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Products Section */}
              <div className="order-products-section">
                <h3 className="order-products-title">
                  <i className="fa-solid fa-box-open" style={{ color: '#d71920' }}></i> Sản phẩm trong đơn hàng
                </h3>
                
                <div className="order-product-grid product-grid-header">
                  <div>Sản phẩm</div>
                  <div style={{ textAlign: 'right' }}>Đơn giá</div>
                  <div style={{ textAlign: 'center' }}>Số lượng</div>
                  <div style={{ textAlign: 'right' }}>Thành tiền</div>
                </div>

                {order.items.map((item) => (
                  <div className="order-product-grid product-grid-row" key={item.id}>
                    <div className="product-info-col">
                      <img
                        src={getMediaUrl(item.productImageUrl, FALLBACK_IMAGE)}
                        alt={item.productName}
                        className="product-img"
                        onError={(event) => {
                          event.currentTarget.src = FALLBACK_IMAGE;
                        }}
                      />
                      <div>
                        <Link to={`/product/${item.productId}`} className="product-name">
                          {item.productName}
                        </Link>
                        <div className="product-sku">SKU{1000 + item.productId}</div>
                      </div>
                    </div>
                    
                    <div className="product-price-col">
                      {Number(item.unitPrice || 0).toLocaleString('vi-VN')}đ
                    </div>
                    
                    <div className="product-qty-col">
                      x{item.quantity}
                    </div>
                    
                    <div className="product-total-col">
                      {Number(item.lineTotal || 0).toLocaleString('vi-VN')}đ
                    </div>
                  </div>
                ))}
              </div>

              {/* Payment Summary */}
              <div className="order-summary-section">
                <div className="order-summary-box">
                  <div className="summary-row">
                    <span>Tạm tính</span>
                    <span style={{ color: '#172033', fontWeight: '500' }}>{Number(order.totalAmount || 0).toLocaleString('vi-VN')}đ</span>
                  </div>
                  <div className="summary-row total">
                    <span>Tổng thanh toán</span>
                    <span className="summary-val">{Number(order.totalAmount || 0).toLocaleString('vi-VN')}đ</span>
                  </div>
                </div>
              </div>

              {/* Actions */}
              <div className="order-actions-row">
                <button className="btn-support" onClick={() => navigate('/contact')}>
                  <i className="fa-solid fa-headset"></i> Liên hệ hỗ trợ
                </button>
                {order.status <= 1 && (
                  <button className="btn-cancel-order" onClick={() => {
                    setCancelReason('');
                    setOtherReason('');
                    setShowCancelModal(true);
                  }}>
                    <i className="fa-solid fa-ban"></i> Hủy đơn hàng
                  </button>
                )}
                {order.status === 4 && (
                  <button className="btn-reorder" onClick={handleReorder}>
                    <i className="fa-solid fa-cart-arrow-down"></i> Mua lại
                  </button>
                )}
              </div>

            </>
          )}
        </section>
      </div>

      {/* Cancel Modal */}
      {showCancelModal && (
        <div className="cancel-modal-overlay" onClick={() => !cancelling && setShowCancelModal(false)}>
          <div className="cancel-modal-content" onClick={e => e.stopPropagation()}>
            <div className="cancel-modal-header">
              <h3><i className="fa-solid fa-triangle-exclamation"></i> Xác nhận hủy đơn hàng</h3>
              <button className="close-modal-btn" onClick={() => !cancelling && setShowCancelModal(false)}>
                <i className="fa-solid fa-xmark"></i>
              </button>
            </div>
            
            <div className="cancel-modal-body">
              <p>Vui lòng cho chúng tôi biết lý do bạn muốn hủy đơn hàng này:</p>
              <div className="cancel-reason-list">
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="Muốn thay đổi địa chỉ giao hàng" checked={cancelReason === 'Muốn thay đổi địa chỉ giao hàng'} onChange={(e) => setCancelReason(e.target.value)} />
                  <span className="cancel-reason-label">Muốn thay đổi địa chỉ giao hàng</span>
                </label>
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="Đổi ý, không muốn mua nữa" checked={cancelReason === 'Đổi ý, không muốn mua nữa'} onChange={(e) => setCancelReason(e.target.value)} />
                  <span className="cancel-reason-label">Đổi ý, không muốn mua nữa</span>
                </label>
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="Đặt nhầm sản phẩm/số lượng" checked={cancelReason === 'Đặt nhầm sản phẩm/số lượng'} onChange={(e) => setCancelReason(e.target.value)} />
                  <span className="cancel-reason-label">Đặt nhầm sản phẩm/số lượng</span>
                </label>
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="Tìm thấy giá rẻ hơn ở nơi khác" checked={cancelReason === 'Tìm thấy giá rẻ hơn ở nơi khác'} onChange={(e) => setCancelReason(e.target.value)} />
                  <span className="cancel-reason-label">Tìm thấy giá rẻ hơn ở nơi khác</span>
                </label>
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="other" checked={cancelReason === 'other'} onChange={(e) => setCancelReason(e.target.value)} />
                  <span className="cancel-reason-label">Lý do khác...</span>
                </label>
              </div>

              {cancelReason === 'other' && (
                <textarea 
                  className="cancel-reason-input" 
                  placeholder="Nhập lý do của bạn..."
                  value={otherReason}
                  onChange={(e) => setOtherReason(e.target.value)}
                />
              )}
            </div>

            <div className="cancel-modal-footer">
              <button className="btn-cancel-modal-close" onClick={() => setShowCancelModal(false)} disabled={cancelling}>
                Đóng
              </button>
              <button 
                className="btn-cancel-modal-submit" 
                onClick={handleCancelOrder}
                disabled={cancelling || !cancelReason || (cancelReason === 'other' && !otherReason.trim())}
              >
                {cancelling ? <i className="fa-solid fa-spinner fa-spin"></i> : <i className="fa-solid fa-check"></i>}
                Xác nhận hủy
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrderDetailPage;
