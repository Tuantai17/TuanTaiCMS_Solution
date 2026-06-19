import axiosClient from '../api/axiosClient';

const authService = {
  // Gửi thông tin đăng ký khách hàng mới
  customerRegister: (data) => {
    const url = '/Auth/CustomerRegister';
    return axiosClient.post(url, data);
  },
  
  // Gửi yêu cầu đăng nhập khách hàng
  customerLogin: (data) => {
    const url = '/Auth/CustomerLogin';
    return axiosClient.post(url, data);
  },

  // Gửi mã xác minh OTP qua Email (Bước 1)
  sendResetCode: (email) => {
    const url = '/Auth/SendResetCode';
    return axiosClient.post(url, { email });
  },

  // Xác thực mã OTP nhập từ giao diện (Bước 2)
  verifyResetCode: (email, code) => {
    const url = '/Auth/VerifyResetCode';
    return axiosClient.post(url, { email, code });
  },

  // Đặt lại mật khẩu mới (Bước 3)
  resetPassword: (email, code, newPassword) => {
    const url = '/Auth/ResetPassword';
    return axiosClient.post(url, { email, code, newPassword });
  }
};

export default authService;
