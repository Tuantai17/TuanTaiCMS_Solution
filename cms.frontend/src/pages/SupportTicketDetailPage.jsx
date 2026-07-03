import React, { useEffect, useRef, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AccountSidebar from '../components/account/AccountSidebar';
import authService from '../services/authService';
import supportService, {
  SUPPORT_EMOJIS,
  SUPPORT_STICKERS,
  getSupportStatusMeta,
} from '../services/supportService';
import { clearStoredCustomer, getStoredCustomer } from '../utils/customerSession';
import '../assets/css/Profile.css';
import '../assets/css/OrderDetail.css';
import '../assets/css/SupportTickets.css';

function SupportTicketDetailPage() {
  const navigate = useNavigate();
  const { ticketId } = useParams();
  const fileInputRef = useRef(null);
  const [customer, setCustomer] = useState(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [composerText, setComposerText] = useState('');
  const [selectedSticker, setSelectedSticker] = useState('');
  const [attachments, setAttachments] = useState([]);
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [showStickerPicker, setShowStickerPicker] = useState(false);
  const [error, setError] = useState('');
  const [unreadBadge, setUnreadBadge] = useState(0);

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
    if (loadingProfile || !customer || !ticketId) {
      return;
    }

    let isCancelled = false;
    setLoading(true);

    const loadTicket = async () => {
      try {
        const detail = await supportService.markCustomerTicketRead(ticketId);
        if (!isCancelled) {
          setTicket(detail);
          setError('');
        }
      } catch (requestError) {
        if (!isCancelled) {
          setTicket(null);
          setError(requestError?.response?.data?.message || 'Không thể tải chi tiết yêu cầu hỗ trợ.');
        }
      } finally {
        if (!isCancelled) {
          setLoading(false);
        }
      }
    };

    loadTicket();

    return () => {
      isCancelled = true;
    };
  }, [customer, loadingProfile, ticketId]);

  useEffect(() => {
    if (loadingProfile || !customer) {
      return;
    }

    let isCancelled = false;

    supportService
      .getSupportBadgeCount()
      .then((count) => {
        if (!isCancelled) {
          setUnreadBadge(count);
        }
      })
      .catch(() => {
        if (!isCancelled) {
          setUnreadBadge(0);
        }
      });

    return () => {
      isCancelled = true;
    };
  }, [customer, loadingProfile, ticket]);

  const handleLogout = () => {
    clearStoredCustomer();
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  const handleAttachmentChange = (event) => {
    const files = Array.from(event.target.files || []);
    const previews = files.map((file) => ({
      id: `preview-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`,
      file,
      previewUrl: URL.createObjectURL(file),
      name: file.name,
    }));
    setAttachments((current) => [...current, ...previews].slice(0, 5));
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

  const sendMessage = async () => {
    if (!ticket || !customer) {
      return;
    }

    try {
      setError('');
      const updatedTicket = await supportService.sendCustomerMessage(ticket.id, {
        content: composerText,
        images: attachments.map((item) => item.file),
        stickerCode: selectedSticker,
      });

      setTicket(updatedTicket);
      setComposerText('');
      setSelectedSticker('');
      setShowEmojiPicker(false);
      setShowStickerPicker(false);
      attachments.forEach((item) => {
        if (item.previewUrl) {
          URL.revokeObjectURL(item.previewUrl);
        }
      });
      setAttachments([]);
    } catch (requestError) {
      setError(requestError?.response?.data?.message || requestError?.message || 'Không thể gửi tin nhắn lúc này.');
    }
  };

  const reopenTicket = async () => {
    if (!ticket || !customer) {
      return;
    }

    try {
      const updatedTicket = await supportService.reopenCustomerTicket(ticket.id);
      setTicket(updatedTicket);
    } catch (requestError) {
      setError(requestError?.response?.data?.message || requestError?.message || 'Không thể mở lại yêu cầu.');
    }
  };

  const statusMeta = ticket ? getSupportStatusMeta(ticket.status) : null;

  return (
    <div className="account-order-page support-page-bg">
      <div className="account-order-layout">
        <AccountSidebar
          activeKey="support"
          customer={customer}
          onLogout={handleLogout}
          badges={{ support: unreadBadge }}
        />

        <section className="order-detail-main-card support-shell animate--fade-in">
          {loading ? (
            <div className="support-loading-box" style={{ minHeight: '480px' }}>
              <span className="spinner-border text-danger" role="status"></span>
              <p>Đang tải chi tiết yêu cầu...</p>
            </div>
          ) : !ticket ? (
            <div className="support-empty-box" style={{ minHeight: '420px' }}>
              <i className="fa-regular fa-folder-open"></i>
              <h3>Không tìm thấy yêu cầu hỗ trợ</h3>
              {error && <p>{error}</p>}
              <Link to="/account/support" className="support-primary-btn">
                Quay lại danh sách
              </Link>
            </div>
          ) : (
            <>
              <div className="support-breadcrumb">
                <Link to="/account/support">Hỗ trợ khách hàng</Link>
                <span>/</span>
                <span>Chi tiết yêu cầu</span>
              </div>

              <div className="support-detail-top">
                <div className="support-detail-info-card">
                  <div className="support-detail-heading">
                    <div>
                      <h1>{ticket.code}</h1>
                      <span className={`support-status-pill tone-${statusMeta?.tone}`}>{statusMeta?.label}</span>
                    </div>
                    {ticket.relatedOrderId && (
                      <Link to={`/account/orders/${ticket.relatedOrderId}`} className="support-order-link-btn">
                        Xem chi tiết đơn hàng
                        <i className="fa-solid fa-chevron-right"></i>
                      </Link>
                    )}
                  </div>
                  <div className="support-meta-list">
                    <p>
                      <strong>Loại vấn đề:</strong> {ticket.categoryLabel}
                    </p>
                    {ticket.relatedOrderCode && (
                      <p>
                        <strong>Đơn hàng liên quan:</strong> {ticket.relatedOrderCode}
                      </p>
                    )}
                    <p>
                      <strong>Thời gian tạo:</strong> {new Date(ticket.createdAt).toLocaleString('vi-VN')}
                    </p>
                  </div>
                </div>
              </div>

              {ticket.status === 'closed' && (
                <div className="support-closed-banner">
                  <div>
                    <strong>Yêu cầu hỗ trợ này đã được đóng.</strong>
                    <p>Nếu bạn vẫn còn vấn đề, vui lòng mở lại yêu cầu để tiếp tục trao đổi.</p>
                  </div>
                  <button type="button" className="support-secondary-danger-btn" onClick={reopenTicket}>
                    Mở lại yêu cầu
                  </button>
                </div>
              )}

              <div className="support-conversation-list">
                {ticket.messages.map((message) => (
                  <div
                    key={message.id}
                    className={`support-message-row ${message.senderType === 'customer' ? 'from-customer' : message.senderType === 'system' ? 'from-system' : 'from-staff'}`}
                  >
                    {message.senderType !== 'customer' && message.senderType !== 'system' && (
                      <div className="support-avatar support-avatar-staff">
                        <img src="https://ui-avatars.com/api/?background=fff3e0&color=d97706&name=CS" alt="Nhân viên" />
                      </div>
                    )}

                    <div className={`support-message-bubble ${message.senderType}`}>
                      {message.senderType !== 'system' && <h4>{message.senderName}</h4>}
                      {message.content && <p>{message.content}</p>}
                      {message.stickerCode && (
                        <div className="support-inline-sticker">
                          {SUPPORT_STICKERS.find((item) => item.code === message.stickerCode)?.emoji || '💬'}
                        </div>
                      )}
                      {message.attachments?.length > 0 && (
                        <div className="support-message-attachments">
                          {message.attachments.map((attachment) => (
                            <img key={attachment.id} src={attachment.url} alt={attachment.name} />
                          ))}
                        </div>
                      )}
                      <span>{new Date(message.createdAt).toLocaleString('vi-VN')}</span>
                    </div>

                    {message.senderType === 'customer' && (
                      <div className="support-avatar support-avatar-customer">
                        <img
                          src={`https://ui-avatars.com/api/?background=fef2f2&color=d71920&name=${encodeURIComponent(customer?.fullName || 'KH')}`}
                          alt={customer?.fullName || 'Khách hàng'}
                        />
                      </div>
                    )}
                  </div>
                ))}
              </div>

              {ticket.status !== 'closed' ? (
                <div className="support-composer-card">
                  <textarea
                    value={composerText}
                    onChange={(event) => setComposerText(event.target.value)}
                    placeholder="Nhập tin nhắn của bạn..."
                    rows={4}
                  />

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

                  {(showEmojiPicker || showStickerPicker) && (
                    <div className="support-picker-panel">
                      {showEmojiPicker && (
                        <div className="support-picker-section">
                          <div className="support-picker-grid">
                            {SUPPORT_EMOJIS.map((emoji) => (
                              <button key={emoji} type="button" onClick={() => setComposerText((current) => `${current}${emoji}`)}>
                                {emoji}
                              </button>
                            ))}
                          </div>
                        </div>
                      )}

                      {showStickerPicker && (
                        <div className="support-picker-section">
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
                      )}
                    </div>
                  )}

                  {error && <div className="support-form-error">{error}</div>}

                  <div className="support-composer-actions">
                    <div className="support-composer-tools">
                      <button type="button" onClick={() => setShowEmojiPicker((current) => !current)}>
                        <i className="fa-regular fa-face-smile"></i>
                      </button>
                      <button type="button" onClick={() => fileInputRef.current?.click()}>
                        <i className="fa-regular fa-image"></i>
                      </button>
                      <button type="button" onClick={() => setShowStickerPicker((current) => !current)}>
                        <i className="fa-regular fa-note-sticky"></i>
                      </button>
                      <input ref={fileInputRef} type="file" accept="image/*" multiple hidden onChange={handleAttachmentChange} />
                    </div>

                    <button type="button" className="support-primary-btn" onClick={sendMessage}>
                      Gửi
                    </button>
                  </div>
                </div>
              ) : (
                <div className="support-timeline-end">
                  <span>Đây là toàn bộ nội dung trao đổi của yêu cầu.</span>
                </div>
              )}
            </>
          )}
        </section>
      </div>
    </div>
  );
}

export default SupportTicketDetailPage;
