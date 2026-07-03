import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import ReviewFormModal from '../components/reviews/ReviewFormModal';
import '../assets/css/Profile.css';
import '../assets/css/OrderDetail.css';
import '../assets/css/ProductReviews.css';
import orderService from '../services/orderService';
import authService from '../services/authService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import { getMediaUrl } from '../utils/mediaUrl';
import { formatOrderCode, parseOrderNotes } from '../utils/orderStatus';

const FALLBACK_IMAGE = 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image';

const ORDER_STATUS_CONFIG = {
  0: { label: 'Chờ duyệt', step: 1 },
  1: { label: 'Đã duyệt', step: 2 },
  2: { label: 'Đang chuẩn bị', step: 3 },
  3: { label: 'Đang giao hàng', step: 4 },
  4: { label: 'Hoàn thành', step: 5 },
  5: { label: 'Đã hủy', step: -1 },
  6: { label: 'Chờ khách xác nhận', step: 3 },
  7: { label: 'Chờ bổ sung hàng', step: 3 },
};

function OrderDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();

  const [customer, setCustomer] = useState(null);
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [fetchingProfile, setFetchingProfile] = useState(true);
  const [error, setError] = useState('');
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [otherReason, setOtherReason] = useState('');
  const [cancelling, setCancelling] = useState(false);
  const [reviewingItem, setReviewingItem] = useState(null);

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
    if (!storedCustomer?.customerId || !storedCustomer?.accessToken) {
      redirectToLogin('Vui lòng đăng nhập để xem chi tiết đơn hàng.');
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
      try {
        setLoading(true);
        setError('');
        const response = await orderService.getMyOrderDetail(id);
        setOrder(response);
      } catch (requestError) {
        if (requestError?.response?.status === 401) {
          redirectToLogin('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
          return;
        }

        setError(requestError?.response?.data?.message || 'Không thể tải chi tiết đơn hàng.');
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
    if (!order?.items?.length) {
      return;
    }

    const cart = JSON.parse(localStorage.getItem('cart') || '[]');

    order.items.forEach((item) => {
      const existingIndex = cart.findIndex((cartItem) => cartItem.id === item.productId);
      if (existingIndex >= 0) {
        cart[existingIndex].quantity += item.quantity;
        cart[existingIndex].price = item.unitPrice;
      } else {
        cart.push({
          id: item.productId,
          name: item.productName,
          price: item.unitPrice,
          quantity: item.quantity,
          imageUrl: getMediaUrl(item.productImageUrl),
          sku: `SKU${1000 + item.productId}`,
        });
      }
    });

    localStorage.setItem('cart', JSON.stringify(cart));
    window.dispatchEvent(new Event('cartChange'));
    navigate('/cart');
  };

  const handleCancelOrder = async () => {
    const finalReason = cancelReason === 'other' ? otherReason : cancelReason;
    if (!finalReason.trim()) {
      return;
    }

    try {
      setCancelling(true);
      await orderService.cancelMyOrder(order.id, finalReason);
      setOrder((current) => (current ? { ...current, status: 5 } : current));
      setShowCancelModal(false);
    } catch (requestError) {
      alert(requestError?.response?.data?.message || 'Không thể hủy đơn hàng. Vui lòng thử lại.');
    } finally {
      setCancelling(false);
    }
  };

  const handleReviewSubmitted = (review) => {
    setOrder((current) => {
      if (!current) {
        return current;
      }

      return {
        ...current,
        items: current.items.map((item) =>
          item.id === review.orderDetailId
            ? {
                ...item,
                canReview: false,
                hasReview: true,
                reviewId: review.id,
                reviewStatus: review.status,
              }
            : item
        ),
      };
    });
  };

  const parsedNotes = order ? parseOrderNotes(order.notes) : { deliveryInfo: '', paymentMethod: '', customerNotes: '' };
  const deliveryParts = parsedNotes.deliveryInfo ? parsedNotes.deliveryInfo.split(' - ') : [];
  const currentStatus = ORDER_STATUS_CONFIG[order?.status] || ORDER_STATUS_CONFIG[0];

  return (
    <div className="account-order-page">
      <div className="account-order-layout">
        <AccountSidebar activeKey="order-history" customer={customer} onLogout={handleLogout} />

        <section className="order-detail-main-card animate--fade-in">
          {fetchingProfile || loading ? (
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
          ) : error || !order ? (
            <div className="order-history-not-found" style={{ textAlign: 'center', padding: '60px 0' }}>
              <div style={{ fontSize: '48px', color: '#dc2626', marginBottom: '16px' }}>
                <i className="fa-solid fa-triangle-exclamation"></i>
              </div>
              <h3>{error || 'Không tìm thấy đơn hàng.'}</h3>
              <Link to="/account/orders" className="btn-back-list mt-3">
                Quay lại danh sách đơn
              </Link>
            </div>
          ) : (
            <>
              <div className="order-detail-header">
                <div>
                  <h1 className="order-detail-title">Chi tiết đơn hàng</h1>
                  <p className="order-detail-description">Theo dõi thông tin, trạng thái và sản phẩm trong đơn hàng của bạn.</p>
                </div>
                <Link to="/account/orders" className="order-back-btn">
                  <i className="fa-solid fa-chevron-left" style={{ fontSize: '12px' }}></i> Quay lại lịch sử
                </Link>
              </div>

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
                  <div className="overview-val" style={{ color: currentStatus.step === 5 ? '#16a34a' : currentStatus.step === -1 ? '#dc2626' : '#d97706' }}>
                    {currentStatus.label}
                  </div>
                </div>
                <div className="order-overview-card">
                  <div className="overview-lbl"><i className="fa-solid fa-money-bill-wave"></i> Tổng tiền</div>
                  <div className="overview-val price">{Number(order.totalAmount || 0).toLocaleString('vi-VN')}đ</div>
                </div>
              </div>

              <div className="order-information-grid">
                <div className="info-card">
                  <div className="info-card-title">
                    <i className="fa-solid fa-location-dot"></i> Thông tin giao hàng
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Người nhận</div>
                    <div className="info-val">{deliveryParts[0] || customer?.fullName || 'Khách hàng'}</div>
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Số điện thoại</div>
                    <div className="info-val">{deliveryParts[1] || customer?.phone || 'Chưa cập nhật'}</div>
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Email</div>
                    <div className="info-val">{deliveryParts[2] || customer?.email || 'Chưa cập nhật'}</div>
                  </div>
                  <div className="shipping-info-row">
                    <div className="info-lbl">Địa chỉ</div>
                    <div className="info-val">{deliveryParts.slice(3).join(' - ') || customer?.address || 'Chưa cập nhật'}</div>
                  </div>
                </div>

                <div className="info-card">
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
                      {order.status === 4 ? (
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

              <div className="order-products-section">
                <h3 className="order-products-title">
                  <i className="fa-solid fa-box-open" style={{ color: '#d71920' }}></i> Sản phẩm trong đơn hàng
                </h3>

                <div className="order-product-grid product-grid-header">
                  <div>Sản phẩm</div>
                  <div style={{ textAlign: 'right' }}>Đơn giá</div>
                  <div style={{ textAlign: 'center' }}>Số lượng</div>
                  <div style={{ textAlign: 'right' }}>Thành tiền</div>
                  <div style={{ textAlign: 'right' }}>Đánh giá</div>
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
                        <Link to={`/products/${item.productId}`} className="product-name">
                          {item.productName}
                        </Link>
                        <div className="product-sku">SKU{1000 + item.productId}</div>
                      </div>
                    </div>

                    <div className="product-price-col">{Number(item.unitPrice || 0).toLocaleString('vi-VN')}đ</div>
                    <div className="product-qty-col">x{item.quantity}</div>
                    <div className="product-total-col">{Number(item.lineTotal || 0).toLocaleString('vi-VN')}đ</div>
                    <div className="product-total-col">
                      {item.canReview ? (
                        <button type="button" className="review-action-btn review-action-btn--primary" onClick={() => setReviewingItem(item)}>
                          <i className="fa-solid fa-star"></i> Đánh giá sản phẩm
                        </button>
                      ) : item.hasReview ? (
                        <div className="review-action-stack">
                          <span className="review-action-state"><i className="fa-solid fa-circle-check"></i> Đã gửi đánh giá</span>
                          <Link to="/account/reviews" className="review-action-btn">Xem đánh giá</Link>
                        </div>
                      ) : (
                        <span className="text-muted small">Chỉ đánh giá sau khi đơn hoàn thành</span>
                      )}
                    </div>
                  </div>
                ))}
              </div>

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

              <div className="order-actions-row">
                <button className="btn-support" onClick={() => navigate(`/account/support/new?orderId=${order.id}`)}>
                  <i className="fa-solid fa-headset"></i> Liên hệ hỗ trợ
                </button>
                {order.status <= 1 && (
                  <button
                    className="btn-cancel-order"
                    onClick={() => {
                      setCancelReason('');
                      setOtherReason('');
                      setShowCancelModal(true);
                    }}
                  >
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

      <ReviewFormModal
        open={Boolean(reviewingItem)}
        item={reviewingItem}
        onClose={() => setReviewingItem(null)}
        onSubmitted={handleReviewSubmitted}
      />

      {showCancelModal && (
        <div className="cancel-modal-overlay" onClick={() => !cancelling && setShowCancelModal(false)}>
          <div className="cancel-modal-content" onClick={(event) => event.stopPropagation()}>
            <div className="cancel-modal-header">
              <h3><i className="fa-solid fa-triangle-exclamation"></i> Xác nhận hủy đơn hàng</h3>
              <button className="close-modal-btn" onClick={() => !cancelling && setShowCancelModal(false)}>
                <i className="fa-solid fa-xmark"></i>
              </button>
            </div>

            <div className="cancel-modal-body">
              <p>Vui lòng cho chúng tôi biết lý do bạn muốn hủy đơn hàng này:</p>
              <div className="cancel-reason-list">
                {[
                  'Muốn thay đổi địa chỉ giao hàng',
                  'Đổi ý, không muốn mua nữa',
                  'Đặt nhầm sản phẩm/số lượng',
                  'Tìm thấy giá rẻ hơn ở nơi khác',
                ].map((reason) => (
                  <label className="cancel-reason-item" key={reason}>
                    <input type="radio" name="cancelReason" value={reason} checked={cancelReason === reason} onChange={(event) => setCancelReason(event.target.value)} />
                    <span className="cancel-reason-label">{reason}</span>
                  </label>
                ))}
                <label className="cancel-reason-item">
                  <input type="radio" name="cancelReason" value="other" checked={cancelReason === 'other'} onChange={(event) => setCancelReason(event.target.value)} />
                  <span className="cancel-reason-label">Lý do khác...</span>
                </label>
              </div>

              {cancelReason === 'other' && (
                <textarea
                  className="cancel-reason-input"
                  placeholder="Nhập lý do của bạn..."
                  value={otherReason}
                  onChange={(event) => setOtherReason(event.target.value)}
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
