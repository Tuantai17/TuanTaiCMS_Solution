import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const Register = () => {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await authService.customerRegister({
        fullName,
        email,
        password,
        phone,
        address
      });

      setSuccess('Đăng ký tài khoản thành công!');
      
      // Tự động đăng nhập: Lưu thông tin trả về của Customer vào localStorage
      localStorage.setItem('customer', JSON.stringify({
        customerId: response.customerId,
        fullName: response.fullName,
        email: response.email,
        phone: phone,
        address: address
      }));
      
      // Phát sự kiện cập nhật trạng thái đăng nhập
      window.dispatchEvent(new Event('customerLoginStateChange'));

      // Chờ 1.5 giây để người dùng thấy thông báo thành công rồi chuyển hướng về Trang chủ
      setTimeout(() => {
        navigate('/');
      }, 1500);
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container my-5 animate--fade-in">
      <div className="row justify-content-center">
        <div className="col-12 col-md-8 col-lg-6">
          <div className="card shadow-lg border-0 rounded-lg overflow-hidden">
            {/* Header Form mang bản sắc MyKingdom */}
            <div className="bg-danger text-white text-center py-4 px-3" style={{ background: 'linear-gradient(135deg, #CF102D, #ff3d57)' }}>
              <h4 className="font-weight-bold text-uppercase mb-1">
                <i className="fa-solid fa-user-plus mr-2"></i> Đăng Ký Thành Viên
              </h4>
              <p className="small mb-0 opacity-75">Tham gia vương quốc đồ chơi để nhận nhiều ưu đãi đặc biệt</p>
            </div>

            <div className="card-body p-4">
              {error && (
                <div className="alert alert-danger rounded-pill px-3 py-2 text-center small" role="alert">
                  <i className="fa-solid fa-triangle-exclamation mr-2"></i> {error}
                </div>
              )}

              {success && (
                <div className="alert alert-success rounded-pill px-3 py-2 text-center small" role="alert">
                  <i className="fa-solid fa-circle-check mr-2"></i> {success}
                </div>
              )}

              <form onSubmit={handleRegister}>
                <div className="row">
                  <div className="col-12 col-md-6 mb-3">
                    <label className="small font-weight-bold text-secondary">Họ và tên *</label>
                    <input
                      type="text"
                      className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                      placeholder="Nguyễn Văn A..."
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      required
                    />
                  </div>

                  <div className="col-12 col-md-6 mb-3">
                    <label className="small font-weight-bold text-secondary">Số điện thoại liên hệ</label>
                    <input
                      type="tel"
                      className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                      placeholder="0912345678..."
                      value={phone}
                      onChange={(e) => setPhone(e.target.value)}
                    />
                  </div>
                </div>

                <div className="mb-3">
                  <label className="small font-weight-bold text-secondary">Địa chỉ Email *</label>
                  <input
                    type="email"
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                    placeholder="nguyenvanan@gmail.com..."
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="small font-weight-bold text-secondary">Mật khẩu bảo mật *</label>
                  <input
                    type="password"
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                    placeholder="Tối thiểu 6 ký tự..."
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                  />
                </div>

                <div className="mb-4">
                  <label className="small font-weight-bold text-secondary">Địa chỉ giao hàng mặc định</label>
                  <input
                    type="text"
                    className="form-control rounded-pill px-3 shadow-none border-secondary-50"
                    placeholder="Số nhà, tên đường, phường/xã, quận/huyện..."
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3"
                  style={{ fontSize: '0.85rem' }}
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>
                      Đang xử lý tạo tài khoản...
                    </>
                  ) : (
                    <>
                      Đăng Ký Tài Khoản <i className="fa-solid fa-user-check ml-2"></i>
                    </>
                  )}
                </button>
              </form>

              <div className="text-center mt-4 pt-3 border-top">
                <span className="text-secondary small">Bạn đã có tài khoản thành viên? </span>
                <Link to="/login" className="font-weight-bold text-danger text-decoration-none small hover-underline">
                  Đăng nhập tại đây!
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
