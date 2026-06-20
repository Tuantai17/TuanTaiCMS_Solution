import React, { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import { saveStoredCustomer } from '../utils/customerSession';

function Login() {
  const navigate = useNavigate();
  const location = useLocation();
  const [loading, setLoading] = useState(false);
  const [credentials, setCredentials] = useState({
    email: '',
    password: '',
  });

  useEffect(() => {
    if (location.state?.email) {
      setCredentials((prev) => ({ ...prev, email: location.state.email }));
    }

    if (location.state?.message) {
      window.alert(location.state.message);
    }
  }, [location.state]);

  const handleChange = (event) => {
    setCredentials((prev) => ({
      ...prev,
      [event.target.name]: event.target.value,
    }));
  };

  const handleLoginSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);

    try {
      const response = await authService.login(credentials);
      const data = response?.data || response;
      saveStoredCustomer(data);
      window.dispatchEvent(new Event('customerLoginStateChange'));
      window.alert(`🎉 XÁC THỰC THÀNH CÔNG: Chào mừng ${data.fullName} đã đăng nhập hệ thống!`);
      navigate(location.state?.from?.pathname || '/');
    } catch (error) {
      window.alert('⛔ ĐĂNG NHẬP THẤT BẠI: Sai tài khoản Email hoặc Mật khẩu không chính xác!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-5">
      <div className="row justify-content-center py-5">
        <div className="col-md-4 col-sm-8">
          <div className="card auth-card">
            {/* Header Form MyKingdom */}
            <div className="text-white text-center py-4 px-3" style={{ background: 'linear-gradient(135deg, #CF102D, #ff3d57)' }}>
              <h4 className="font-weight-bold text-uppercase mb-1">
                <i className="fa-solid fa-user-shield mr-2"></i> ĐĂNG NHẬP HỆ THỐNG
              </h4>
              <p className="small mb-0 opacity-75">Chào mừng bạn trở lại với MyKingdom</p>
            </div>

            <div className="card-body p-4">
              <form onSubmit={handleLoginSubmit}>
                <div className="form-group mb-3">
                  <label className="small font-weight-bold text-secondary">TÀI KHOẢN EMAIL</label>
                  <input
                    type="email"
                    name="email"
                    className="form-control"
                    placeholder="example@gmail.com"
                    value={credentials.email}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="form-group mb-4">
                  <label className="small font-weight-bold text-secondary">MẬT KHẨU</label>
                  <input
                    type="password"
                    name="password"
                    className="form-control"
                    placeholder="******"
                    value={credentials.password}
                    onChange={handleChange}
                    required
                  />
                  <div className="text-right mt-2">
                    <Link to="/forgot-password" className="small font-weight-bold text-danger text-decoration-none">
                      Quên mật khẩu?
                    </Link>
                  </div>
                </div>

                <button
                  type="submit"
                  className="btn btn-danger btn-block w-100 py-2 font-weight-bold shadow-sm"
                  disabled={loading}
                >
                  {loading ? 'ĐANG XÁC THỰC...' : 'ĐĂNG NHẬP'}
                </button>
              </form>

              <p className="text-center mt-3 small m-0 text-muted">
                Chưa có tài khoản?{' '}
                <Link to="/register" className="font-weight-bold text-danger">
                  Đăng ký tại đây
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Login;
