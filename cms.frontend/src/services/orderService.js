import axiosClient from '../api/axiosClient';

const orderService = {
  // Gửi thông tin giỏ hàng và thông tin đặt hàng lên Server
  createOrder: (data) => {
    const url = '/Orders';
    return axiosClient.post(url, data);
  },

  // API lịch sử mua hàng của khách đang đăng nhập
  getMyOrders: (params) => {
    const url = '/Orders/my';
    return axiosClient.get(url, { params });
  },

  // API chi tiết đơn hàng của khách đang đăng nhập
  getMyOrderDetail: (id) => {
    const url = `/Orders/my/${id}`;
    return axiosClient.get(url);
  },

  // API hủy đơn hàng của khách đang đăng nhập
  cancelMyOrder: (id, reason) => {
    const url = `/Orders/my/${id}/cancel`;
    return axiosClient.put(url, { reason });
  },

  // Giữ lại API cũ để tránh ảnh hưởng các phần khác nếu đang dùng
  getCustomerOrders: (customerId) => {
    const url = `/Orders/customer/${customerId}`;
    return axiosClient.get(url);
  }
};

export default orderService;
