import axiosClient from '../api/axiosClient';

const favoriteService = {
  getFavorites: async (page = 1, pageSize = 12, keyword = '') => {
    const url = `/favorites?page=${page}&pageSize=${pageSize}&keyword=${encodeURIComponent(keyword)}`;
    return axiosClient.get(url);
  },

  addFavorite: async (productId) => {
    const url = `/favorites/${productId}`;
    return axiosClient.post(url);
  },

  removeFavorite: async (productId) => {
    const url = `/favorites/${productId}`;
    return axiosClient.delete(url);
  },

  checkStatus: async (productId) => {
    const url = `/favorites/${productId}/status`;
    return axiosClient.get(url);
  },

  getCount: async () => {
    const url = `/favorites/count`;
    return axiosClient.get(url);
  }
};

export default favoriteService;
