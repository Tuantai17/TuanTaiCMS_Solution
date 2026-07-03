import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import authService from '../services/authService';
import addressService from '../services/addressService';
import { getMediaUrl } from '../utils/mediaUrl';
import { useFavorite } from '../contexts/FavoriteContext';
import AccountSidebar from '../components/account/AccountSidebar';
import '../assets/css/Profile.css';

const DEFAULT_AVATAR = 'https://ui-avatars.com/api/?background=c80f1e&color=fff&size=200&font-size=0.4&bold=true&name=';

const Profile = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const avatarInputRef = useRef(null);

  // State dữ liệu tài khoản
  const [customerId, setCustomerId] = useState(null);
  const { favoriteCount } = useFavorite();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [gender, setGender] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  const [createdAt, setCreatedAt] = useState('');
  const [totalOrders, setTotalOrders] = useState(0);
  const [defaultAddressObj, setDefaultAddressObj] = useState(null);

  // State dữ liệu gốc để hỗ trợ "Hủy thay đổi"
  const [originalData, setOriginalData] = useState({});

  // State đổi mật khẩu
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [showOldPw, setShowOldPw] = useState(false);
  const [showNewPw, setShowNewPw] = useState(false);
  const [showConfirmPw, setShowConfirmPw] = useState(false);

  // State tab & UI
  const [activeTab, setActiveTab] = useState('info');
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});

  // State avatar preview
  const [avatarPreview, setAvatarPreview] = useState('');

  // Tải dữ liệu tài khoản khi mount
  useEffect(() => {
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      navigate('/login');
      return;
    }

    try {
      const parsed = JSON.parse(storedCustomer);
      setCustomerId(parsed.customerId);

      const fetchData = async () => {
        try {
          setFetching(true);
          const data = await authService.getProfile(parsed.customerId);
          populateFields(data);
          
          try {
            const addressesData = await addressService.getAddresses(parsed.customerId);
            const defAddr = addressesData?.find(a => a.isDefault);
            setDefaultAddressObj(defAddr || null);
          } catch (addrErr) {
            console.error("Lỗi khi tải danh sách địa chỉ:", addrErr);
          }
        } catch (err) {
          console.error("Lỗi khi tải thông tin tài khoản:", err);
          // Fallback từ localStorage
          populateFields(parsed);
        } finally {
          setFetching(false);
        }
      };
      fetchData();
    } catch (e) {
      localStorage.removeItem('customer');
      navigate('/login');
    }
  }, [navigate]);

  useEffect(() => {
    if (location.state?.profileTab) {
      setActiveTab(location.state.profileTab);
    }
  }, [location.state]);

  const populateFields = (data) => {
    setFullName(data.fullName || '');
    setEmail(data.email || '');
    setPhone(data.phone || '');
    setAddress(data.address || '');
    setGender(data.gender || '');
    setAvatarUrl(data.avatarUrl || '');
    setCreatedAt(data.createdAt || '');
    setTotalOrders(data.totalOrders || 0);
    if (data.dateOfBirth) {
      const d = new Date(data.dateOfBirth);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      setDateOfBirth(`${yyyy}-${mm}-${dd}`);
    } else {
      setDateOfBirth('');
    }
    setOriginalData({
      fullName: data.fullName || '',
      email: data.email || '',
      phone: data.phone || '',
      address: data.address || '',
      gender: data.gender || '',
      dateOfBirth: data.dateOfBirth || '',
    });
  };

  const getAvatarSrc = () => {
    if (avatarPreview) return avatarPreview;
    if (avatarUrl) return getMediaUrl(avatarUrl);
    return DEFAULT_AVATAR + encodeURIComponent(fullName || 'User');
  };

  const handleTabChange = (key) => {
    if (key === 'logout') {
      handleLogout();
      return;
    }
    if (key === 'change-password') {
      navigate('/profile/change-password');
      return;
    }
    if (key === 'order-history') {
      navigate('/account/orders');
      return;
    }
    if (key === 'address') {
      navigate('/account/addresses');
      return;
    }
    if (key === 'favorites') {
      navigate('/profile/favorites');
      return;
    }
    setActiveTab(key);
    setError('');
    setSuccess('');
    setFieldErrors({});
  };

  const handleLogout = () => {
    localStorage.removeItem('customer');
    window.dispatchEvent(new Event('customerLoginStateChange'));
    navigate('/');
  };

  // === CẬP NHẬT THÔNG TIN ===
  const validateInfo = () => {
    const errs = {};
    if (!fullName.trim() || fullName.trim().length < 2) errs.fullName = 'Họ và tên phải từ 2 ký tự trở lên';
    if (!email.trim() || !/\S+@\S+\.\S+/.test(email)) errs.email = 'Email không hợp lệ';
    if (phone && !/^[0-9]{9,11}$/.test(phone.replace(/\s/g, ''))) errs.phone = 'Số điện thoại không hợp lệ (9-11 số)';
    if (dateOfBirth && new Date(dateOfBirth) > new Date()) errs.dateOfBirth = 'Ngày sinh không được lớn hơn hôm nay';
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleUpdateInfo = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    if (!validateInfo()) return;
    setLoading(true);

    try {
      const response = await authService.updateProfile({
        customerId,
        fullName: fullName.trim(),
        email: email.trim(),
        phone: phone.trim(),
        address: address.trim(),
        dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : null,
        gender: gender || null,
      });

      // Cập nhật localStorage để Header đồng bộ
      const stored = localStorage.getItem('customer');
      if (stored) {
        const parsed = JSON.parse(stored);
        parsed.fullName = response.fullName;
        parsed.email = response.email;
        parsed.phone = response.phone;
        parsed.address = response.address;
        parsed.dateOfBirth = response.dateOfBirth;
        parsed.gender = response.gender;
        parsed.avatarUrl = response.avatarUrl;
        localStorage.setItem('customer', JSON.stringify(parsed));
      }

      setOriginalData({
        fullName: response.fullName, email: response.email, phone: response.phone || '',
        address: response.address || '', gender: response.gender || '', dateOfBirth: response.dateOfBirth || '',
      });
      setSuccess('Cập nhật thông tin tài khoản thành công!');
      window.dispatchEvent(new Event('customerLoginStateChange'));
    } catch (err) {
      setError(err.response?.data?.message || 'Cập nhật thất bại. Vui lòng kiểm tra lại.');
    } finally {
      setLoading(false);
    }
  };

  const handleResetForm = () => {
    setFullName(originalData.fullName || '');
    setEmail(originalData.email || '');
    setPhone(originalData.phone || '');
    setAddress(originalData.address || '');
    setGender(originalData.gender || '');
    if (originalData.dateOfBirth) {
      const d = new Date(originalData.dateOfBirth);
      setDateOfBirth(`${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`);
    } else {
      setDateOfBirth('');
    }
    setFieldErrors({});
    setError('');
    setSuccess('');
  };

  // === UPLOAD AVATAR ===
  const handleAvatarChange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    if (!allowedTypes.includes(file.type)) {
      setError('Chỉ chấp nhận file ảnh JPG, PNG hoặc WEBP');
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      setError('Dung lượng ảnh tối đa 2MB');
      return;
    }

    // Preview
    const reader = new FileReader();
    reader.onload = (ev) => setAvatarPreview(ev.target.result);
    reader.readAsDataURL(file);

    try {
      setError('');
      setSuccess('');
      const res = await authService.uploadAvatar(customerId, file);
      setAvatarUrl(res.avatarUrl);
      setAvatarPreview('');
      setSuccess('Cập nhật ảnh đại diện thành công!');

      // Cập nhật localStorage
      const stored = localStorage.getItem('customer');
      if (stored) {
        const parsed = JSON.parse(stored);
        parsed.avatarUrl = res.avatarUrl;
        localStorage.setItem('customer', JSON.stringify(parsed));
      }
      window.dispatchEvent(new Event('customerLoginStateChange'));
    } catch (err) {
      setError(err.response?.data?.message || 'Upload ảnh thất bại');
      setAvatarPreview('');
    }
  };

  // === ĐỔI MẬT KHẨU ===
  const handleChangePassword = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    const errs = {};
    if (!oldPassword) errs.oldPassword = 'Vui lòng nhập mật khẩu hiện tại';
    if (!newPassword || newPassword.length < 6) errs.newPassword = 'Mật khẩu mới tối thiểu 6 ký tự';
    if (newPassword !== confirmNewPassword) errs.confirmNewPassword = 'Xác nhận mật khẩu không khớp';
    if (newPassword && oldPassword && newPassword === oldPassword) errs.newPassword = 'Mật khẩu mới không được giống mật khẩu cũ';
    setFieldErrors(errs);
    if (Object.keys(errs).length > 0) return;

    setLoading(true);
    try {
      await authService.changePassword({ customerId, oldPassword, newPassword });
      setSuccess('Đổi mật khẩu thành công!');
      setOldPassword('');
      setNewPassword('');
      setConfirmNewPassword('');
    } catch (err) {
      setError(err.response?.data?.message || 'Đổi mật khẩu thất bại');
    } finally {
      setLoading(false);
    }
  };

  // === LOADING SKELETON ===
  if (fetching) {
    return (
      <div className="profile-page">
        <div className="profile-skeleton">
          <div className="profile-skeleton-sidebar">
            <div className="skeleton-block" style={{ height: '320px' }}></div>
          </div>
          <div className="profile-skeleton-content">
            <div className="skeleton-block" style={{ height: '420px' }}></div>
          </div>
        </div>
      </div>
    );
  }

  // === RENDER TAB CONTENT ===
  const renderTabContent = () => {
    switch (activeTab) {
      case 'info':
        return renderInfoTab();
      case 'address':
        return renderEmptyState('fa-solid fa-location-dot', 'Chưa có địa chỉ nào', 'Bạn chưa thêm địa chỉ giao hàng nào. Thêm địa chỉ để tiết kiệm thời gian khi đặt hàng.', 'Thêm địa chỉ mới', null, 'Sổ địa chỉ', 'Quản lý danh sách địa chỉ giao hàng của bạn.');
      case 'change-password':
        return renderChangePasswordTab();
      default:
        return renderInfoTab();
    }
  };

  // === TAB: THÔNG TIN TÀI KHOẢN ===
  const renderInfoTab = () => (
    <>
      <div className="profile-content-card">
        <h2 className="profile-content-title">Thông tin tài khoản</h2>
        <p className="profile-content-subtitle">Quản lý và cập nhật thông tin cá nhân của bạn.</p>

        {error && <div className="profile-alert profile-alert-error"><i className="fa-solid fa-circle-exclamation"></i> {error}</div>}
        {success && <div className="profile-alert profile-alert-success"><i className="fa-solid fa-circle-check"></i> {success}</div>}

        <form onSubmit={handleUpdateInfo}>
          <div className="profile-info-layout">
            {/* Phần form bên trái */}
            <div className="profile-info-form">
              <div className="profile-form-row">
                <div className="profile-form-group">
                  <label>Họ và tên</label>
                  <input type="text" className={`profile-form-input ${fieldErrors.fullName ? 'is-invalid' : ''}`} value={fullName} onChange={(e) => setFullName(e.target.value)} placeholder="Nhập họ và tên..." />
                  {fieldErrors.fullName && <div className="profile-form-error">{fieldErrors.fullName}</div>}
                </div>
                <div className="profile-form-group">
                  <label>Email</label>
                  <input type="email" className={`profile-form-input ${fieldErrors.email ? 'is-invalid' : ''}`} value={email} onChange={(e) => setEmail(e.target.value)} placeholder="email@example.com" readOnly style={{ background: '#f8f9fa', cursor: 'not-allowed' }} />
                  {fieldErrors.email && <div className="profile-form-error">{fieldErrors.email}</div>}
                </div>
              </div>

              <div className="profile-form-row">
                <div className="profile-form-group">
                  <label>Số điện thoại</label>
                  <input type="tel" className={`profile-form-input ${fieldErrors.phone ? 'is-invalid' : ''}`} value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="0901 234 567" />
                  {fieldErrors.phone && <div className="profile-form-error">{fieldErrors.phone}</div>}
                </div>
                <div className="profile-form-group">
                  <label>Địa chỉ mặc định</label>
                  <textarea 
                    className="profile-form-input" 
                    value={defaultAddressObj ? `${defaultAddressObj.addressLine}, ${defaultAddressObj.wardName}, ${defaultAddressObj.districtName}, ${defaultAddressObj.provinceName}` : address} 
                    readOnly 
                    rows={2}
                    style={{ background: '#f8f9fa', cursor: 'not-allowed', resize: 'none', height: 'auto', lineHeight: '1.4' }}
                    title="Vui lòng cập nhật trong mục Sổ địa chỉ nhận hàng"
                    placeholder="Chưa thiết lập địa chỉ mặc định..." 
                  />
                </div>
              </div>

              <div className="profile-form-row">
                <div className="profile-form-group">
                  <label>Ngày tạo tài khoản</label>
                  <input type="text" className="profile-form-input" value={createdAt ? new Date(createdAt).toLocaleString('vi-VN') : 'Không có thông tin'} readOnly />
                </div>
                <div className="profile-form-group">
                </div>
              </div>

              <div className="profile-btn-group">
                <button type="submit" className="profile-btn-save" disabled={loading}>
                  {loading ? (
                    <><span className="spinner-border spinner-border-sm mr-2" role="status"></span> Đang lưu...</>
                  ) : (
                    <><i className="fa-solid fa-floppy-disk"></i> Cập nhật thông tin</>
                  )}
                </button>
                <button type="button" className="profile-btn-cancel" onClick={handleResetForm}>Hủy thay đổi</button>
              </div>
            </div>

            {/* Phần avatar bên phải */}
            <div className="profile-avatar-upload">
              <img src={getAvatarSrc()} alt="Ảnh đại diện" className="profile-avatar-preview" />
              <span className="profile-avatar-label">Ảnh đại diện</span>
              <span className="profile-avatar-hint">JPG, PNG Tối đa 2MB</span>
              <input type="file" accept="image/jpeg,image/png,image/webp" ref={avatarInputRef} style={{ display: 'none' }} onChange={handleAvatarChange} />
              <button type="button" className="profile-avatar-change-btn" onClick={() => avatarInputRef.current?.click()}>
                <i className="fa-solid fa-cloud-arrow-up"></i> Đổi ảnh
              </button>
            </div>
          </div>
        </form>
      </div>

      {/* Bottom cards */}
      <div className="profile-bottom-cards">
        {/* Địa chỉ mặc định */}
        <div className="profile-bottom-card">
          <div className="profile-bottom-card-header">
            <span className="profile-bottom-card-title">Địa chỉ mặc định</span>
            <span className="profile-bottom-card-link" onClick={() => handleTabChange('address')}>Quản lý địa chỉ</span>
          </div>
          {defaultAddressObj ? (
            <div className="profile-address-info">
              <div className="profile-address-icon"><i className="fa-solid fa-house"></i></div>
              <div className="profile-address-detail">
                <div className="profile-address-label">
                  <span className="profile-address-type">{defaultAddressObj.addressType || 'Nhà riêng'}</span>
                  <span className="profile-address-badge">Mặc định</span>
                </div>
                <p className="profile-address-text">{`${defaultAddressObj.addressLine}, ${defaultAddressObj.wardName}, ${defaultAddressObj.districtName}, ${defaultAddressObj.provinceName}`}</p>
                <p className="profile-address-text">{defaultAddressObj.recipientName} - {defaultAddressObj.phoneNumber || 'Chưa có SĐT'}</p>
              </div>
            </div>
          ) : address ? (
            <div className="profile-address-info">
              <div className="profile-address-icon"><i className="fa-solid fa-house"></i></div>
              <div className="profile-address-detail">
                <div className="profile-address-label">
                  <span className="profile-address-type">Nhà riêng</span>
                  <span className="profile-address-badge">Mặc định</span>
                </div>
                <p className="profile-address-text">{address}</p>
                <p className="profile-address-text">{fullName} - {phone || 'Chưa có SĐT'}</p>
              </div>
            </div>
          ) : (
            <div style={{ textAlign: 'center', padding: '16px 0', color: '#999', fontSize: '0.85rem' }}>
              <i className="fa-solid fa-location-dot" style={{ fontSize: '1.5rem', marginBottom: '8px', display: 'block', opacity: 0.4 }}></i>
              Chưa có địa chỉ mặc định
            </div>
          )}
        </div>

        {/* Thông tin nhanh */}
        <div className="profile-bottom-card">
          <div className="profile-bottom-card-header">
            <span className="profile-bottom-card-title">Thông tin nhanh</span>
          </div>
          <div className="profile-quick-stats">
            <Link to="/account/orders" className="profile-stat-item">
              <div className="profile-stat-icon orders"><i className="fa-solid fa-box"></i></div>
              <span className="profile-stat-number">{totalOrders}</span>
              <span className="profile-stat-label">Đơn hàng gần đây</span>
              <span className="profile-stat-link">Xem đơn hàng</span>
            </Link>
            <Link to="/profile/favorites" className="profile-stat-item">
              <div className="profile-stat-icon wishlist" style={{ background: '#FFF0F2', color: '#CF102D' }}><i className="fa-solid fa-heart"></i></div>
              <span className="profile-stat-number">{favoriteCount}</span>
              <span className="profile-stat-label">Sản phẩm yêu thích</span>
              <span className="profile-stat-link">Xem danh sách</span>
            </Link>
            <Link to="/account/addresses" className="profile-stat-item">
              <div className="profile-stat-icon address"><i className="fa-solid fa-location-dot"></i></div>
              <span className="profile-stat-number">1</span>
              <span className="profile-stat-label">Quản lý địa chỉ</span>
              <span className="profile-stat-link">Xem sổ địa chỉ</span>
            </Link>
            <Link to="/profile/change-password" className="profile-stat-item">
              <div className="profile-stat-icon notifications"><i className="fa-solid fa-key"></i></div>
              <span className="profile-stat-number">1</span>
              <span className="profile-stat-label">Bảo mật tài khoản</span>
              <span className="profile-stat-link">Đổi mật khẩu</span>
            </Link>
          </div>
        </div>
      </div>
    </>
  );

  // === TAB: ĐỔI MẬT KHẨU ===
  const renderChangePasswordTab = () => (
    <div className="profile-content-card">
      <h2 className="profile-content-title">Đổi mật khẩu</h2>
      <p className="profile-content-subtitle">Để bảo mật tài khoản, vui lòng không chia sẻ mật khẩu cho người khác.</p>

      {error && <div className="profile-alert profile-alert-error"><i className="fa-solid fa-circle-exclamation"></i> {error}</div>}
      {success && <div className="profile-alert profile-alert-success"><i className="fa-solid fa-circle-check"></i> {success}</div>}

      <form onSubmit={handleChangePassword} style={{ maxWidth: '480px' }}>
        <div className="profile-form-group">
          <label>Mật khẩu hiện tại *</label>
          <div className="profile-password-wrapper">
            <input type={showOldPw ? 'text' : 'password'} className={`profile-form-input ${fieldErrors.oldPassword ? 'is-invalid' : ''}`} value={oldPassword} onChange={(e) => setOldPassword(e.target.value)} placeholder="Nhập mật khẩu hiện tại..." />
            <button type="button" className="profile-password-toggle" onClick={() => setShowOldPw(!showOldPw)}><i className={showOldPw ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i></button>
          </div>
          {fieldErrors.oldPassword && <div className="profile-form-error">{fieldErrors.oldPassword}</div>}
        </div>

        <div className="profile-form-group">
          <label>Mật khẩu mới *</label>
          <div className="profile-password-wrapper">
            <input type={showNewPw ? 'text' : 'password'} className={`profile-form-input ${fieldErrors.newPassword ? 'is-invalid' : ''}`} value={newPassword} onChange={(e) => setNewPassword(e.target.value)} placeholder="Tối thiểu 6 ký tự..." />
            <button type="button" className="profile-password-toggle" onClick={() => setShowNewPw(!showNewPw)}><i className={showNewPw ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i></button>
          </div>
          {fieldErrors.newPassword && <div className="profile-form-error">{fieldErrors.newPassword}</div>}
        </div>

        <div className="profile-form-group">
          <label>Xác nhận mật khẩu mới *</label>
          <div className="profile-password-wrapper">
            <input type={showConfirmPw ? 'text' : 'password'} className={`profile-form-input ${fieldErrors.confirmNewPassword ? 'is-invalid' : ''}`} value={confirmNewPassword} onChange={(e) => setConfirmNewPassword(e.target.value)} placeholder="Nhập lại mật khẩu mới..." />
            <button type="button" className="profile-password-toggle" onClick={() => setShowConfirmPw(!showConfirmPw)}><i className={showConfirmPw ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'}></i></button>
          </div>
          {fieldErrors.confirmNewPassword && <div className="profile-form-error">{fieldErrors.confirmNewPassword}</div>}
        </div>

        <div className="profile-btn-group">
          <button type="submit" className="profile-btn-save" disabled={loading}>
            {loading ? (
              <><span className="spinner-border spinner-border-sm mr-2" role="status"></span> Đang xử lý...</>
            ) : (
              <><i className="fa-solid fa-key"></i> Xác nhận đổi mật khẩu</>
            )}
          </button>
        </div>
      </form>
    </div>
  );

  // === EMPTY STATE CHUNG ===
  const renderEmptyState = (icon, title, desc, btnText, btnLink, pageTitle, pageSubtitle) => (
    <div className="profile-content-card">
      <h2 className="profile-content-title">{pageTitle}</h2>
      <p className="profile-content-subtitle">{pageSubtitle}</p>
      <div className="profile-empty-state">
        <div className="profile-empty-icon"><i className={icon}></i></div>
        <h3 className="profile-empty-title">{title}</h3>
        <p className="profile-empty-desc">{desc}</p>
        {btnText && (
          btnLink ? (
            <Link to={btnLink} className="profile-empty-btn"><i className="fa-solid fa-arrow-right"></i> {btnText}</Link>
          ) : (
            <button className="profile-empty-btn"><i className="fa-solid fa-plus"></i> {btnText}</button>
          )
        )}
      </div>
    </div>
  );

  return (
    <div className="profile-page">
      <div className="profile-layout">
        {/* ===== SIDEBAR ===== */}
        <AccountSidebar 
          activeKey={activeTab} 
          customer={{ fullName, email, avatarUrl: avatarPreview || avatarUrl }} 
          onLogout={handleLogout} 
        />

        {/* ===== MAIN CONTENT ===== */}
        <div className="profile-content">
          {renderTabContent()}
        </div>
      </div>
    </div>
  );
};

export default Profile;
