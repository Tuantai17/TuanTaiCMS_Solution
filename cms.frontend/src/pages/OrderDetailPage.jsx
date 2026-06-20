import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import '../assets/css/Profile.css';
import '../assets/css/OrderHistory.css';
import orderService from '../services/orderService';
import authService from '../services/authService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import { getMediaUrl } from '../utils/mediaUrl';
import { formatOrderCode, getOrderStatusMeta } from '../utils/orderStatus';

const FALLBACK_IMAGE = 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image';

function OrderDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();

  const [customer, setCustomer] = useState(null);
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [fetchingProfile, setFetchingProfile] = useState(true);
  const [error, setError] = useState('');

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
      redirectToLogin('Phiên đăng nhập cũ không còn phù hợp với bảo mật mới của trang đơn hàng. Vui lòng đăng nhập lại để tiếp tục.');
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

  if (fetchingProfile) {
    return (
      <div className="order-history-page">
        <div className="profile-skeleton">
          <div className="profile-skeleton-sidebar">
            <div className="skeleton-block" style={{ height: '320px' }}></div>
          </div>
          <div className="profile-skeleton-content">
            <div className="skeleton-block" style={{ height: '520px' }}></div>
          </div>
        </div>
      </div>
    );
  }

  const statusMeta = order ? getOrderStatusMeta(order.status) : getOrderStatusMeta(-1);

  return (
    <div className="order-history-page order-detail-page">
      <div className="order-history-layout">
        <AccountSidebar activeKey="order-history" customer={customer} onLogout={handleLogout} />

        <section className="order-history-content">
          <div className="order-detail-card">
            <div className="order-detail-title-row">
              <div>
                <h1 className="order-detail-title">Chi tiết đơn hàng</h1>
                <p className="order-detail-subtitle">Theo dõi thông tin và sản phẩm trong đơn hàng của bạn.</p>
              </div>
              <button type="button" className="order-history-reset-btn" onClick={() => navigate('/account/orders')}>
                Quay lại lịch sử
              </button>
            </div>

            {loading ? (
              <div className="order-detail-loading">
                <div className="order-detail-loading-icon">
                  <i className="fa-solid fa-spinner fa-spin"></i>
                </div>
                <h3>Đang tải chi tiết đơn hàng</h3>
                <p>Vui lòng đợi trong giây lát để hệ thống lấy dữ liệu từ backend.</p>
              </div>
            ) : error ? (
              <div className="order-history-not-found">
                <div className="order-history-not-found-icon">
                  <i className="fa-regular fa-circle-xmark"></i>
                </div>
                <h3>{error}</h3>
                <p>Đơn hàng này có thể không tồn tại hoặc không thuộc quyền truy cập của tài khoản hiện tại.</p>
                <Link to="/account/orders" className="order-history-shop-btn">
                  Về lịch sử mua hàng
                </Link>
              </div>
            ) : (
              <>
                <div className="order-detail-grid">
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Mã đơn hàng</div>
                    <div className="order-detail-summary-value">{formatOrderCode(order.id)}</div>
                  </div>
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Ngày đặt</div>
                    <div className="order-detail-summary-value">{new Date(order.orderDate).toLocaleString('vi-VN')}</div>
                  </div>
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Tổng tiền</div>
                    <div className="order-detail-summary-value price">
                      {Number(order.totalAmount || 0).toLocaleString('vi-VN')} ₫
                    </div>
                  </div>
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Trạng thái</div>
                    <span className={`order-status-badge ${statusMeta.badgeClass}`}>
                      <i className={statusMeta.icon} aria-hidden="true"></i>
                      {statusMeta.label}
                    </span>
                  </div>
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Phương thức thanh toán</div>
                    <div className="order-detail-summary-value">{order.paymentMethod || 'Không xác định'}</div>
                  </div>
                  <div className="order-detail-summary-box">
                    <div className="order-detail-summary-label">Ghi chú</div>
                    <div className="order-detail-summary-value">{order.notes || 'Không có ghi chú'}</div>
                  </div>
                </div>

                <div className="order-detail-items">
                  {order.items.map((item) => (
                    <article className="order-detail-item" key={item.id}>
                      <img
                        src={getMediaUrl(item.productImageUrl, FALLBACK_IMAGE)}
                        alt={item.productName}
                        className="order-detail-item-image"
                        onError={(event) => {
                          event.currentTarget.src = FALLBACK_IMAGE;
                        }}
                      />
                      <div>
                        <div className="order-detail-item-name">{item.productName}</div>
                        <div className="order-detail-muted">Số lượng: {item.quantity}</div>
                        <div className="order-detail-muted">
                          Đơn giá: {Number(item.unitPrice || 0).toLocaleString('vi-VN')} ₫
                        </div>
                      </div>
                      <div className="order-detail-item-total">
                        <div className="order-detail-muted">Thành tiền</div>
                        <div className="order-detail-total-value">
                          {Number(item.lineTotal || 0).toLocaleString('vi-VN')} ₫
                        </div>
                      </div>
                    </article>
                  ))}
                </div>
              </>
            )}
          </div>
        </section>
      </div>
    </div>
  );
}

export default OrderDetailPage;
