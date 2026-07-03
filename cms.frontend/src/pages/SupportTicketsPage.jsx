import React, { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import authService from '../services/authService';
import supportService, {
  SUPPORT_CATEGORY_OPTIONS,
  SUPPORT_STATUS_OPTIONS,
} from '../services/supportService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import '../assets/css/Profile.css';
import '../assets/css/OrderDetail.css';
import '../assets/css/SupportTickets.css';

function SupportTicketsPage() {
  const navigate = useNavigate();
  const [customer, setCustomer] = useState(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [keyword, setKeyword] = useState('');
  const [statusTab, setStatusTab] = useState('all');
  const [statusFilter, setStatusFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [page, setPage] = useState(1);
  const [result, setResult] = useState({
    items: [],
    stats: {},
    page: 1,
    totalPages: 1,
    totalItems: 0,
    pageSize: 10,
  });

  useEffect(() => {
    const bootstrap = async () => {
      const stored = getStoredCustomer();
      if (!stored?.customerId || !stored?.accessToken) {
        clearStoredCustomer();
        navigate('/login', { replace: true });
        return;
      }

      try {
        const profile = await authService.getProfile(stored.customerId);
        setCustomer({ ...stored, ...profile });
      } catch {
        setCustomer(stored);
      } finally {
        setLoadingProfile(false);
      }
    };

    bootstrap();
  }, [navigate]);

  useEffect(() => {
    if (loadingProfile || !customer) {
      return;
    }

    let isCancelled = false;
    setLoading(true);
    const timerId = window.setTimeout(() => {
      supportService
        .getCustomerTickets({
          keyword,
          status: statusTab,
          category: categoryFilter,
          page,
          pageSize: 10,
        })
        .then((data) => {
          if (isCancelled) {
            return;
          }

          setResult(data);
          setError('');
        })
        .catch((requestError) => {
          if (isCancelled) {
            return;
          }

          setResult((current) => ({ ...current, items: [], totalItems: 0, totalPages: 1, stats: {} }));
          setError(requestError?.response?.data?.message || 'Không thể tải danh sách yêu cầu hỗ trợ lúc này.');
        })
        .finally(() => {
          if (!isCancelled) {
            setLoading(false);
          }
        });
    }, 180);

    return () => {
      isCancelled = true;
      window.clearTimeout(timerId);
    };
  }, [categoryFilter, customer, keyword, loadingProfile, page, statusFilter, statusTab]);

  const tabs = useMemo(
    () =>
      SUPPORT_STATUS_OPTIONS.map((item) => ({
        ...item,
        count: result.stats?.[item.value] || 0,
      })),
    [result.stats]
  );

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const handleTabChange = (nextStatus) => {
    setStatusTab(nextStatus);
    setStatusFilter(nextStatus === 'all' ? '' : nextStatus);
    setPage(1);
  };

  const paginationItems = Array.from({ length: result.totalPages || 1 }, (_, index) => index + 1);
  const rangeStart = result.totalItems === 0 ? 0 : (result.page - 1) * result.pageSize + 1;
  const rangeEnd = result.totalItems === 0 ? 0 : Math.min(result.page * result.pageSize, result.totalItems);

  return (
    <div className="account-order-page support-page-bg">
      <div className="account-order-layout">
        <AccountSidebar
          activeKey="support"
          customer={customer}
          onLogout={handleLogout}
          badges={{ support: result.stats?.unreadTickets || 0 }}
        />

        <section className="order-detail-main-card support-shell animate--fade-in">
          <div className="support-page-head">
            <div>
              <h1 className="order-detail-title">Hỗ trợ khách hàng</h1>
              <p className="order-detail-description">
                Theo dõi các yêu cầu hỗ trợ, trao đổi với nhân viên và xem lại toàn bộ lịch sử xử lý.
              </p>
            </div>
            <Link to="/account/support/new" className="support-primary-btn">
              <i className="fa-solid fa-plus"></i>
              Tạo yêu cầu hỗ trợ
            </Link>
          </div>

          <div className="support-tabs-row">
            {tabs.map((tab) => (
              <button
                key={tab.value}
                type="button"
                className={`support-tab-btn ${statusTab === tab.value ? 'active' : ''}`}
                onClick={() => handleTabChange(tab.value)}
              >
                {tab.label} <span>({tab.count})</span>
              </button>
            ))}
          </div>

          <div className="support-toolbar">
            <div className="support-search-box">
              <i className="fa-solid fa-magnifying-glass"></i>
              <input
                type="text"
                value={keyword}
                onChange={(event) => {
                  setKeyword(event.target.value);
                  setPage(1);
                }}
                placeholder="Tìm kiếm theo mã, tiêu đề hoặc đơn hàng..."
              />
            </div>

            <select
              className="support-toolbar-select"
              value={statusFilter}
              onChange={(event) => {
                const nextValue = event.target.value;
                setStatusFilter(nextValue);
                setStatusTab(nextValue || 'all');
                setPage(1);
              }}
            >
              <option value="">Tất cả trạng thái</option>
              {SUPPORT_STATUS_OPTIONS.filter((item) => item.value !== 'all').map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </select>

            <select
              className="support-toolbar-select"
              value={categoryFilter}
              onChange={(event) => {
                setCategoryFilter(event.target.value);
                setPage(1);
              }}
            >
              <option value="">Tất cả loại vấn đề</option>
              {SUPPORT_CATEGORY_OPTIONS.map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </select>

            <button
              type="button"
              className="support-icon-btn"
              onClick={() => {
                setKeyword('');
                setStatusFilter('');
                setStatusTab('all');
                setCategoryFilter('');
                setPage(1);
              }}
              aria-label="Đặt lại bộ lọc"
            >
              <i className="fa-solid fa-filter-circle-xmark"></i>
            </button>
          </div>

          <div className="support-table-card">
            {error && !loading && <div className="support-form-error m-3 mb-0">{error}</div>}
            <div className="support-table-wrap">
              <table className="support-ticket-table">
                <thead>
                  <tr>
                    <th>Mã yêu cầu</th>
                    <th>Tiêu đề</th>
                    <th>Loại vấn đề</th>
                    <th>Đơn hàng / Sản phẩm</th>
                    <th>Trạng thái</th>
                    <th>Cập nhật cuối</th>
                    <th>Chưa đọc</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    <tr>
                      <td colSpan="8" className="support-empty-cell">
                        <div className="support-loading-box">
                          <span className="spinner-border text-danger" role="status"></span>
                          <p>Đang tải yêu cầu hỗ trợ...</p>
                        </div>
                      </td>
                    </tr>
                  ) : result.items.length === 0 ? (
                    <tr>
                      <td colSpan="8" className="support-empty-cell">
                        <div className="support-empty-box">
                          <i className="fa-regular fa-life-ring"></i>
                          <h3>Chưa có yêu cầu phù hợp</h3>
                          <p>Hãy thử đổi bộ lọc hoặc tạo một yêu cầu hỗ trợ mới.</p>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    result.items.map((ticket) => (
                      <tr key={ticket.id}>
                        <td>
                          <Link to={`/account/support/${ticket.id}`} className="support-ticket-code">
                            {ticket.code}
                          </Link>
                        </td>
                        <td>
                          <div className="support-ticket-title">{ticket.subject}</div>
                          <div className="support-ticket-preview">{ticket.lastMessagePreview}</div>
                        </td>
                        <td>{ticket.categoryLabel}</td>
                        <td>
                          {ticket.relatedOrderCode || ticket.relatedProductName ? (
                            <>
                              {ticket.relatedOrderCode && <div className="support-related-order">{ticket.relatedOrderCode}</div>}
                              {ticket.relatedProductName && <div className="support-related-product">{ticket.relatedProductName}</div>}
                            </>
                          ) : (
                            <span className="support-muted-line">-</span>
                          )}
                        </td>
                        <td>
                          <span className={`support-status-pill tone-${ticket.statusMeta.tone}`}>{ticket.statusMeta.label}</span>
                        </td>
                        <td>{new Date(ticket.updatedAt).toLocaleString('vi-VN')}</td>
                        <td>
                          {ticket.unreadCount > 0 ? (
                            <span className="support-unread-dot">{ticket.unreadCount}</span>
                          ) : (
                            <span className="support-unread-muted">-</span>
                          )}
                        </td>
                        <td>
                          <Link to={`/account/support/${ticket.id}`} className="support-row-action" aria-label="Xem chi tiết">
                            <i className="fa-solid fa-ellipsis-vertical"></i>
                          </Link>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            <div className="support-table-footer">
              <p>
                Hiển thị {rangeStart}-{rangeEnd} trong tổng số {result.totalItems} yêu cầu
              </p>
              <div className="support-pagination">
                {paginationItems.map((item) => (
                  <button
                    key={item}
                    type="button"
                    className={item === result.page ? 'active' : ''}
                    onClick={() => setPage(item)}
                  >
                    {item}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}

export default SupportTicketsPage;
