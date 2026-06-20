import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

function Register() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    password: '',
  });

  const handleChange = (event) => {
    setFormData((prev) => ({
      ...prev,
      [event.target.name]: event.target.value,
    }));
  };

  const validateForm = () => {
    if (!formData.fullName || !formData.email || !formData.password) {
      window.alert('⛔ LỖI: Vui lòng không bỏ trống Họ tên, Email và Mật khẩu!');
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      window.alert('⛔ LỖI: Định dạng Email không hợp lệ (Ví dụ đúng: NguyenVanA@gmail.com)!');
      return false;
    }

    if (formData.password.length < 6) {
      window.alert('⛔ LỖI: Mật khẩu phải chứa ít nhất 6 ký tự để đảm bảo an toàn!');
      return false;
    }

    return true;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      await authService.register(formData);
      window.alert('🎉 ĐĂNG KÝ THÀNH CÔNG! Chào mừng bạn đến với hệ thống. Hãy đăng nhập ngay.');
      navigate('/login');
    } catch (error) {
      const message =
        error?.response?.data?.message ||
        '⛔ ĐĂNG KÝ THẤT BẠI: Email này có thể đã được đăng ký trên hệ thống!';
      window.alert(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card auth-card">
            {/* Header Form MyKingdom */}
            <div className="text-white text-center py-4 px-3" style={{ background: 'linear-gradient(135deg, #CF102D, #ff3d57)' }}>
              <h4 className="font-weight-bold text-uppercase mb-1">
                <i className="fa-solid fa-user-plus mr-2"></i> ĐĂNG KÝ TÀI KHOẢN
              </h4>
              <p className="small mb-0 opacity-75">Tham gia thành viên để nhận ngập tràn ưu đãi từ MyKingdom</p>
            </div>

            <div className="card-body p-4">
              <form onSubmit={handleSubmit}>
                <div className="form-group mb-3">
                  <label className="small font-weight-bold text-secondary">Họ và Tên *</label>
                  <input
                    type="text"
                    name="fullName"
                    className="form-control"
                    value={formData.fullName}
                    onChange={handleChange}
                    placeholder="Nhập họ và tên của bạn"
                  />
                </div>

                <div className="form-group mb-3">
                  <label className="small font-weight-bold text-secondary">Email (Tài khoản đăng nhập) *</label>
                  <input
                    type="email"
                    name="email"
                    className="form-control"
                    value={formData.email}
                    onChange={handleChange}
                    placeholder="example@gmail.com"
                  />
                </div>

                <div className="row">
                  <div className="col-md-6 form-group mb-3">
                    <label className="small font-weight-bold text-secondary">Số Điện Thoại</label>
                    <input
                      type="text"
                      name="phone"
                      className="form-control"
                      value={formData.phone}
                      onChange={handleChange}
                      placeholder="090xxxxxxx"
                    />
                  </div>

                  <div className="col-md-6 form-group mb-3">
                    <label className="small font-weight-bold text-secondary">Mật Khẩu *</label>
                    <input
                      type="password"
                      name="password"
                      className="form-control"
                      value={formData.password}
                      onChange={handleChange}
                      placeholder="Tối thiểu 6 ký tự"
                    />
                  </div>
                </div>

                <div className="form-group mb-4">
                  <label className="small font-weight-bold text-secondary">Địa Chỉ Nhận Hàng</label>
                  <textarea
                    name="address"
                    className="form-control"
                    rows="2"
                    value={formData.address}
                    onChange={handleChange}
                    placeholder="Nhập số nhà, tên đường, quận/huyện..."
                  ></textarea>
                </div>

                <button
                  type="submit"
                  className="btn btn-danger btn-block w-100 py-2 font-weight-bold shadow-sm"
                  disabled={loading}
                >
                  {loading ? 'ĐANG XỬ LÝ...' : 'ĐĂNG KÝ NGAY'}
                </button>
              </form>

              <p className="text-center mt-3 small m-0 text-muted">
                Đã có tài khoản thành viên?{' '}
                <Link to="/login" className="font-weight-bold text-danger">
                  Đăng nhập tại đây
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Register;
