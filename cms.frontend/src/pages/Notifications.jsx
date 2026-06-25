import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Header from '../components/Header';
import Footer from '../components/Footer';
import AccountSidebar from '../components/account/AccountSidebar';
import notificationService from '../services/notificationService';

const Notifications = () => {
  const [notifications, setNotifications] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const customer = localStorage.getItem('customer');
    if (!customer) {
      navigate('/login');
      return;
    }
    fetchNotifications(page);
  }, [page, navigate]);

  const fetchNotifications = async (currentPage) => {
    setLoading(true);
    try {
      const res = await notificationService.getNotifications(currentPage, 10);
      setNotifications(res.items || []);
      setTotalPages(res.totalPages || 0);
    } catch (error) {
      console.error('Error fetching notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleMarkAsRead = async (id) => {
    try {
      await notificationService.markAsRead(id);
      // Update local state
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
      // Trigger header to update
      window.dispatchEvent(new Event('customerLoginStateChange')); // lazy way to force re-fetch in Header if needed, or better, just rely on Header's own interval if any.
    } catch (error) {
      console.error('Lỗi khi đánh dấu đã đọc:', error);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await notificationService.markAllAsRead();
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch (error) {
      console.error('Lỗi khi đánh dấu tất cả đã đọc:', error);
    }
  };

  return (
    <div className="bg-light py-4" style={{ minHeight: 'calc(100vh - 400px)' }}>
      <div className="container">
        <div className="row">
          <div className="col-lg-3 mb-4 mb-lg-0">
            <AccountSidebar />
          </div>
          
          <div className="col-lg-9">
            <div className="card border-0 shadow-sm rounded-lg">
              <div className="card-header bg-white border-bottom d-flex justify-content-between align-items-center py-3">
                <h5 className="mb-0 font-weight-bold"><i className="fa-regular fa-bell text-danger mr-2"></i> Thông báo của bạn</h5>
                <button className="btn btn-sm btn-outline-secondary" onClick={handleMarkAllAsRead}>
                  <i className="fa-solid fa-check-double mr-1"></i> Đánh dấu tất cả đã đọc
                </button>
              </div>
              
              <div className="card-body p-0">
                {loading ? (
                  <div className="text-center p-5">
                    <div className="spinner-border text-danger" role="status">
                      <span className="sr-only">Đang tải...</span>
                    </div>
                  </div>
                ) : notifications.length > 0 ? (
                  <ul className="list-group list-group-flush">
                    {notifications.map(notification => (
                      <li 
                        key={notification.id} 
                        className={`list-group-item p-4 ${!notification.isRead ? 'bg-light border-left border-danger' : ''}`}
                        style={{ borderLeftWidth: !notification.isRead ? '4px !important' : '0' }}
                      >
                        <div className="d-flex justify-content-between align-items-start">
                          <div>
                            <h6 className={`mb-1 ${!notification.isRead ? 'font-weight-bold text-dark' : 'text-secondary'}`}>
                              {notification.referenceType === 'Order' && notification.referenceId ? (
                                <Link to={`/account/orders/${notification.referenceId}`} style={{ color: 'inherit', textDecoration: 'none' }}>
                                  {notification.title}
                                </Link>
                              ) : (
                                <span>{notification.title}</span>
                              )}
                              {!notification.isRead && <span className="badge badge-danger ml-2">Mới</span>}
                            </h6>
                            <p className="mb-1 text-muted" style={{ fontSize: '0.9rem' }}>{notification.message}</p>
                            <small className="text-secondary"><i className="fa-regular fa-clock mr-1"></i> {new Date(notification.createdAt).toLocaleString('vi-VN')}</small>
                          </div>
                          
                          {!notification.isRead && (
                            <button 
                              className="btn btn-sm btn-light text-primary" 
                              onClick={() => handleMarkAsRead(notification.id)}
                              title="Đánh dấu đã đọc"
                            >
                              <i className="fa-solid fa-check"></i>
                            </button>
                          )}
                        </div>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <div className="text-center p-5">
                    <img src="https://cdn-icons-png.flaticon.com/512/1044/1044003.png" alt="No notifications" style={{ width: '80px', opacity: 0.5, marginBottom: '20px' }} />
                    <h6 className="text-muted">Bạn chưa có thông báo nào</h6>
                  </div>
                )}
              </div>

              {totalPages > 1 && (
                <div className="card-footer bg-white border-top py-3">
                  <nav aria-label="Page navigation">
                    <ul className="pagination justify-content-center mb-0">
                      <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                        <button className="page-link" onClick={() => setPage(p => Math.max(1, p - 1))}>Trước</button>
                      </li>
                      {[...Array(totalPages)].map((_, i) => (
                        <li key={i} className={`page-item ${page === i + 1 ? 'active' : ''}`}>
                          <button className="page-link" onClick={() => setPage(i + 1)}>{i + 1}</button>
                        </li>
                      ))}
                      <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
                        <button className="page-link" onClick={() => setPage(p => Math.min(totalPages, p + 1))}>Sau</button>
                      </li>
                    </ul>
                  </nav>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Notifications;
