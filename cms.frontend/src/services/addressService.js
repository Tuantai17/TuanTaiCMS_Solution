import axiosClient from '../api/axiosClient';

const addressService = {
  // Lấy danh sách địa chỉ của khách hàng
  getAddresses: (customerId) => {
    const url = `/Addresses/customer/${customerId}`;
    return axiosClient.get(url);
  },

  // Thêm mới địa chỉ nhận hàng
  createAddress: (data) => {
    const url = '/Addresses';
    return axiosClient.post(url, data);
  },

  // Cập nhật thông tin địa chỉ nhận hàng
  updateAddress: (id, data) => {
    const url = `/Addresses/${id}`;
    return axiosClient.put(url, data);
  },

  // Xóa địa chỉ nhận hàng
  deleteAddress: (id, customerId) => {
    const url = `/Addresses/${id}?customerId=${customerId}`;
    return axiosClient.delete(url);
  },

  // Thiết lập địa chỉ mặc định
  setDefaultAddress: (id, customerId) => {
    const url = `/Addresses/${id}/default?customerId=${customerId}`;
    return axiosClient.put(url);
  }
};

export default addressService;
