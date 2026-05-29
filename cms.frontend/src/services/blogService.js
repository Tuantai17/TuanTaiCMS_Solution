import axiosClient from '../api/axiosClient';

const blogService = {
  // Lấy danh mục bài viết (Chuyên mục tin tức nếu có API riêng)
  getBlogCategories: () => {
    const url = '/Categories';
    return axiosClient.get(url);
  },

  // Lấy toàn bộ các bài viết tin tức và blog từ CSDL
  getAllPosts: () => {
    const url = '/Posts';
    return axiosClient.get(url);
  },

  // Lọc bài viết theo danh mục bài viết
  getPostsByCategory: (categoryId) => {
    const url = `/Posts/category/${categoryId}`;
    return axiosClient.get(url);
  },

  // Lấy chi tiết thông tin đầy đủ của một bài viết theo ID
  getPostDetail: (id) => {
    const url = `/Posts/${id}`;
    return axiosClient.get(url);
  }
};

export default blogService;
