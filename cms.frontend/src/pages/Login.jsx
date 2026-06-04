import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.customerLogin({ email, password });
      // Đăng nhập thành công, lưu thông tin customer vào localStorage
      localStorage.setItem('customer', JSON.stringify(response));
      
      // Kích hoạt sự kiện tùy biến để báo cho Header cập nhật trạng thái
      window.dispatchEvent(new Event('customerLoginStateChange'));
      
      // Điều hướng về trang chủ
      navigate('/');
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản hoặc kết nối mạng.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container my-5 animate--fade-in">
      <div className="row justify-content-center">
        <div className="col-12 col-md-6 col-lg-5">
          <div className="card shadow-lg border-0 rounded-lg overflow-hidden">
            {/* Header Form mang bản sắc MyKingdom */}
            <div className="bg-danger text-white text-center py-4 px-3" style={{ background: 'linear-gradient(135deg, #CF102D, #ff3d57)' }}>
              <h4 className="font-weight-bold text-uppercase mb-1">
                <i className="fa-solid fa-user-lock mr-2"></i> Khách Hàng Đăng Nhập
              </h4>
              <p className="small mb-0 opacity-75">Chào mừng bạn trở lại với vương quốc đồ chơi MyKingdom</p>
            </div>

            <div className="card-body p-4">
              {error && (
                <div className="alert alert-danger rounded-pill px-3 py-2 text-center small" role="alert">
                  <i className="fa-solid fa-triangle-exclamation mr-2"></i> {error}
                </div>
              )}

              <form onSubmit={handleLogin}>
                <div className="mb-3">
                  <label className="small font-weight-bold text-secondary">Địa chỉ Email *</label>
                  <div className="input-group">
                    <div className="input-group-prepend">
                      <span className="input-group-text bg-light border-right-0 rounded-left-pill px-3">
                        <i className="fa-regular fa-envelope text-muted"></i>
                      </span>
                    </div>
                    <input
                      type="email"
                      className="form-control border-left-0 rounded-right-pill px-3 shadow-none"
                      placeholder="username@gmail.com..."
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      required
                    />
                  </div>
                </div>

                <div className="mb-4">
                  <label className="small font-weight-bold text-secondary">Mật khẩu bảo mật *</label>
                  <div className="input-group">
                    <div className="input-group-prepend">
                      <span className="input-group-text bg-light border-right-0 rounded-left-pill px-3">
                        <i className="fa-solid fa-key text-muted"></i>
                      </span>
                    </div>
                    <input
                      type="password"
                      className="form-control border-left-0 rounded-right-pill px-3 shadow-none"
                      placeholder="Nhập mật khẩu..."
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      required
                    />
                  </div>
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
                      Đang xác thực...
                    </>
                  ) : (
                    <>
                      Đăng Nhập Ngay <i className="fa-solid fa-arrow-right-to-bracket ml-2"></i>
                    </>
                  )}
                </button>
              </form>

              <div className="text-center mt-4 pt-3 border-top">
                <span className="text-secondary small">Bạn chưa có tài khoản? </span>
                <Link to="/register" className="font-weight-bold text-danger text-decoration-none small hover-underline">
                  Đăng ký ngay tại đây!
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
