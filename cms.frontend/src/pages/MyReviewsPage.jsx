import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import ReviewStars from '../components/reviews/ReviewStars';
import authService from '../services/authService';
import reviewService from '../services/reviewService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import { getMediaUrl } from '../utils/mediaUrl';
import '../assets/css/Profile.css';
import '../assets/css/ProductReviews.css';

const STATUS_TABS = [
  { label: 'Tất cả', value: '' },
  { label: 'Chờ duyệt', value: 'Pending' },
  { label: 'Đã công khai', value: 'Published' },
  { label: 'Đã ẩn', value: 'Hidden' },
  { label: 'Đã từ chối', value: 'Rejected' },
];

function MyReviewsPage() {
  const navigate = useNavigate();
  const [customer, setCustomer] = useState(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [result, setResult] = useState({ items: [], totalPages: 0, page: 1 });

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

    const loadReviews = async () => {
      try {
        setLoading(true);
        setError('');
        const data = await reviewService.getMyReviews({
          status: statusFilter || undefined,
          page: 1,
          pageSize: 10,
        });
        setResult(data);
      } catch (requestError) {
        setError(requestError?.response?.data?.message || 'Không thể tải đánh giá của bạn.');
      } finally {
        setLoading(false);
      }
    };

    loadReviews();
  }, [customer, loadingProfile, statusFilter]);

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  return (
    <div className="account-order-page">
      <div className="account-order-layout">
        <AccountSidebar activeKey="my-reviews" customer={customer} onLogout={handleLogout} />

        <section className="order-detail-main-card animate--fade-in">
          <div className="order-detail-header">
            <div>
              <h1 className="order-detail-title">Đánh giá của tôi</h1>
              <p className="order-detail-description">Quản lý các đánh giá bạn đã gửi cho sản phẩm.</p>
            </div>
          </div>

          <div className="product-review-filters account-review-tabs">
            {STATUS_TABS.map((tab) => (
              <button
                key={tab.value || 'all'}
                type="button"
                className={statusFilter === tab.value ? 'active' : ''}
                onClick={() => setStatusFilter(tab.value)}
              >
                {tab.label}
              </button>
            ))}
          </div>

          {loading ? (
            <div className="product-review-empty-state">
              <div className="spinner-border text-danger" role="status" style={{ width: '3rem', height: '3rem', marginBottom: '16px' }}></div>
              <p style={{ fontWeight: '600', color: '#475569' }}>Đang tải đánh giá của bạn...</p>
            </div>
          ) : error ? (
            <div className="product-review-empty-state">
              <i className="fa-solid fa-triangle-exclamation" style={{ fontSize: '3rem', color: '#dc2626', marginBottom: '16px' }}></i>
              <p style={{ fontWeight: '600', color: '#b91c1c' }}>{error}</p>
            </div>
          ) : result.items.length === 0 ? (
            <div className="product-review-empty-state">
              <i className="fa-solid fa-comment-slash" style={{ fontSize: '4rem', color: '#cbd5e1', marginBottom: '16px' }}></i>
              <h3 style={{ fontSize: '1.25rem', color: '#334155', fontWeight: 'bold' }}>Bạn chưa gửi đánh giá nào</h3>
              <p style={{ color: '#64748b' }}>Hãy mua sắm và để lại đánh giá cho các sản phẩm nhé.</p>
              <Link to="/products" className="btn-view-product mt-3" style={{ background: '#d71920', color: '#fff', border: 'none' }}>
                Tiếp tục mua sắm
              </Link>
            </div>
          ) : (
            <div className="my-review-list">
              {result.items.map((review) => {
                const statusLabel = review.reviewStatusLabel || (
                  review.status === 0 ? 'Chờ duyệt' : 
                  review.status === 1 ? 'Đã công khai' : 
                  review.status === 2 ? 'Đã ẩn' : 'Đã từ chối'
                );

                return (
                  <article className="my-review-card-modern" key={review.id}>
                    <div className="my-review-header">
                      <div className="my-review-product-info">
                        <img 
                          src={getMediaUrl(review.images?.[0]?.imageUrl, 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image')} 
                          alt={review.productName} 
                          className="my-review-product-img" 
                          onError={(e) => { e.currentTarget.src = 'https://placehold.co/120x120/f3f4f6/9ca3af?text=No+Image'; }}
                        />
                        <div className="my-review-product-details">
                          <Link to={`/products/${review.productId}`} className="my-review-product-title">
                            {review.productName}
                          </Link>
                          <ReviewStars value={review.rating} size="1.1rem" />
                        </div>
                      </div>
                      <div className="my-review-status-badge">
                        <span className={`status-badge status-${review.status}`}>
                          {review.status === 0 && <i className="fa-solid fa-clock-rotate-left" style={{ marginRight: '6px' }}></i>}
                          {review.status === 1 && <i className="fa-solid fa-circle-check" style={{ marginRight: '6px' }}></i>}
                          {(review.status === 2 || review.status === 3) && <i className="fa-solid fa-circle-xmark" style={{ marginRight: '6px' }}></i>}
                          {statusLabel}
                        </span>
                      </div>
                    </div>

                    <div className="my-review-body">
                      <p className="my-review-text">{review.content}</p>
                      {review.replies?.length > 0 && (
                        <div className="my-review-reply-box">
                          <div className="reply-title"><i className="fa-solid fa-reply"></i> Phản hồi từ cửa hàng</div>
                          <p className="reply-text">{review.replies[0].content}</p>
                        </div>
                      )}
                      {(review.status === 2 || review.status === 3) && (
                        <div className="my-review-alert">
                          <i className="fa-solid fa-circle-exclamation"></i> {review.moderationReason || 'Đánh giá này không được hiển thị do vi phạm chính sách nội dung.'}
                        </div>
                      )}
                    </div>

                    <div className="my-review-footer">
                      <div className="my-review-date">
                        <i className="fa-regular fa-clock"></i> {new Date(review.createdAt).toLocaleString('vi-VN')}
                      </div>
                      <Link to={`/products/${review.productId}`} className="btn-view-product">
                        Xem sản phẩm <i className="fa-solid fa-arrow-right"></i>
                      </Link>
                    </div>
                  </article>
                );
              })}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}

export default MyReviewsPage;
