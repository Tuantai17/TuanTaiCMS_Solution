import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import '../assets/css/Profile.css';
import '../assets/css/OrderHistory.css';
import orderService from '../services/orderService';
import authService from '../services/authService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import { getMediaUrl } from '../utils/mediaUrl';
import { formatOrderCode, getOrderStatusMeta, ORDER_STATUS_OPTIONS } from '../utils/orderStatus';

const DEFAULT_PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 400;
const FALLBACK_IMAGE = 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image';

function OrderHistory() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const listTopRef = useRef(null);

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
        from: { pathname: '/account/orders' },
      },
    });
  }, [navigate]);

  const [customer, setCustomer] = useState(null);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [fetchingProfile, setFetchingProfile] = useState(true);
  const [error, setError] = useState('');
  const [filterError, setFilterError] = useState('');
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    totalItems: 0,
    totalPages: 0,
  });

  const [keywordInput, setKeywordInput] = useState(searchParams.get('keyword') || '');

  const selectedStatus = searchParams.get('status') || '';
  const keyword = searchParams.get('keyword') || '';
  const fromDate = searchParams.get('fromDate') || '';
  const toDate = searchParams.get('toDate') || '';
  const currentPage = Number(searchParams.get('page') || '1');

  const updateSearchParams = useCallback((updates) => {
    const nextParams = new URLSearchParams(searchParams);

    Object.entries(updates).forEach(([paramKey, paramValue]) => {
      if (paramValue === null || paramValue === undefined || paramValue === '') {
        nextParams.delete(paramKey);
      } else {
        nextParams.set(paramKey, String(paramValue));
      }
    });

    setSearchParams(nextParams, { replace: true });
  }, [searchParams, setSearchParams]);

  useEffect(() => {
    setKeywordInput(keyword);
  }, [keyword]);

  useEffect(() => {
    const storedCustomer = getStoredCustomer();
    if (!storedCustomer?.customerId) {
      redirectToLogin('Vui lòng đăng nhập để xem đơn hàng của tôi.');
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
    const debounceId = window.setTimeout(() => {
      const nextKeyword = keywordInput.trim();
      if (nextKeyword === keyword) {
        return;
      }

      updateSearchParams({
        keyword: nextKeyword || null,
        page: '1',
      });
    }, SEARCH_DEBOUNCE_MS);

    return () => window.clearTimeout(debounceId);
  }, [keywordInput, keyword, updateSearchParams]);

  useEffect(() => {
    if (fetchingProfile || !customer) {
      return;
    }

    const loadOrders = async () => {
      if (fromDate && toDate && fromDate > toDate) {
        setFilterError('Ngày bắt đầu không được lớn hơn ngày kết thúc.');
        setOrders([]);
        setPagination((prev) => ({ ...prev, page: currentPage, totalItems: 0, totalPages: 0 }));
        setLoading(false);
        return;
      }

      setFilterError('');
      setError('');
      setLoading(true);

      try {
        const response = await orderService.getMyOrders({
          status: selectedStatus || undefined,
          keyword: keyword || undefined,
          fromDate: fromDate || undefined,
          toDate: toDate || undefined,
          page: currentPage,
          pageSize: DEFAULT_PAGE_SIZE,
        });

        setOrders(response?.items || []);
        setPagination({
          page: response?.page || 1,
          pageSize: response?.pageSize || DEFAULT_PAGE_SIZE,
          totalItems: response?.totalItems || 0,
          totalPages: response?.totalPages || 0,
        });
      } catch (requestError) {
        if (requestError?.response?.status === 401) {
          clearStoredCustomer();
          window.dispatchEvent(new Event('customerLoginStateChange'));
          redirectToLogin('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại để xem đơn hàng của tôi.');
          return;
        }

        setOrders([]);
        setPagination((prev) => ({ ...prev, totalItems: 0, totalPages: 0 }));
        setError(requestError?.response?.data?.message || 'Không thể tải lịch sử mua hàng. Vui lòng thử lại.');
      } finally {
        setLoading(false);
      }
    };

    loadOrders();
  }, [customer, currentPage, fetchingProfile, fromDate, keyword, redirectToLogin, selectedStatus, toDate]);

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const handleStatusChange = (statusValue) => {
    updateSearchParams({
      status: statusValue || null,
      page: '1',
    });
  };

  const handleDateChange = (key, value) => {
    updateSearchParams({
      [key]: value || null,
      page: '1',
    });
  };

  const handleResetFilters = () => {
    setKeywordInput('');
    setSearchParams(new URLSearchParams(), { replace: true });
  };

  const handlePageChange = (page) => {
    updateSearchParams({ page });
    listTopRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  const paginationItems = useMemo(() => {
    if (pagination.totalPages <= 1) {
      return [];
    }

    const totalPages = pagination.totalPages;
    const page = pagination.page;
    const pages = new Set([1, totalPages, page - 1, page, page + 1]);
    const sortedPages = [...pages].filter((value) => value >= 1 && value <= totalPages).sort((a, b) => a - b);

    const items = [];
    let previous = 0;

    sortedPages.forEach((value) => {
      if (previous && value - previous > 1) {
        items.push(`ellipsis-${previous}`);
      }
      items.push(value);
      previous = value;
    });

    return items;
  }, [pagination.page, pagination.totalPages]);

  const summaryText = useMemo(() => {
    if (!pagination.totalItems) {
      return 'Hiển thị 0 đơn hàng';
    }

    const start = (pagination.page - 1) * pagination.pageSize + 1;
    const end = Math.min(start + orders.length - 1, pagination.totalItems);
    return `Hiển thị ${start}-${end} trong tổng số ${pagination.totalItems} đơn hàng`;
  }, [orders.length, pagination.page, pagination.pageSize, pagination.totalItems]);

  const hasActiveFilters = Boolean(selectedStatus || keyword || fromDate || toDate);
  const isEmpty = !loading && !error && orders.length === 0;

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

  const renderOrderActions = (order) => (
    <div className="order-history-actions">
      <button
        type="button"
        className="order-history-secondary-btn"
        onClick={() => navigate(`/account/orders/${order.id}`)}
      >
        Xem chi tiết
      </button>
    </div>
  );

  const renderEmptyState = () => {
    if (hasActiveFilters) {
      return (
        <div className="order-history-empty">
          <div className="order-history-empty-icon">
            <i className="fa-solid fa-filter-circle-xmark"></i>
          </div>
          <h3>Không tìm thấy đơn hàng phù hợp</h3>
          <p>Hãy thử thay đổi bộ lọc, khoảng thời gian hoặc từ khóa tìm kiếm của bạn.</p>
          <button type="button" className="order-history-reset-btn" onClick={handleResetFilters}>
            Xóa bộ lọc
          </button>
        </div>
      );
    }

    return (
      <div className="order-history-empty">
        <div className="order-history-empty-icon">
          <i className="fa-solid fa-box-open"></i>
        </div>
        <h3>Bạn chưa có đơn hàng nào</h3>
        <p>Hãy khám phá sản phẩm và bắt đầu mua sắm để theo dõi lịch sử đơn hàng tại đây.</p>
        <Link to="/products" className="order-history-shop-btn">
          Tiếp tục mua sắm
        </Link>
      </div>
    );
  };

  return (
    <div className="order-history-page">
      <div className="order-history-layout">
        <AccountSidebar activeKey="order-history" customer={customer} onLogout={handleLogout} />

        <section className="order-history-content" ref={listTopRef}>
          <div className="order-history-card">
            <div className="order-history-title-row">
              <div>
                <h1 className="order-history-title">Đơn hàng của tôi</h1>
                <p className="order-history-subtitle">Theo dõi toàn bộ đơn hàng đã mua và đang xử lý của bạn.</p>
              </div>
            </div>

            <div className="order-history-status-tabs" role="tablist" aria-label="Lọc trạng thái đơn hàng">
              {ORDER_STATUS_OPTIONS.map((option) => (
                <button
                  key={option.key}
                  type="button"
                  className={`order-history-status-tab tone-${option.tone} ${selectedStatus === option.value ? 'is-active' : ''}`}
                  onClick={() => handleStatusChange(option.value)}
                >
                  <i className={option.icon} aria-hidden="true"></i>
                  <span>{option.label}</span>
                </button>
              ))}
            </div>

            <div className="order-history-filter-card">
              <div className="order-history-filter-row">
                <div className="order-history-field">
                  <label htmlFor="orderHistoryKeyword">Tìm kiếm theo mã đơn hàng</label>
                  <input
                    id="orderHistoryKeyword"
                    className="order-history-input"
                    type="text"
                    placeholder="Tìm kiếm theo mã đơn hàng..."
                    value={keywordInput}
                    onChange={(event) => setKeywordInput(event.target.value)}
                    aria-label="Tìm kiếm theo mã đơn hàng"
                  />
                </div>

                <div className="order-history-field">
                  <label htmlFor="orderHistoryFromDate">Từ ngày</label>
                  <input
                    id="orderHistoryFromDate"
                    className="order-history-date"
                    type="date"
                    value={fromDate}
                    max={toDate || undefined}
                    onChange={(event) => handleDateChange('fromDate', event.target.value)}
                  />
                </div>

                <div className="order-history-field">
                  <label htmlFor="orderHistoryToDate">Đến ngày</label>
                  <input
                    id="orderHistoryToDate"
                    className="order-history-date"
                    type="date"
                    value={toDate}
                    min={fromDate || undefined}
                    onChange={(event) => handleDateChange('toDate', event.target.value)}
                  />
                </div>

                <button type="button" className="order-history-reset-btn" onClick={handleResetFilters}>
                  Đặt lại
                </button>
              </div>
            </div>

            {filterError && <div className="order-history-filter-error">{filterError}</div>}
            {error && <div className="order-history-error-box">{error}</div>}
            {loading && <div className="order-history-loading-note">Đang tải danh sách đơn hàng...</div>}

            {!error && (
              <>
                <div className="order-history-table-wrap">
                  <table className="order-history-table">
                    <thead>
                      <tr>
                        <th>Mã đơn hàng</th>
                        <th>Ngày đặt</th>
                        <th>Sản phẩm</th>
                        <th>Tổng tiền</th>
                        <th>Phương thức thanh toán</th>
                        <th>Trạng thái</th>
                        <th>Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      {orders.map((order) => {
                        const statusMeta = getOrderStatusMeta(order.status);
                        const otherProductCount = Math.max((order.productCount || 0) - 1, 0);

                        return (
                          <tr key={order.id}>
                            <td>
                              <div className="order-history-code">{formatOrderCode(order.id)}</div>
                              <button
                                type="button"
                                className="order-history-link-btn"
                                onClick={() => navigate(`/account/orders/${order.id}`)}
                              >
                                Xem chi tiết
                              </button>
                            </td>
                            <td>
                              <span className="order-history-date-text">
                                {new Date(order.orderDate).toLocaleDateString('vi-VN')}
                              </span>
                              <span className="order-history-time-text">
                                {new Date(order.orderDate).toLocaleTimeString('vi-VN', {
                                  hour: '2-digit',
                                  minute: '2-digit',
                                })}
                              </span>
                            </td>
                            <td>
                              <div className="order-history-product">
                                <img
                                  src={getMediaUrl(order.firstProductImageUrl, FALLBACK_IMAGE)}
                                  alt={order.firstProductName || 'Sản phẩm'}
                                  onError={(event) => {
                                    event.currentTarget.src = FALLBACK_IMAGE;
                                  }}
                                />
                                <div>
                                  <div className="order-history-product-name">{order.firstProductName || 'Sản phẩm không xác định'}</div>
                                  <div className="order-history-product-meta">Số lượng: {order.totalQuantity || 0}</div>
                                  {otherProductCount > 0 && (
                                    <div className="order-history-product-meta">Và {otherProductCount} sản phẩm khác</div>
                                  )}
                                </div>
                              </div>
                            </td>
                            <td>
                              <div className="order-history-price">
                                {Number(order.totalAmount || 0).toLocaleString('vi-VN')} ₫
                              </div>
                            </td>
                            <td>
                              <span className="order-history-payment">{order.paymentMethod || 'Không xác định'}</span>
                              <span className="order-history-payment-sub">
                                Hệ thống hiện chưa lưu chi tiết phương thức thanh toán cho đơn này
                              </span>
                            </td>
                            <td>
                              <span className={`order-status-badge ${statusMeta.badgeClass}`}>
                                <i className={statusMeta.icon} aria-hidden="true"></i>
                                {statusMeta.label}
                              </span>
                            </td>
                            <td>{renderOrderActions(order)}</td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>

                <div className="order-history-mobile-list">
                  {orders.map((order) => {
                    const statusMeta = getOrderStatusMeta(order.status);
                    const otherProductCount = Math.max((order.productCount || 0) - 1, 0);

                    return (
                      <article className="order-history-mobile-card" key={order.id}>
                        <div className="order-history-mobile-top">
                          <div>
                            <div className="order-history-mobile-code">{formatOrderCode(order.id)}</div>
                            <div className="order-history-mobile-date">
                              {new Date(order.orderDate).toLocaleString('vi-VN')}
                            </div>
                          </div>
                          <span className={`order-status-badge ${statusMeta.badgeClass}`}>
                            <i className={statusMeta.icon} aria-hidden="true"></i>
                            {statusMeta.label}
                          </span>
                        </div>

                        <div className="order-history-mobile-main">
                          <img
                            src={getMediaUrl(order.firstProductImageUrl, FALLBACK_IMAGE)}
                            alt={order.firstProductName || 'Sản phẩm'}
                            onError={(event) => {
                              event.currentTarget.src = FALLBACK_IMAGE;
                            }}
                          />
                          <div>
                            <div className="order-history-mobile-name">{order.firstProductName || 'Sản phẩm không xác định'}</div>
                            <div className="order-history-mobile-meta">Số lượng: {order.totalQuantity || 0}</div>
                            {otherProductCount > 0 && (
                              <div className="order-history-mobile-meta">Và {otherProductCount} sản phẩm khác</div>
                            )}
                            <div className="order-history-mobile-meta">Thanh toán: {order.paymentMethod || 'Không xác định'}</div>
                          </div>
                        </div>

                        <div className="order-history-mobile-bottom">
                          <div className="order-history-price">
                            {Number(order.totalAmount || 0).toLocaleString('vi-VN')} ₫
                          </div>
                        </div>

                        <div className="order-history-mobile-actions">{renderOrderActions(order)}</div>
                      </article>
                    );
                  })}
                </div>

                {loading && (
                  <div className="order-history-skeleton-list" aria-live="polite">
                    {[1, 2, 3].map((item) => (
                      <div key={item} className="order-history-skeleton-row"></div>
                    ))}
                  </div>
                )}

                {isEmpty && renderEmptyState()}

                {!isEmpty && !loading && orders.length > 0 && (
                  <div className="order-history-footer">
                    <div className="order-history-mobile-meta">{summaryText}</div>
                    {pagination.totalPages > 1 && (
                      <div className="order-history-pagination" aria-label="Phân trang đơn hàng">
                        <button
                          type="button"
                          className="order-history-page-btn"
                          onClick={() => handlePageChange(Math.max(pagination.page - 1, 1))}
                          disabled={pagination.page <= 1}
                          aria-label="Trang trước"
                        >
                          <i className="fa-solid fa-chevron-left"></i>
                        </button>

                        {paginationItems.map((item) =>
                          typeof item === 'string' ? (
                            <span key={item} className="order-history-ellipsis">
                              ...
                            </span>
                          ) : (
                            <button
                              key={item}
                              type="button"
                              className={`order-history-page-btn ${pagination.page === item ? 'is-active' : ''}`}
                              onClick={() => handlePageChange(item)}
                              aria-label={`Trang ${item}`}
                            >
                              {item}
                            </button>
                          )
                        )}

                        <button
                          type="button"
                          className="order-history-page-btn"
                          onClick={() => handlePageChange(Math.min(pagination.page + 1, pagination.totalPages))}
                          disabled={pagination.page >= pagination.totalPages}
                          aria-label="Trang sau"
                        >
                          <i className="fa-solid fa-chevron-right"></i>
                        </button>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
          </div>
        </section>
      </div>
    </div>
  );
}

export default OrderHistory;
