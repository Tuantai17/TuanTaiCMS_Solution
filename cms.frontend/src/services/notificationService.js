import axiosClient from '../api/axiosClient';

const notificationService = {
  getNotifications: (page = 1, pageSize = 10) => {
    return axiosClient.get(`/customer-notifications?page=${page}&pageSize=${pageSize}`);
  },

  getUnreadCount: () => {
    return axiosClient.get('/customer-notifications/unread-count');
  },

  markAsRead: (id) => {
    return axiosClient.put(`/customer-notifications/${id}/read`);
  },

  markAllAsRead: () => {
    return axiosClient.put('/customer-notifications/read-all');
  }
};

export default notificationService;
