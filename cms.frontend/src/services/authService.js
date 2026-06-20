import axios from 'axios';
import axiosClient from '../api/axiosClient';

const authService = {
  customerRegister: (data) => {
    const url = '/Customers/register';
    return axiosClient.post(url, data);
  },

  register: async (customerData) => {
    return authService.customerRegister(customerData);
  },

  customerLogin: (data) => {
    const url = '/Customers/login';
    return axiosClient.post(url, data);
  },

  login: async (credentials) => {
    return authService.customerLogin(credentials);
  },

  sendResetCode: (email) => {
    const url = '/Auth/SendResetCode';
    return axiosClient.post(url, { email });
  },

  verifyResetCode: (email, code) => {
    const url = '/Auth/VerifyResetCode';
    return axiosClient.post(url, { email, code });
  },

  resetPassword: (email, code, newPassword) => {
    const url = '/Auth/ResetPassword';
    return axiosClient.post(url, { email, code, newPassword });
  },

  getProfile: (customerId) => {
    const url = `/Auth/GetProfile/${customerId}`;
    return axiosClient.get(url);
  },

  updateProfile: (data) => {
    const url = '/Auth/UpdateProfile';
    return axiosClient.post(url, data);
  },

  changePassword: (data) => {
    const url = '/Auth/ChangePassword';
    return axiosClient.post(url, data);
  },

  uploadAvatar: (customerId, file) => {
    const baseURL = process.env.REACT_APP_API_URL || 'https://localhost:7238/api';
    const formData = new FormData();
    formData.append('customerId', customerId);
    formData.append('avatar', file);

    return axios
      .post(`${baseURL}/Auth/UploadAvatar`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((res) => res.data);
  },
};

export default authService;
