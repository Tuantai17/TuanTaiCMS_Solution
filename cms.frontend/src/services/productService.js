import axiosClient from '../api/axiosClient';

const productService = {
  // Lấy toàn bộ danh sách sản phẩm thời trang & đồ chơi mẫu
  getAllProducts: () => {
    const url = '/Products';
    return axiosClient.get(url);
  },

  // Lọc danh sách sản phẩm theo mã ID danh mục
  getProductsByCategory: (categoryId) => {
    const url = `/Products/categoryproduct/${categoryId}`;
    return axiosClient.get(url);
  },

  // Lấy chi tiết thông tin của duy nhất một sản phẩm theo ID
  getProductDetail: (id) => {
    const url = `/Products/${id}`;
    return axiosClient.get(url);
  }
};

export default productService;
