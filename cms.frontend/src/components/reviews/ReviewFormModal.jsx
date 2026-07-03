import React, { useMemo, useState } from 'react';
import { getMediaUrl } from '../../utils/mediaUrl';
import reviewService from '../../services/reviewService';
import ReviewStars from './ReviewStars';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_FILES = 5;
const MAX_SIZE = 5 * 1024 * 1024;

function ReviewFormModal({ open, item, onClose, onSubmitted }) {
  const [rating, setRating] = useState(5);
  const [content, setContent] = useState('');
  const [images, setImages] = useState([]);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const previews = useMemo(
    () => images.map((file) => ({ file, url: URL.createObjectURL(file) })),
    [images]
  );

  React.useEffect(() => () => {
    previews.forEach((preview) => URL.revokeObjectURL(preview.url));
  }, [previews]);

  React.useEffect(() => {
    if (!open) {
      setRating(5);
      setContent('');
      setImages([]);
      setError('');
      setSubmitting(false);
    }
  }, [open]);

  if (!open || !item) {
    return null;
  }

  const validateFiles = (nextFiles) => {
    if (nextFiles.length > MAX_FILES) {
      return 'Bạn chỉ được tải tối đa 5 hình ảnh.';
    }

    for (const file of nextFiles) {
      if (!ALLOWED_TYPES.includes(file.type)) {
        return 'Định dạng hình ảnh không hợp lệ.';
      }

      if (file.size > MAX_SIZE) {
        return 'Mỗi hình ảnh không được vượt qua 5 MB.';
      }
    }

    return '';
  };

  const handleFilesChange = (event) => {
    const nextFiles = [...images, ...Array.from(event.target.files || [])];
    const validationError = validateFiles(nextFiles);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError('');
    setImages(nextFiles.slice(0, MAX_FILES));
  };

  const handleRemoveImage = (index) => {
    setImages((current) => current.filter((_, currentIndex) => currentIndex !== index));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!rating) {
      setError('Vui lòng chọn số sao.');
      return;
    }

    if (content.trim().length < 10) {
      setError('Nội dung đánh giá phải có ít nhất 10 ký tự.');
      return;
    }

    const formData = new FormData();
    formData.append('orderDetailId', item.id);
    formData.append('rating', String(rating));
    formData.append('content', content.trim());
    images.forEach((file) => formData.append('images', file));

    try {
      setSubmitting(true);
      setError('');
      const response = await reviewService.createReview(formData);
      onSubmitted?.(response.review);
      onClose?.();
    } catch (requestError) {
      setError(requestError?.response?.data?.message || 'Không thể gửi đánh giá. Vui lòng thử lại.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="review-modal-overlay" onClick={() => !submitting && onClose?.()}>
      <div className="review-modal-card" onClick={(event) => event.stopPropagation()}>
        <div className="review-modal-header">
          <div>
            <h3>Đánh giá sản phẩm</h3>
            <p>Đánh giá của bạn sẽ được gửi cho quản trị viên duyệt trước khi hiển thị công khai.</p>
          </div>
          <button type="button" className="review-modal-close" onClick={() => !submitting && onClose?.()}>
            <i className="fa-solid fa-xmark"></i>
          </button>
        </div>

        <div className="review-product-brief">
          <img src={getMediaUrl(item.productImageUrl)} alt={item.productName} />
          <div>
            <h4>{item.productName}</h4>
            <div className="review-product-sku">Order item #{item.id}</div>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="review-form-body">
          <div className="review-form-group">
            <label>Đánh giá của bạn</label>
            <ReviewStars value={rating} onChange={setRating} size="1.5rem" interactive />
          </div>

          <div className="review-form-group">
            <label htmlFor="review-content">Nội dung đánh giá</label>
            <textarea
              id="review-content"
              rows="5"
              maxLength={2000}
              value={content}
              onChange={(event) => setContent(event.target.value)}
              placeholder="Chia sẻ cảm nhận thật của bạn sau khi sử dụng sản phẩm"
              required
            />
            <div className="review-field-hint">{content.trim().length}/2000 ký tự</div>
          </div>

          <div className="review-form-group">
            <label>Hình ảnh thực tế (tối đa 5 ảnh)</label>
            <label className="review-upload-box">
              <input type="file" accept=".jpg,.jpeg,.png,.webp" multiple onChange={handleFilesChange} hidden />
              <span><i className="fa-solid fa-camera"></i> Thêm ảnh</span>
            </label>

            {previews.length > 0 && (
              <div className="review-image-preview-grid">
                {previews.map((preview, index) => (
                  <div className="review-image-preview-item" key={`${preview.file.name}-${index}`}>
                    <img src={preview.url} alt={preview.file.name} />
                    <button type="button" onClick={() => handleRemoveImage(index)}>
                      <i className="fa-solid fa-xmark"></i>
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {error && <div className="review-form-error">{error}</div>}

          <div className="review-modal-actions">
            <button type="button" className="review-cancel-btn" onClick={() => onClose?.()} disabled={submitting}>
              Hủy
            </button>
            <button type="submit" className="review-submit-btn" disabled={submitting}>
              {submitting ? 'Đang gửi...' : 'Gửi đánh giá'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default ReviewFormModal;
