import React, { useEffect, useState } from 'react';
import reviewService from '../../services/reviewService';
import { getMediaUrl } from '../../utils/mediaUrl';
import ReviewStars from './ReviewStars';

/**
 * Component hiển thị một mục đánh giá duy nhất
 * Chứa thông tin người dùng, số sao đánh giá, nội dung, hình ảnh và phản hồi từ cửa hàng
 */
function ReviewItem({ review }) {
  const [isExpanded, setIsExpanded] = useState(false);

  // Kiểm tra xem nội dung đánh giá có quá dài không (> 150 ký tự)
  // Nếu quá dài, sẽ ẩn bớt và hiển thị nút "Xem chi tiết đánh giá"
  const hasLongText = review.content.length > 150;

  return (
    <article className="product-review-card">
      <div className="product-review-card-side">
        <img
          src={getMediaUrl(review.userAvatar, 'https://ui-avatars.com/api/?background=c80f1e&color=fff&name=User')}
          alt={review.userDisplayName}
        />
        <strong>{review.userDisplayName}</strong>
        <span className="product-review-badge">Đã mua hàng</span>
        <time>{new Date(review.createdAt).toLocaleDateString('vi-VN')}</time>
      </div>

      <div className="product-review-card-body">
        <ReviewStars value={review.rating} size="1rem" />
        
        <div className={`product-review-text-content ${isExpanded ? 'expanded' : 'collapsed'}`}>
          <p>{review.content}</p>
        </div>

        {review.images?.length > 0 && (
          <div className="product-review-images">
            {review.images.map((image) => (
              <div key={image.id} className="product-review-image-wrapper">
                <img src={getMediaUrl(image.imageUrl)} alt="Review" />
              </div>
            ))}
          </div>
        )}

        {review.replies?.length > 0 && (
          <div className="product-review-replies">
            {review.replies.map((reply) => (
              <div className="product-review-reply" key={reply.id}>
                <div className="product-review-reply-title">Phản hồi từ cửa hàng</div>
                <p>{reply.content}</p>
              </div>
            ))}
          </div>
        )}

        {/* View Details Button */}
        {(!isExpanded && hasLongText) && (
          <button 
            type="button" 
            className="btn btn-link text-danger p-0 mt-2 font-weight-bold" 
            style={{ fontSize: '0.9rem', textDecoration: 'none' }}
            onClick={() => setIsExpanded(true)}
          >
            Xem chi tiết đánh giá <i className="fa-solid fa-chevron-down ms-1"></i>
          </button>
        )}
        
        {isExpanded && (
          <button 
            type="button" 
            className="btn btn-link text-secondary p-0 mt-2 font-weight-bold" 
            style={{ fontSize: '0.9rem', textDecoration: 'none' }}
            onClick={() => setIsExpanded(false)}
          >
            Thu gọn <i className="fa-solid fa-chevron-up ms-1"></i>
          </button>
        )}
      </div>
    </article>
  );
}

/**
 * Component chính quản lý và hiển thị toàn bộ khu vực "Đánh giá sản phẩm"
 * Bao gồm: Bảng tóm tắt số sao, Bộ lọc (số sao, hình ảnh), Sắp xếp và Danh sách các đánh giá
 */
function ProductReviewSection({ productId }) {
  // State lưu trữ dữ liệu tổng quan (điểm trung bình, tổng số đánh giá, phân bố sao)
  const [summary, setSummary] = useState(null);
  // State lưu trữ mảng các đánh giá hiện tại đang hiển thị
  const [reviews, setReviews] = useState([]);
  
  // State phục vụ phân trang (Pagination)
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  // State phục vụ bộ lọc và sắp xếp
  const [ratingFilter, setRatingFilter] = useState(''); // Lọc theo số sao (1,2,3,4,5)
  const [hasImages, setHasImages] = useState(false); // Lọc chỉ hiển thị đánh giá có ảnh
  const [sortBy, setSortBy] = useState('newest'); // Sắp xếp (Mới nhất, Cũ nhất, Sao cao, Sao thấp)
  
  // State theo dõi trạng thái tải dữ liệu
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Hook useEffect được gọi mỗi khi có sự thay đổi về: page, productId, bộ lọc hoặc sắp xếp
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError('');

        // Gọi đồng thời 2 API để tối ưu tốc độ: Lấy tổng quan (Summary) và Lấy danh sách (List)
        const [summaryData, reviewData] = await Promise.all([
          reviewService.getProductReviewSummary(productId),
          reviewService.getProductReviews(productId, {
            page,
            pageSize: 5, // Mỗi trang hiển thị tối đa 5 đánh giá
            rating: ratingFilter || undefined,
            hasImages: hasImages || undefined,
            sortBy,
          }),
        ]);

        setSummary(summaryData);
        setReviews(reviewData.items || []);
        setTotalPages(reviewData.totalPages || 0);
      } catch (requestError) {
        setError(requestError?.response?.data?.message || 'Không thể tải danh sách đánh giá.');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [hasImages, page, productId, ratingFilter, sortBy]);

  // Hàm hỗ trợ vẽ một dòng biểu đồ tỉ lệ sao (ví dụ: Thanh % hiển thị 5 sao chiếm bao nhiêu)
  const renderDistributionBar = (label, count, total) => {
    // Tính phần trăm width để fill màu cho thanh tiến trình
    const width = total > 0 ? `${Math.round((count / total) * 100)}%` : '0%';
    return (
      <div className="product-review-distribution-row" key={label}>
        <span>{label} sao</span>
        <div className="product-review-distribution-track">
          <div className="product-review-distribution-fill" style={{ width }}></div>
        </div>
        <strong>{count}</strong>
      </div>
    );
  };

  return (
    <section className="product-review-section">
      <div className="product-review-section-header">
        <div>
          <h3>Đánh giá sản phẩm</h3>
          <p>Chỉ hiển thị các đánh giá đã được kiểm duyệt.</p>
        </div>
      </div>

      {summary && summary.totalReviews > 0 && (
        <div className="product-review-summary-card">
          <div className="product-review-score">
            <div className="product-review-score-value">{summary.averageRating.toFixed(1)}</div>
            <ReviewStars value={Math.round(summary.averageRating)} size="1.2rem" />
            <div className="product-review-score-total">{summary.totalReviews} đánh giá</div>
          </div>

          <div className="product-review-distribution">
            {renderDistributionBar(5, summary.fiveStarCount, summary.totalReviews)}
            {renderDistributionBar(4, summary.fourStarCount, summary.totalReviews)}
            {renderDistributionBar(3, summary.threeStarCount, summary.totalReviews)}
            {renderDistributionBar(2, summary.twoStarCount, summary.totalReviews)}
            {renderDistributionBar(1, summary.oneStarCount, summary.totalReviews)}
          </div>
        </div>
      )}

      <div className="product-review-toolbar">
        <div className="product-review-filters">
          <button type="button" className={ratingFilter === '' ? 'active' : ''} onClick={() => { setRatingFilter(''); setPage(1); }}>
            Tất cả
          </button>
          {[5, 4, 3, 2, 1].map((star) => (
            <button key={star} type="button" className={Number(ratingFilter) === star ? 'active' : ''} onClick={() => { setRatingFilter(String(star)); setPage(1); }}>
              {star} sao
            </button>
          ))}
          <button type="button" className={hasImages ? 'active' : ''} onClick={() => { setHasImages((current) => !current); setPage(1); }}>
            Có hình ảnh
          </button>
        </div>

        <select value={sortBy} onChange={(event) => { setSortBy(event.target.value); setPage(1); }}>
          <option value="newest">Mới nhất</option>
          <option value="oldest">Cũ nhất</option>
          <option value="rating-desc">Sao cao trước</option>
          <option value="rating-asc">Sao thấp trước</option>
        </select>
      </div>

      {loading ? (
        <div className="product-review-empty-state">Đang tải đánh giá...</div>
      ) : error ? (
        <div className="product-review-empty-state">
          <div>{error}</div>
          <button type="button" onClick={() => setPage(1)}>Thử lại</button>
        </div>
      ) : reviews.length === 0 ? (
        <div className="product-review-empty-state">
          <div>Chưa có đánh giá nào cho sản phẩm này.</div>
          <small>Hãy là người đầu tiên chia sẻ trải nghiệm.</small>
        </div>
      ) : (
        <>
          <div className="product-review-list">
            {reviews.map((review) => (
              <ReviewItem key={review.id} review={review} />
            ))}
          </div>

          {totalPages > 1 && (
            <div className="product-review-pagination">
              <button type="button" onClick={() => setPage((current) => Math.max(1, current - 1))} disabled={page === 1}>
                Trước
              </button>
              <span>Trang {page}/{totalPages}</span>
              <button type="button" onClick={() => setPage((current) => Math.min(totalPages, current + 1))} disabled={page === totalPages}>
                Sau
              </button>
            </div>
          )}
        </>
      )}
    </section>
  );
}

export default ProductReviewSection;
