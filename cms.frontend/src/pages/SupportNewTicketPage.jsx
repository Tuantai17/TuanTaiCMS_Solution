import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import authService from '../services/authService';
import orderService from '../services/orderService';
import supportService, {
  SUPPORT_CATEGORY_OPTIONS,
  SUPPORT_EMOJIS,
  SUPPORT_STICKERS,
} from '../services/supportService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import '../assets/css/Profile.css';
import '../assets/css/OrderDetail.css';
import '../assets/css/SupportTickets.css';

const MAX_ATTACHMENTS = 5;

function SupportNewTicketPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const orderIdFromQuery = searchParams.get('orderId') || '';

  const [customer, setCustomer] = useState(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [loadingOrders, setLoadingOrders] = useState(true);
  const [orders, setOrders] = useState([]);
  const [orderItems, setOrderItems] = useState([]);
  const [form, setForm] = useState({
    subject: '',
    category: 'order',
    relatedOrderId: orderIdFromQuery,
    relatedProductId: '',
    content: '',
  });
  const [selectedSticker, setSelectedSticker] = useState('');
  const [attachments, setAttachments] = useState([]);
  const [error, setError] = useState('');

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

    const loadOrders = async () => {
      try {
        setLoadingOrders(true);
        const response = await orderService.getMyOrders({ page: 1, pageSize: 50 });
        setOrders(response?.items || []);
      } catch {
        setOrders([]);
      } finally {
        setLoadingOrders(false);
      }
    };

    loadOrders();
  }, [customer, loadingProfile]);

  useEffect(() => {
    if (!form.relatedOrderId || loadingProfile || !customer) {
      setOrderItems([]);
      return;
    }

    const loadOrderDetail = async () => {
      try {
        const orderDetail = await orderService.getMyOrderDetail(form.relatedOrderId);
        setOrderItems(orderDetail?.items || []);
      } catch {
        setOrderItems([]);
      }
    };

    loadOrderDetail();
  }, [customer, form.relatedOrderId, loadingProfile]);

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const appendEmoji = (emoji) => {
    setForm((current) => ({
      ...current,
      content: `${current.content}${emoji}`,
    }));
  };

  const handleAttachmentChange = (event) => {
    const files = Array.from(event.target.files || []);
    const nextFiles = files.slice(0, Math.max(MAX_ATTACHMENTS - attachments.length, 0));
    const previews = nextFiles.map((file) => ({
      id: `preview-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`,
      file,
      name: file.name,
      previewUrl: URL.createObjectURL(file),
    }));

    setAttachments((current) => [...current, ...previews].slice(0, MAX_ATTACHMENTS));
    event.target.value = '';
  };

  const removeAttachment = (attachmentId) => {
    setAttachments((current) => {
      const nextList = current.filter((item) => item.id !== attachmentId);
      const removed = current.find((item) => item.id === attachmentId);
      if (removed?.previewUrl) {
        URL.revokeObjectURL(removed.previewUrl);
      }
      return nextList;
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');

    if (!form.subject.trim() || !form.content.trim()) {
      setError('Vui lòng nhập tiêu đề và nội dung cần hỗ trợ.');
      return;
    }

    try {
      setSubmitting(true);
      const createdTicket = await supportService.createTicket({
        subject: form.subject,
        category: form.category,
        content: form.content,
        relatedOrderId: form.relatedOrderId,
        relatedProductId: form.relatedProductId,
        images: attachments.map((item) => item.file),
        stickerCode: selectedSticker,
      });

      navigate(`/account/support/${createdTicket.id}`);
    } catch (requestError) {
      setError(requestError?.response?.data?.message || requestError?.message || 'Không thể tạo yêu cầu hỗ trợ lúc này.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="account-order-page support-page-bg">
      <div className="account-order-layout">
        <AccountSidebar activeKey="support" customer={customer} onLogout={handleLogout} />

        <section className="order-detail-main-card support-shell animate--fade-in">
          <div className="support-breadcrumb">
            <Link to="/account/support">Hỗ trợ khách hàng</Link>
            <span>/</span>
            <span>Tạo yêu cầu mới</span>
          </div>

          <div className="support-form-head">
            <div>
              <h1 className="order-detail-title">Tạo yêu cầu hỗ trợ mới</h1>
              <p className="order-detail-description">
                Vui lòng cung cấp đầy đủ thông tin để chúng tôi hỗ trợ bạn nhanh chóng và chính xác hơn.
              </p>
            </div>
          </div>

          <form className="support-form-card" onSubmit={handleSubmit}>
            <div className="support-form-grid support-form-grid-single">
              <div className="support-field">
                <label>Tiêu đề *</label>
                <input
                  type="text"
                  value={form.subject}
                  maxLength={100}
                  onChange={(event) => setForm((current) => ({ ...current, subject: event.target.value }))}
                  placeholder="Nhập tiêu đề ngắn gọn về vấn đề của bạn"
                />
                <span className="support-field-counter">{form.subject.length}/100</span>
              </div>
            </div>

            <div className="support-form-grid">
              <div className="support-field">
                <label>Loại vấn đề *</label>
                <select
                  value={form.category}
                  onChange={(event) => setForm((current) => ({ ...current, category: event.target.value }))}
                >
                  {SUPPORT_CATEGORY_OPTIONS.map((item) => (
                    <option key={item.value} value={item.value}>
                      {item.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="support-field">
                <label>Đơn hàng liên quan</label>
                <select
                  value={form.relatedOrderId}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      relatedOrderId: event.target.value,
                      relatedProductId: '',
                    }))
                  }
                  disabled={loadingOrders}
                >
                  <option value="">Chọn đơn hàng</option>
                  {orders.map((order) => (
                    <option key={order.id} value={order.id}>
                      #{order.id}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="support-form-grid">
              <div className="support-field">
                <label>Sản phẩm liên quan</label>
                <select
                  value={form.relatedProductId}
                  onChange={(event) => setForm((current) => ({ ...current, relatedProductId: event.target.value }))}
                  disabled={!form.relatedOrderId || orderItems.length === 0}
                >
                  <option value="">Chọn sản phẩm</option>
                  {orderItems.map((item) => (
                    <option key={item.productId} value={item.productId}>
                      {item.productName}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="support-form-grid support-form-grid-single">
              <div className="support-field">
                <label>Nội dung cần hỗ trợ *</label>
                <textarea
                  value={form.content}
                  maxLength={1500}
                  onChange={(event) => setForm((current) => ({ ...current, content: event.target.value }))}
                  placeholder="Mô tả chi tiết vấn đề bạn đang gặp phải..."
                  rows={5}
                />
                <span className="support-field-counter">{form.content.length}/1500</span>
              </div>
            </div>

            <div className="support-form-grid support-form-grid-single">
              <div className="support-field">
                <label>Đính kèm hình ảnh</label>
                <label className="support-upload-box">
                  <input type="file" accept="image/*" multiple onChange={handleAttachmentChange} hidden />
                  <i className="fa-regular fa-image"></i>
                  <div>
                    <strong>Kéo thả ảnh vào đây hoặc chọn ảnh</strong>
                    <p>Tối đa {MAX_ATTACHMENTS} ảnh, định dạng JPG, PNG, WEBP.</p>
                  </div>
                </label>

                {attachments.length > 0 && (
                  <div className="support-upload-preview-list">
                    {attachments.map((item) => (
                      <div className="support-upload-preview" key={item.id}>
                        <img src={item.previewUrl} alt={item.name} />
                        <button type="button" onClick={() => removeAttachment(item.id)}>
                          <i className="fa-solid fa-xmark"></i>
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="support-form-grid support-form-grid-single">
              <div className="support-picker-group">
                <label>Emoji</label>
                <div className="support-emoji-row">
                  {SUPPORT_EMOJIS.map((emoji) => (
                    <button key={emoji} type="button" onClick={() => appendEmoji(emoji)}>
                      {emoji}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            <div className="support-form-grid support-form-grid-single">
              <div className="support-picker-group">
                <label>Sticker</label>
                <div className="support-sticker-row">
                  {SUPPORT_STICKERS.map((sticker) => (
                    <button
                      key={sticker.code}
                      type="button"
                      className={selectedSticker === sticker.code ? 'active' : ''}
                      onClick={() => setSelectedSticker((current) => (current === sticker.code ? '' : sticker.code))}
                    >
                      <span>{sticker.emoji}</span>
                      {sticker.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {error && <div className="support-form-error">{error}</div>}

            <div className="support-form-actions">
              <Link to="/account/support" className="support-secondary-btn">
                Hủy
              </Link>
              <button type="submit" className="support-primary-btn" disabled={submitting}>
                <i className="fa-regular fa-paper-plane"></i>
                {submitting ? 'Đang gửi...' : 'Gửi yêu cầu'}
              </button>
            </div>
          </form>
        </section>
      </div>
    </div>
  );
}

export default SupportNewTicketPage;
