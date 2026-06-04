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
  }
};

export default authService;
