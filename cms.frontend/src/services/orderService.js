import axiosClient from '../api/axiosClient';

const orderService = {
  // Gửi thông tin giỏ hàng và thông tin đặt hàng lên Server
  createOrder: (data) => {
    const url = '/Orders';
    return axiosClient.post(url, data);
  },

  // Tải danh sách đơn hàng đã mua của khách hàng theo CustomerId
  getCustomerOrders: (customerId) => {
    const url = `/Orders/customer/${customerId}`;
    return axiosClient.get(url);
  }
};

export default orderService;
