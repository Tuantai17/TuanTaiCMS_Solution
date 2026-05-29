import axios from 'axios';

// Khởi tạo thực thể axios với cấu hình kết nối API của Backend
const axiosClient = axios.create({
  baseURL: 'https://localhost:7238/api', // Khớp chính xác cổng Port của ASP.NET Core Backend
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // Thời gian chờ tối đa 10 giây
});

// Interceptor xử lý dữ liệu trả về tập trung để gọt bỏ cấu trúc thừa của axios response
axiosClient.interceptors.response.use(
  (response) => {
    // Trả về trực tiếp phần dữ liệu chính JSON từ API
    return response.data;
  },
  (error) => {
    // Xử lý lỗi kết nối API tập trung (server sập, lỗi mạng, lỗi CORS)
    console.error('Lỗi kết nối API Hệ thống:', error.message);
    return Promise.reject(error);
  }
);

export default axiosClient;
