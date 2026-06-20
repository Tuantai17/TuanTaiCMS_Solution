import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import addressService from '../services/addressService';
import { provincesData } from '../utils/addressData';
import { getMediaUrl } from '../utils/mediaUrl';
import '../assets/css/AddressesPage.css';

const DEFAULT_AVATAR = 'https://ui-avatars.com/api/?background=c80f1e&color=fff&size=200&font-size=0.4&bold=true&name=';

const MENU_ITEMS = [
  { key: 'info', label: 'Thông tin tài khoản', icon: 'fa-solid fa-user' },
  { key: 'address', label: 'Sổ địa chỉ', icon: 'fa-solid fa-location-dot' },
  { key: 'order-history', label: 'Lịch sử mua hàng', icon: 'fa-solid fa-clock-rotate-left' },
  { key: 'change-password', label: 'Đổi mật khẩu', icon: 'fa-solid fa-key' },
  { key: 'logout', label: 'Đăng xuất', icon: 'fa-solid fa-right-from-bracket', isLogout: true },
];

const AddressesPage = () => {
  const navigate = useNavigate();

  // State thông tin khách hàng
  const [customer, setCustomer] = useState(null);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  const [totalOrders, setTotalOrders] = useState(0);

  // State danh sách địa chỉ
  const [addresses, setAddresses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  // State Modal (Thêm/Sửa)
  const [showModal, setShowModal] = useState(false);
  const [modalMode, setModalMode] = useState('create'); // 'create' hoặc 'edit'
  const [selectedAddress, setSelectedAddress] = useState(null);

  // State Form fields
  const [recipientName, setRecipientName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [provinceName, setProvinceName] = useState('');
  const [districtName, setDistrictName] = useState('');
  const [wardName, setWardName] = useState('');
  const [addressLine, setAddressLine] = useState('');
  const [addressType, setAddressType] = useState('Nhà riêng');
  const [isDefault, setIsDefault] = useState(false);

  // State lỗi nhập liệu
  const [formErrors, setFormErrors] = useState({});

  // State Modal xác nhận xóa
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [addressToDelete, setAddressToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  // State Toast thông báo nhanh
  const [toast, setToast] = useState(null); // { message, type: 'success'|'error' }

  // Danh sách quận/huyện và phường/xã khả dụng dựa trên tỉnh thành được chọn
  const [availableDistricts, setAvailableDistricts] = useState([]);
  const [availableWards, setAvailableWards] = useState([]);

  // Load thông tin khách hàng và danh sách địa chỉ
  useEffect(() => {
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      navigate('/login');
      return;
    }

    try {
      const parsed = JSON.parse(storedCustomer);
      setCustomer(parsed);
      setFullName(parsed.fullName || '');
      setEmail(parsed.email || '');
      setAvatarUrl(parsed.avatarUrl || '');

      // Tải thông tin tài khoản hoàn chỉnh từ API để lấy totalOrders thực tế
      authService.getProfile(parsed.customerId).then(res => {
        setTotalOrders(res.totalOrders || 0);
        if (res.avatarUrl !== parsed.avatarUrl) {
          setAvatarUrl(res.avatarUrl || '');
          parsed.avatarUrl = res.avatarUrl;
          localStorage.setItem('customer', JSON.stringify(parsed));
        }
      }).catch(e => console.error("Lỗi lấy thông tin cá nhân:", e));

      // Tải danh sách địa chỉ
      fetchAddresses(parsed.customerId);

    } catch (e) {
      localStorage.removeItem('customer');
      navigate('/login');
    }
  }, [navigate]);

  const fetchAddresses = async (cid) => {
    try {
      setLoading(true);
      const data = await addressService.getAddresses(cid);
      setAddresses(data || []);
    } catch (err) {
      console.error("Lỗi lấy danh sách địa chỉ:", err);
      showToastMessage("Không thể tải danh sách địa chỉ. Vui lòng thử lại sau.", "error");
    } finally {
      setLoading(false);
    }
  };

  const showToastMessage = (message, type = 'success') => {
    setToast({ message, type });
    setTimeout(() => {
      setToast(null);
    }, 3000);
  };

  // Đồng bộ Tỉnh -> Quận -> Phường
  useEffect(() => {
    if (!provinceName) {
      setAvailableDistricts([]);
      setAvailableWards([]);
      return;
    }
    const foundProv = provincesData.find(p => p.name === provinceName);
    if (foundProv) {
      setAvailableDistricts(foundProv.districts || []);
    } else {
      setAvailableDistricts([]);
    }
    setAvailableWards([]);
  }, [provinceName]);

  useEffect(() => {
    if (!districtName || availableDistricts.length === 0) {
      setAvailableWards([]);
      return;
    }
    const foundDist = availableDistricts.find(d => d.name === districtName);
    if (foundDist) {
      setAvailableWards(foundDist.wards || []);
    } else {
      setAvailableWards([]);
    }
  }, [districtName, availableDistricts]);

  const handleTabChange = (key) => {
    if (key === 'logout') {
      localStorage.removeItem('customer');
      window.dispatchEvent(new Event('customerLoginStateChange'));
      navigate('/');
      return;
    }
    if (key === 'address') return;
    if (key === 'order-history') {
      navigate('/account/orders');
      return;
    }
    navigate(`/profile?tab=${key}`);
  };

  // Mở Modal
  const openAddressModal = (mode, addr = null) => {
    setModalMode(mode);
    setFormErrors({});
    
    if (mode === 'edit' && addr) {
      setSelectedAddress(addr);
      setRecipientName(addr.recipientName || '');
      setPhoneNumber(addr.phoneNumber || '');
      setProvinceName(addr.provinceName || '');
      
      // Sử dụng setTimeout để đợi hiệu ứng đồng bộ districts và wards
      setTimeout(() => {
        setDistrictName(addr.districtName || '');
        setTimeout(() => {
          setWardName(addr.wardName || '');
        }, 50);
      }, 50);

      setAddressLine(addr.addressLine || '');
      setAddressType(addr.addressType || 'Nhà riêng');
      setIsDefault(addr.isDefault || false);
    } else {
      setSelectedAddress(null);
      setRecipientName('');
      setPhoneNumber('');
      setProvinceName('');
      setDistrictName('');
      setWardName('');
      setAddressLine('');
      setAddressType('Nhà riêng');
      setIsDefault(false);
    }
    setShowModal(true);
  };

  const closeAddressModal = () => {
    setShowModal(false);
    setSelectedAddress(null);
  };

  // Validate form
  const validateForm = () => {
    const errs = {};
    if (!recipientName.trim()) {
      errs.recipientName = 'Họ và tên người nhận không được để trống';
    } else if (recipientName.trim().length < 2 || recipientName.trim().length > 100) {
      errs.recipientName = 'Họ và tên phải từ 2 đến 100 ký tự';
    }

    const cleanPhone = phoneNumber.replace(/[\s\-\(\)]/g, '');
    if (!phoneNumber.trim()) {
      errs.phoneNumber = 'Số điện thoại không được để trống';
    } else if (!/^[0-9]{9,11}$/.test(cleanPhone)) {
      errs.phoneNumber = 'Số điện thoại không hợp lệ (phải từ 9 đến 11 số)';
    }

    if (!provinceName) errs.provinceName = 'Vui lòng chọn Tỉnh/Thành phố';
    if (!districtName) errs.districtName = 'Vui lòng chọn Quận/Huyện';
    if (!wardName) errs.wardName = 'Vui lòng chọn Phường/Xã';

    if (!addressLine.trim()) {
      errs.addressLine = 'Địa chỉ chi tiết không được để trống';
    }

    setFormErrors(errs);
    return Object.keys(errs).length === 0;
  };

  // Xử lý Lưu địa chỉ (Thêm hoặc Cập nhật)
  const handleSaveAddress = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    setSubmitting(true);
    const payload = {
      customerId: customer.customerId,
      recipientName: recipientName.trim(),
      phoneNumber: phoneNumber.trim(),
      provinceName,
      districtName,
      wardName,
      addressLine: addressLine.trim(),
      addressType,
      isDefault
    };

    try {
      let updatedAddr = null;
      if (modalMode === 'create') {
        updatedAddr = await addressService.createAddress(payload);
        showToastMessage("Thêm địa chỉ giao hàng thành công!");
      } else {
        payload.id = selectedAddress.id;
        updatedAddr = await addressService.updateAddress(selectedAddress.id, payload);
        showToastMessage("Cập nhật địa chỉ thành công!");
      }

      // Đồng bộ thông tin ở localStorage nếu địa chỉ được tạo/sửa là mặc định
      if (updatedAddr && updatedAddr.isDefault) {
        const fullAddrStr = `${updatedAddr.addressLine}, ${updatedAddr.wardName}, ${updatedAddr.districtName}, ${updatedAddr.provinceName}`;
        const stored = localStorage.getItem('customer');
        if (stored) {
          const parsed = JSON.parse(stored);
          parsed.address = fullAddrStr;
          localStorage.setItem('customer', JSON.stringify(parsed));
          window.dispatchEvent(new Event('customerLoginStateChange'));
        }
      }

      closeAddressModal();
      fetchAddresses(customer.customerId);
    } catch (err) {
      console.error("Lỗi khi lưu địa chỉ:", err);
      showToastMessage(err.response?.data?.message || "Lưu địa chỉ thất bại. Vui lòng thử lại.", "error");
    } finally {
      setSubmitting(false);
    }
  };

  // Thiết lập mặc định
  const handleSetDefault = async (addr) => {
    if (addr.isDefault) return;

    try {
      await addressService.setDefaultAddress(addr.id, customer.customerId);
      showToastMessage("Đặt địa chỉ mặc định thành công!");

      // Đồng bộ localStorage
      const fullAddrStr = `${addr.addressLine}, ${addr.wardName}, ${addr.districtName}, ${addr.provinceName}`;
      const stored = localStorage.getItem('customer');
      if (stored) {
        const parsed = JSON.parse(stored);
        parsed.address = fullAddrStr;
        localStorage.setItem('customer', JSON.stringify(parsed));
        window.dispatchEvent(new Event('customerLoginStateChange'));
      }

      fetchAddresses(customer.customerId);
    } catch (err) {
      console.error("Lỗi khi đặt địa chỉ mặc định:", err);
      showToastMessage("Đặt mặc định thất bại.", "error");
    }
  };

  // Xác nhận xóa địa chỉ
  const openDeleteConfirm = (addr) => {
    if (addr.isDefault && addresses.length > 1) {
      // Nếu là địa chỉ mặc định duy nhất thì có thể xóa, còn nếu có nhiều địa chỉ thì khuyên đặt cái khác làm mặc định trước
      // Tuy nhiên ở Backend chúng ta đã viết: Nếu xóa mặc định thì tự chuyển sang địa chỉ khác. Nên cho phép xóa bình thường.
    }
    setAddressToDelete(addr);
    setShowDeleteConfirm(true);
  };

  const closeDeleteConfirm = () => {
    setShowDeleteConfirm(false);
    setAddressToDelete(null);
  };

  const handleDeleteAddress = async () => {
    if (!addressToDelete) return;
    setDeleting(true);

    try {
      await addressService.deleteAddress(addressToDelete.id, customer.customerId);
      showToastMessage("Xóa địa chỉ giao hàng thành công!");

      // Nếu địa chỉ bị xóa là địa chỉ mặc định thì backend tự gán cái khác làm mặc định (nếu còn).
      // Chúng ta gọi fetchAddresses để cập nhật danh sách và xem cái nào thành mặc định mới.
      const data = await addressService.getAddresses(customer.customerId);
      setAddresses(data || []);

      // Đồng bộ lại địa chỉ mặc định mới vào localStorage
      const nextDefault = data?.find(a => a.isDefault);
      const stored = localStorage.getItem('customer');
      if (stored) {
        const parsed = JSON.parse(stored);
        if (nextDefault) {
          parsed.address = `${nextDefault.addressLine}, ${nextDefault.wardName}, ${nextDefault.districtName}, ${nextDefault.provinceName}`;
        } else {
          parsed.address = null;
        }
        localStorage.setItem('customer', JSON.stringify(parsed));
        window.dispatchEvent(new Event('customerLoginStateChange'));
      }

      closeDeleteConfirm();
    } catch (err) {
      console.error("Lỗi xóa địa chỉ:", err);
      showToastMessage("Xóa địa chỉ thất bại.", "error");
    } finally {
      setDeleting(false);
    }
  };

  // Điểm trang trí avatar bên trái
  const getAvatarSrc = () => {
    if (avatarUrl) return getMediaUrl(avatarUrl);
    return DEFAULT_AVATAR + encodeURIComponent(fullName || 'User');
  };

  const getAddressIcon = (type) => {
    switch (type) {
      case 'Nhà riêng':
        return 'fa-solid fa-house';
      case 'Văn phòng':
        return 'fa-solid fa-briefcase';
      default:
        return 'fa-solid fa-location-dot';
    }
  };

  return (
    <div className="address-page">
      <div className="address-layout">
        {/* ===== SIDEBAR TÀI KHOẢN ===== */}
        <div className="address-sidebar">
          <div className="address-sidebar-card">
            <div className="address-sidebar-header">
              <div className="address-avatar-wrapper">
                <img src={getAvatarSrc()} alt={fullName} className="address-avatar-img" />
              </div>
              <h4 className="address-sidebar-name">{fullName}</h4>
              <p className="address-sidebar-email">{email}</p>
              <span className="address-member-badge"><i className="fa-solid fa-crown"></i> Thành viên</span>
            </div>

            <ul className="address-sidebar-menu">
              {MENU_ITEMS.map((item) => (
                <React.Fragment key={item.key}>
                  {item.isLogout && <li><div className="address-menu-divider"></div></li>}
                  <li>
                    <button
                      type="button"
                      className={`address-menu-item ${item.key === 'address' ? 'active' : ''} ${item.isLogout ? 'logout-item' : ''}`}
                      onClick={() => handleTabChange(item.key)}
                    >
                      <i className={item.icon}></i>
                      <span>{item.label}</span>
                    </button>
                  </li>
                </React.Fragment>
              ))}
            </ul>
          </div>
        </div>

        {/* ===== NỘI DUNG CHÍNH ===== */}
        <div className="address-content animate--fade-in">
          <div className="address-card-container">
            <div className="address-header-row">
              <div className="address-title-group">
                <h2>Sổ địa chỉ nhận hàng</h2>
                <p>Quản lý và sử dụng các địa chỉ nhận hàng của bạn.</p>
              </div>
              <button 
                type="button" 
                className="btn-add-address"
                onClick={() => openAddressModal('create')}
              >
                <i className="fa-solid fa-plus"></i> Thêm địa chỉ mới
              </button>
            </div>

            {loading ? (
              <div className="address-grid">
                {[1, 2, 3].map(i => (
                  <div key={i} className="address-skeleton-card">
                    <div className="skeleton-line" style={{ height: '24px', width: '60%' }}></div>
                    <div className="skeleton-line" style={{ height: '18px', width: '80%' }}></div>
                    <div className="skeleton-line" style={{ height: '18px', width: '50%' }}></div>
                    <div className="skeleton-line" style={{ height: '16px', width: '90%' }}></div>
                    <div className="skeleton-line" style={{ height: '32px', width: '100%', marginTop: 'auto' }}></div>
                  </div>
                ))}
              </div>
            ) : addresses.length === 0 ? (
              <div className="address-empty-state">
                <div className="address-empty-icon"><i className="fa-solid fa-location-dot"></i></div>
                <h3 className="address-empty-title">Bạn chưa có địa chỉ giao hàng nào</h3>
                <p className="address-empty-desc">Hãy thêm địa chỉ giao hàng để thực hiện thanh toán mua sắm tại MyKingdom nhanh chóng hơn.</p>
                <button 
                  type="button" 
                  className="btn-add-address"
                  onClick={() => openAddressModal('create')}
                >
                  <i className="fa-solid fa-plus"></i> Thêm địa chỉ đầu tiên
                </button>
              </div>
            ) : (
              <div className="address-grid">
                {addresses.map((addr) => (
                  <div key={addr.id} className={`address-item-card ${addr.isDefault ? 'is-default' : ''}`}>
                    <div className="address-card-header">
                      <div className="address-badges">
                        <span className={`badge-addr-type ${addr.addressType.toLowerCase().replace(' ', '-')}`}>
                          <i className={getAddressIcon(addr.addressType)}></i> {addr.addressType}
                        </span>
                        {addr.isDefault && <span className="badge-addr-default"><i className="fa-solid fa-star"></i> Mặc định</span>}
                      </div>
                    </div>

                    <div className="address-details-body">
                      <h4 className="address-recipient-name">{addr.recipientName}</h4>
                      <p className="address-recipient-phone"><i className="fa-solid fa-phone"></i> {addr.phoneNumber}</p>
                      <p className="address-text-block">
                        <i className="fa-solid fa-location-dot"></i>
                        {addr.addressLine}, {addr.wardName}, {addr.districtName}, {addr.provinceName}
                      </p>
                    </div>

                    <div className="address-card-footer">
                      <button 
                        type="button" 
                        className="btn-card-action btn-edit"
                        onClick={() => openAddressModal('edit', addr)}
                        title="Chỉnh sửa địa chỉ"
                      >
                        <i className="fa-solid fa-pen-to-square"></i> Sửa
                      </button>
                      
                      {!addr.isDefault ? (
                        <>
                          <button 
                            type="button" 
                            className="btn-card-action btn-set-default"
                            onClick={() => handleSetDefault(addr)}
                            title="Đặt làm địa chỉ nhận hàng mặc định"
                          >
                            <i className="fa-regular fa-star"></i> Mặc định
                          </button>
                          <button 
                            type="button" 
                            className="btn-card-action btn-delete"
                            onClick={() => openDeleteConfirm(addr)}
                            title="Xóa địa chỉ"
                          >
                            <i className="fa-solid fa-trash-can"></i> Xóa
                          </button>
                        </>
                      ) : (
                        <button 
                          type="button" 
                          className="btn-card-action btn-delete"
                          onClick={() => openDeleteConfirm(addr)}
                          title="Xóa địa chỉ mặc định"
                        >
                          <i className="fa-solid fa-trash-can"></i> Xóa
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* ===== KHU VỰC THỐNG KÊ PHÍA DƯỚI ===== */}
          <div className="address-stats-card">
            <div className="address-stats-layout">
              <div className="address-stats-user">
                <img src={getAvatarSrc()} alt={fullName} className="address-stats-avatar" />
                <div>
                  <h5 className="address-stats-user-name">{fullName}</h5>
                  <p className="address-stats-user-email">{email}</p>
                  <span className="badge badge-warning" style={{ fontSize: '0.68rem', fontWeight: 'bold' }}>Thành viên vàng</span>
                </div>
              </div>
              <div className="address-stats-grid">
                <div className="address-stat-item">
                  <div className="address-stat-icon saved"><i className="fa-solid fa-folder-open"></i></div>
                  <div className="address-stat-details">
                    <span className="address-stat-num">{addresses.length}</span>
                    <span className="address-stat-lbl">Địa chỉ đã lưu</span>
                  </div>
                </div>
                <div className="address-stat-item">
                  <div className="address-stat-icon default"><i className="fa-solid fa-star"></i></div>
                  <div className="address-stat-details">
                    <span className="address-stat-num">{addresses.some(a => a.isDefault) ? 1 : 0}</span>
                    <span className="address-stat-lbl">Địa chỉ mặc định</span>
                  </div>
                </div>
                <div className="address-stat-item">
                  <div className="address-stat-icon orders"><i className="fa-solid fa-truck-ramp-box"></i></div>
                  <div className="address-stat-details">
                    <span className="address-stat-num">{totalOrders}</span>
                    <span className="address-stat-lbl">Đơn hàng đã giao</span>
                  </div>
                </div>
                <div className="address-stat-item">
                  <div className="address-stat-icon orders"><i className="fa-solid fa-clock-rotate-left"></i></div>
                  <div className="address-stat-details">
                    <span className="address-stat-num">{totalOrders}</span>
                    <span className="address-stat-lbl">Lịch sử đơn hàng</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* ===== MODAL: THÊM / SỬA ĐỊA CHỈ ===== */}
      {showModal && (
        <div className="address-modal-overlay">
          <div className="address-modal-container">
            <div className="address-modal-header">
              <h3>{modalMode === 'create' ? 'Thêm địa chỉ giao hàng mới' : 'Chỉnh sửa địa chỉ nhận hàng'}</h3>
              <button type="button" className="btn-close-modal" onClick={closeAddressModal}>&times;</button>
            </div>

            <form onSubmit={handleSaveAddress}>
              <div className="address-modal-body">
                <div className="row">
                  <div className="col-12 col-md-6 form-group">
                    <label className="small font-weight-bold text-secondary">Họ và tên người nhận *</label>
                    <input 
                      type="text" 
                      className={`form-control rounded px-3 ${formErrors.recipientName ? 'is-invalid' : ''}`}
                      placeholder="Nhập họ tên người nhận..." 
                      value={recipientName}
                      onChange={(e) => setRecipientName(e.target.value)}
                    />
                    {formErrors.recipientName && <div className="invalid-feedback">{formErrors.recipientName}</div>}
                  </div>
                  <div className="col-12 col-md-6 form-group">
                    <label className="small font-weight-bold text-secondary">Số điện thoại liên hệ *</label>
                    <input 
                      type="tel" 
                      className={`form-control rounded px-3 ${formErrors.phoneNumber ? 'is-invalid' : ''}`}
                      placeholder="Ví dụ: 0912345678..." 
                      value={phoneNumber}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                    />
                    {formErrors.phoneNumber && <div className="invalid-feedback">{formErrors.phoneNumber}</div>}
                  </div>
                </div>

                <div className="row">
                  <div className="col-12 col-md-4 form-group">
                    <label className="small font-weight-bold text-secondary">Tỉnh / Thành phố *</label>
                    <select 
                      className={`form-control rounded ${formErrors.provinceName ? 'is-invalid' : ''}`}
                      value={provinceName}
                      onChange={(e) => {
                        setProvinceName(e.target.value);
                        setDistrictName('');
                        setWardName('');
                      }}
                    >
                      <option value="">-- Chọn Tỉnh/TP --</option>
                      {provincesData.map(p => (
                        <option key={p.name} value={p.name}>{p.name}</option>
                      ))}
                    </select>
                    {formErrors.provinceName && <div className="invalid-feedback">{formErrors.provinceName}</div>}
                  </div>
                  <div className="col-12 col-md-4 form-group">
                    <label className="small font-weight-bold text-secondary">Quận / Huyện *</label>
                    <select 
                      className={`form-control rounded ${formErrors.districtName ? 'is-invalid' : ''}`}
                      value={districtName}
                      onChange={(e) => {
                        setDistrictName(e.target.value);
                        setWardName('');
                      }}
                      disabled={!provinceName}
                    >
                      <option value="">-- Chọn Quận/Huyện --</option>
                      {availableDistricts.map(d => (
                        <option key={d.name} value={d.name}>{d.name}</option>
                      ))}
                    </select>
                    {formErrors.districtName && <div className="invalid-feedback">{formErrors.districtName}</div>}
                  </div>
                  <div className="col-12 col-md-4 form-group">
                    <label className="small font-weight-bold text-secondary">Phường / Xã *</label>
                    <select 
                      className={`form-control rounded ${formErrors.wardName ? 'is-invalid' : ''}`}
                      value={wardName}
                      onChange={(e) => setWardName(e.target.value)}
                      disabled={!districtName}
                    >
                      <option value="">-- Chọn Phường/Xã --</option>
                      {availableWards.map(w => (
                        <option key={w} value={w}>{w}</option>
                      ))}
                    </select>
                    {formErrors.wardName && <div className="invalid-feedback">{formErrors.wardName}</div>}
                  </div>
                </div>

                <div className="form-group">
                  <label className="small font-weight-bold text-secondary">Địa chỉ chi tiết (Số nhà, Tên đường) *</label>
                  <input 
                    type="text" 
                    className={`form-control rounded px-3 ${formErrors.addressLine ? 'is-invalid' : ''}`}
                    placeholder="Ví dụ: Số 23, Ngõ 45, Đường Lê Lợi..." 
                    value={addressLine}
                    onChange={(e) => setAddressLine(e.target.value)}
                  />
                  {formErrors.addressLine && <div className="invalid-feedback">{formErrors.addressLine}</div>}
                </div>

                <div className="form-group">
                  <label className="small font-weight-bold text-secondary mb-2">Loại địa chỉ</label>
                  <div className="address-type-selector">
                    {['Nhà riêng', 'Văn phòng', 'Khác'].map(type => (
                      <button
                        key={type}
                        type="button"
                        className={`btn-type-option ${addressType === type ? 'selected' : ''}`}
                        onClick={() => setAddressType(type)}
                      >
                        <i className={getAddressIcon(type)}></i> {type}
                      </button>
                    ))}
                  </div>
                </div>

                <div className="custom-control custom-checkbox mt-3">
                  <input 
                    type="checkbox" 
                    className="custom-control-input" 
                    id="isDefaultCheckbox"
                    checked={isDefault}
                    onChange={(e) => setIsDefault(e.target.checked)}
                    disabled={modalMode === 'edit' && selectedAddress?.isDefault} // Không cho bỏ check mặc định nếu đang là mặc định
                  />
                  <label className="custom-control-label font-weight-bold text-dark" htmlFor="isDefaultCheckbox">
                    Đặt làm địa chỉ giao hàng mặc định
                  </label>
                </div>
              </div>

              <div className="address-modal-footer">
                <button type="button" className="btn-modal-cancel" onClick={closeAddressModal}>Hủy bỏ</button>
                <button type="submit" className="btn-modal-save" disabled={submitting}>
                  {submitting ? 'Đang lưu...' : 'Lưu địa chỉ'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ===== MODAL XÁC NHẬN XÓA ===== */}
      {showDeleteConfirm && (
        <div className="address-modal-overlay">
          <div className="address-modal-container" style={{ maxWidth: '420px' }}>
            <div className="address-modal-header" style={{ background: '#dc3545' }}>
              <h3>Xác nhận xóa địa chỉ</h3>
              <button type="button" className="btn-close-modal" onClick={closeDeleteConfirm}>&times;</button>
            </div>
            <div className="address-modal-body text-center py-4">
              <i className="fa-solid fa-circle-exclamation text-danger mb-3" style={{ fontSize: '2.5rem' }}></i>
              <p className="mb-0" style={{ fontSize: '0.95rem', color: '#333' }}>
                Bạn có chắc chắn muốn xóa địa chỉ của <strong>{addressToDelete?.recipientName}</strong> không?
              </p>
              {addressToDelete?.isDefault && (
                <p className="text-warning small mt-2 mb-0">
                  <i className="fa-solid fa-triangle-exclamation"></i> Đây là địa chỉ mặc định hiện tại. Địa chỉ khác sẽ tự động được đặt làm mặc định.
                </p>
              )}
            </div>
            <div className="address-modal-footer">
              <button type="button" className="btn-modal-cancel" onClick={closeDeleteConfirm}>Hủy</button>
              <button 
                type="button" 
                className="btn btn-danger font-weight-bold rounded-lg px-4"
                style={{ fontSize: '0.85rem' }}
                onClick={handleDeleteAddress}
                disabled={deleting}
              >
                {deleting ? 'Đang xóa...' : 'Xóa địa chỉ'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ===== TOAST NOTIFICATION ===== */}
      {toast && (
        <div className={`address-toast ${toast.type}`}>
          {toast.type === 'success' ? (
            <i className="fa-solid fa-circle-check"></i>
          ) : (
            <i className="fa-solid fa-circle-exclamation"></i>
          )}
          <span>{toast.message}</span>
        </div>
      )}
    </div>
  );
};

export default AddressesPage;
