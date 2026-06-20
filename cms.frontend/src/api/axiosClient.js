import axios from 'axios';
import { clearStoredCustomer, getCustomerAccessToken } from '../utils/customerSession';

// Khởi tạo thực thể axios với cấu hình kết nối API của Backend
const axiosClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'https://localhost:7238/api', // Lấy từ biến môi trường hoặc dự phòng mặc định
  headers: {
    'Content-Type': 'application/json',
    'Cache-Control': 'no-cache',
    Pragma: 'no-cache',
  },
  timeout: 10000, // Thời gian chờ tối đa 10 giây
});

axiosClient.interceptors.request.use((config) => {
  const accessToken = getCustomerAccessToken();
  if (accessToken) {
    config.headers = config.headers || {};
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

// Interceptor xử lý dữ liệu trả về tập trung để gọt bỏ cấu trúc thừa của axios response
axiosClient.interceptors.response.use(
  (response) => {
    // Trả về trực tiếp phần dữ liệu chính JSON từ API
    return response.data;
  },
  (error) => {
    if (error?.response?.status === 401) {
      clearStoredCustomer();
      window.dispatchEvent(new Event('customerLoginStateChange'));
    }

    // Xử lý lỗi kết nối API tập trung (chỉ hiển thị console.error cho lỗi hệ thống/mạng thực sự, không log lỗi xác thực/validation 4xx)
    if (!error.response || error.response.status >= 500) {
      console.error('Lỗi kết nối API Hệ thống:', error.message);
    }
    return Promise.reject(error);
  }
);

export default axiosClient;
