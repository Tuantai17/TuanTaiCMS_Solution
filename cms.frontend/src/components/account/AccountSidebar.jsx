import React from 'react';
import { useNavigate } from 'react-router-dom';
import { getMediaUrl } from '../../utils/mediaUrl';

const DEFAULT_AVATAR = 'https://ui-avatars.com/api/?background=c80f1e&color=fff&size=200&font-size=0.4&bold=true&name=';

const MENU_ITEMS = [
  { key: 'info', label: 'Thông tin tài khoản', icon: 'fa-solid fa-user' },
  { key: 'address', label: 'Sổ địa chỉ', icon: 'fa-solid fa-location-dot' },
  { key: 'order-history', label: 'Lịch sử mua hàng', icon: 'fa-solid fa-clock-rotate-left' },
  { key: 'change-password', label: 'Đổi mật khẩu', icon: 'fa-solid fa-key' },
  { key: 'logout', label: 'Đăng xuất', icon: 'fa-solid fa-right-from-bracket', isLogout: true },
];

function AccountSidebar({ activeKey, customer, onLogout }) {
  const navigate = useNavigate();

  const handleItemClick = (key) => {
    if (key === 'logout') {
      onLogout?.();
      return;
    }

    if (key === 'info') {
      navigate('/profile');
      return;
    }

    if (key === 'address') {
      navigate('/account/addresses');
      return;
    }

    if (key === 'order-history') {
      navigate('/account/orders');
      return;
    }

    if (key === 'change-password') {
      navigate('/profile/change-password');
      return;
    }

    navigate('/profile', { state: { profileTab: key } });
  };

  const avatarSrc = customer?.avatarUrl
    ? getMediaUrl(customer.avatarUrl)
    : `${DEFAULT_AVATAR}${encodeURIComponent(customer?.fullName || 'User')}`;

  return (
    <aside className="profile-sidebar">
      <div className="profile-sidebar-card">
        <div className="profile-sidebar-header">
          <div className="profile-avatar-wrapper profile-avatar-wrapper-static">
            <img src={avatarSrc} alt={customer?.fullName || 'Khách hàng'} className="profile-avatar-img" />
          </div>
          <h4 className="profile-sidebar-name">{customer?.fullName || 'Khách hàng'}</h4>
          <p className="profile-sidebar-email">{customer?.email || 'Chưa có email'}</p>
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
                  className={`profile-menu-item ${activeKey === item.key ? 'active' : ''} ${item.isLogout ? 'logout-item' : ''}`}
                  onClick={() => handleItemClick(item.key)}
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
  );
}

export default AccountSidebar;
