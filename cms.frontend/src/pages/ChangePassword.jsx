import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import { getMediaUrl } from '../utils/mediaUrl';
import '../assets/css/Profile.css';

const DEFAULT_AVATAR = 'https://ui-avatars.com/api/?background=c80f1e&color=fff&size=200&font-size=0.4&bold=true&name=';

const MENU_ITEMS = [
  { key: 'info', label: 'Thông tin tài khoản', icon: 'fa-solid fa-user', type: 'route', path: '/profile' },
  { key: 'address', label: 'Sổ địa chỉ', icon: 'fa-solid fa-location-dot', type: 'tab' },
  { key: 'order-history', label: 'Lịch sử mua hàng', icon: 'fa-solid fa-clock-rotate-left', type: 'route', path: '/account/orders' },
  { key: 'change-password', label: 'Đổi mật khẩu', icon: 'fa-solid fa-key', type: 'current' },
  { key: 'logout', label: 'Đăng xuất', icon: 'fa-solid fa-right-from-bracket', isLogout: true, type: 'logout' },
];

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;

function ChangePassword() {
  const navigate = useNavigate();
  const [customerId, setCustomerId] = useState(null);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  const [fetching, setFetching] = useState(true);

  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showOldPassword, setShowOldPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});

  useEffect(() => {
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      navigate('/login');
      return;
    }

    let parsedCustomer = null;

    try {
      parsedCustomer = JSON.parse(storedCustomer);
    } catch {
      localStorage.removeItem('customer');
      navigate('/login');
      return;
    }

    if (!parsedCustomer?.customerId) {
      localStorage.removeItem('customer');
      navigate('/login');
      return;
    }

    setCustomerId(parsedCustomer.customerId);

    const populateCustomer = (data) => {
      setFullName(data.fullName || '');
      setEmail(data.email || '');
      setAvatarUrl(data.avatarUrl || '');
    };

    const loadProfile = async () => {
      try {
        const profile = await authService.getProfile(parsedCustomer.customerId);
        populateCustomer(profile);
      } catch {
        populateCustomer(parsedCustomer);
      } finally {
        setFetching(false);
      }
    };

    loadProfile();
  }, [navigate]);

  const passwordChecks = useMemo(() => {
    const value = newPassword || '';

    return {
      length: value.length >= 8,
      lower: /[a-z]/.test(value),
      upper: /[A-Z]/.test(value),
      number: /\d/.test(value),
      special: /[^A-Za-z\d]/.test(value),
      valid: PASSWORD_PATTERN.test(value),
    };
  }, [newPassword]);

  const confirmMatched = confirmPassword.length > 0 && newPassword === confirmPassword;

  const getAvatarSrc = () => {
    if (avatarUrl) {
      return getMediaUrl(avatarUrl);
    }

    return DEFAULT_AVATAR + encodeURIComponent(fullName || 'User');
  };

  const resetForm = () => {
    setOldPassword('');
    setNewPassword('');
    setConfirmPassword('');
    setFieldErrors({});
    setError('');
    setSuccess('');
  };

  const validateForm = () => {
    const nextErrors = {};

    if (!oldPassword) {
      nextErrors.oldPassword = 'Vui lòng nhập mật khẩu hiện tại.';
    }

    if (!newPassword) {
      nextErrors.newPassword = 'Vui lòng nhập mật khẩu mới.';
    } else if (!PASSWORD_PATTERN.test(newPassword)) {
      nextErrors.newPassword = 'Mật khẩu mới phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.';
    }

    if (!confirmPassword) {
      nextErrors.confirmPassword = 'Vui lòng xác nhận mật khẩu mới.';
    } else if (newPassword !== confirmPassword) {
      nextErrors.confirmPassword = 'Mật khẩu xác nhận chưa khớp.';
    }

    if (oldPassword && newPassword && oldPassword === newPassword) {
      nextErrors.newPassword = 'Mật khẩu mới không được trùng với mật khẩu hiện tại.';
    }

    setFieldErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setSuccess('');

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const response = await authService.changePassword({
        customerId,
        oldPassword,
        newPassword,
      });

      setSuccess(response?.message || 'Cập nhật mật khẩu thành công.');
      resetForm();
    } catch (err) {
      const message = err.response?.data?.message || 'Đổi mật khẩu thất bại. Vui lòng thử lại.';
      const nextErrors = {};

      if (message.toLowerCase().includes('hiện tại')) {
        nextErrors.oldPassword = message;
      }

      if (message.toLowerCase().includes('mật khẩu mới')) {
        nextErrors.newPassword = message;
      }

      setFieldErrors((prev) => ({ ...prev, ...nextErrors }));
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const handleSidebarClick = (item) => {
    if (item.type === 'logout') {
      localStorage.removeItem('customer');
      window.dispatchEvent(new Event('customerLoginStateChange'));
      navigate('/');
      return;
    }

    if (item.type === 'route' && item.path) {
      navigate(item.path);
      return;
    }

    if (item.type === 'tab') {
      navigate('/profile', { state: { profileTab: item.key } });
    }
  };

  if (fetching) {
    return (
      <div className="profile-page">
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

  return (
    <div className="profile-page">
      <div className="profile-layout">
        <aside className="profile-sidebar">
          <div className="profile-sidebar-card">
            <div className="profile-sidebar-header">
              <div className="profile-avatar-wrapper profile-avatar-wrapper-static">
                <img src={getAvatarSrc()} alt={fullName} className="profile-avatar-img" />
              </div>
              <h4 className="profile-sidebar-name">{fullName}</h4>
              <p className="profile-sidebar-email">{email}</p>
              <span className="profile-member-badge">
                <i className="fa-solid fa-crown"></i>
                Thành viên
              </span>
            </div>

            <ul className="profile-sidebar-menu">
              {MENU_ITEMS.map((item) => (
                <React.Fragment key={item.key}>
                  {item.isLogout && (
                    <li>
                      <div className="profile-menu-divider"></div>
                    </li>
                  )}
                  <li>
                    <button
                      type="button"
                      className={`profile-menu-item ${item.key === 'change-password' ? 'active' : ''} ${item.isLogout ? 'logout-item' : ''}`}
                      onClick={() => handleSidebarClick(item)}
                    >
                      <i className={item.icon}></i>
                      <span>{item.label}</span>
                      {item.badge > 0 && <span className="menu-badge">{item.badge}</span>}
                    </button>
                  </li>
                </React.Fragment>
              ))}
            </ul>
          </div>
        </aside>

        <section className="profile-content">
          <div className="profile-content-card profile-password-card">
            <h2 className="profile-content-title">Đổi mật khẩu</h2>
            <p className="profile-content-subtitle">Cập nhật mật khẩu để bảo vệ tài khoản của bạn.</p>

            {error && (
              <div className="profile-alert profile-alert-error">
                <i className="fa-solid fa-circle-exclamation"></i>
                {error}
              </div>
            )}

            {success && (
              <div className="profile-alert profile-alert-success">
                <i className="fa-solid fa-circle-check"></i>
                {success}
              </div>
            )}

            <form className="profile-password-form" onSubmit={handleSubmit}>
              <div className="profile-form-group">
                <label htmlFor="oldPassword">Mật khẩu hiện tại *</label>
                <div className="profile-password-wrapper">
                  <input
                    id="oldPassword"
                    type={showOldPassword ? 'text' : 'password'}
                    className={`profile-form-input profile-password-input ${fieldErrors.oldPassword ? 'is-invalid' : ''}`}
                    value={oldPassword}
                    onChange={(event) => setOldPassword(event.target.value)}
                    placeholder="Nhập mật khẩu hiện tại"
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    className="profile-password-toggle"
                    onClick={() => setShowOldPassword((value) => !value)}
                    aria-label={showOldPassword ? 'Ẩn mật khẩu hiện tại' : 'Hiện mật khẩu hiện tại'}
                  >
                    <i className={showOldPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i>
                  </button>
                </div>
                {fieldErrors.oldPassword ? (
                  <div className="profile-form-error">{fieldErrors.oldPassword}</div>
                ) : (
                  <div className={`profile-password-hint ${oldPassword ? 'valid' : 'info'}`}>
                    <i className={`fa-solid ${oldPassword ? 'fa-circle-check' : 'fa-circle-info'}`}></i>
                    {oldPassword ? 'Mật khẩu hiện tại sẽ được xác thực khi cập nhật.' : 'Nhập đúng mật khẩu hiện tại của tài khoản.'}
                  </div>
                )}
              </div>

              <div className="profile-form-group">
                <label htmlFor="newPassword">Mật khẩu mới *</label>
                <div className="profile-password-wrapper">
                  <input
                    id="newPassword"
                    type={showNewPassword ? 'text' : 'password'}
                    className={`profile-form-input profile-password-input ${fieldErrors.newPassword ? 'is-invalid' : ''}`}
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    placeholder="Nhập mật khẩu mới"
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    className="profile-password-toggle"
                    onClick={() => setShowNewPassword((value) => !value)}
                    aria-label={showNewPassword ? 'Ẩn mật khẩu mới' : 'Hiện mật khẩu mới'}
                  >
                    <i className={showNewPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i>
                  </button>
                </div>
                {fieldErrors.newPassword && <div className="profile-form-error">{fieldErrors.newPassword}</div>}

                <div className={`profile-password-hint ${passwordChecks.valid ? 'valid' : 'info'}`}>
                  <i className={`fa-solid ${passwordChecks.valid ? 'fa-circle-check' : 'fa-shield-halved'}`}></i>
                  Mật khẩu mạnh: Tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.
                </div>

                <div className="profile-password-rule-grid">
                  <span className={`profile-password-rule ${passwordChecks.length ? 'valid' : ''}`}>8 ký tự</span>
                  <span className={`profile-password-rule ${passwordChecks.upper ? 'valid' : ''}`}>Chữ hoa</span>
                  <span className={`profile-password-rule ${passwordChecks.lower ? 'valid' : ''}`}>Chữ thường</span>
                  <span className={`profile-password-rule ${passwordChecks.number ? 'valid' : ''}`}>Số</span>
                  <span className={`profile-password-rule ${passwordChecks.special ? 'valid' : ''}`}>Ký tự đặc biệt</span>
                </div>
              </div>

              <div className="profile-form-group">
                <label htmlFor="confirmPassword">Xác nhận mật khẩu mới *</label>
                <div className="profile-password-wrapper">
                  <input
                    id="confirmPassword"
                    type={showConfirmPassword ? 'text' : 'password'}
                    className={`profile-form-input profile-password-input ${fieldErrors.confirmPassword ? 'is-invalid' : ''}`}
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                    placeholder="Nhập lại mật khẩu mới"
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    className="profile-password-toggle"
                    onClick={() => setShowConfirmPassword((value) => !value)}
                    aria-label={showConfirmPassword ? 'Ẩn xác nhận mật khẩu mới' : 'Hiện xác nhận mật khẩu mới'}
                  >
                    <i className={showConfirmPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i>
                  </button>
                </div>
                {fieldErrors.confirmPassword ? (
                  <div className="profile-form-error">{fieldErrors.confirmPassword}</div>
                ) : (
                  <div className={`profile-password-hint ${confirmMatched ? 'valid' : 'info'}`}>
                    <i className={`fa-solid ${confirmMatched ? 'fa-circle-check' : 'fa-circle-info'}`}></i>
                    {confirmMatched ? 'Mật khẩu xác nhận khớp.' : 'Nhập lại đúng mật khẩu mới để xác nhận.'}
                  </div>
                )}
              </div>

              <div className="profile-btn-group profile-password-actions">
                <button type="submit" className="profile-btn-save" disabled={loading}>
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                      Đang cập nhật...
                    </>
                  ) : (
                    <>
                      <i className="fa-solid fa-lock"></i>
                      Cập nhật mật khẩu
                    </>
                  )}
                </button>
                <button type="button" className="profile-btn-cancel" onClick={resetForm} disabled={loading}>
                  Hủy
                </button>
              </div>
            </form>
          </div>
        </section>
      </div>
    </div>
  );
}

export default ChangePassword;
