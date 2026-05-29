import axiosClient from '../api/axiosClient';

const categoryProductService = {
  // Lấy toàn bộ danh mục sản phẩm từ bảng CategoriesProducts ở Backend SQL Server
  getAllCategoryProducts: () => {
    const url = '/CategoriesProducts';
    return axiosClient.get(url);
  }
};

export default categoryProductService;
