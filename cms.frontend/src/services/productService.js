import axiosClient from '../api/axiosClient';

const productService = {
  // Lấy toàn bộ danh sách sản phẩm thời trang & đồ chơi mẫu
  getAllProducts: (params) => {
    const url = '/Products';
    return axiosClient.get(url, { params: { ...params, _t: Date.now() } });
  },

  getNewestProducts: (limit) => {
    const url = '/Products';
    const params = { sortBy: 'newest', _t: Date.now() };
    if (limit) params.take = limit;
    return axiosClient.get(url, { params });
  },

  getBestSellingProducts: (limit = 8) => {
    const url = '/Products';
    return axiosClient.get(url, { params: { sortBy: 'best-selling', take: limit, _t: Date.now() } });
  },

  // Lọc danh sách sản phẩm theo mã ID danh mục
  getProductsByCategory: (categoryId) => {
    const url = `/Products/categoryproduct/${categoryId}`;
    return axiosClient.get(url, { params: { _t: Date.now() } });
  },

  // Lấy chi tiết thông tin của duy nhất một sản phẩm theo ID
  getProductDetail: (id) => {
    const url = `/Products/${id}`;
    return axiosClient.get(url, { params: { _t: Date.now() } });
  },

  // Lấy danh sách sản phẩm có trạng thái New (IsNew = true)
  getNewProducts: (limit) => {
    const url = '/Products';
    return axiosClient.get(url, { params: { filter: 'new', take: limit, _t: Date.now() } });
  },

  // Lấy danh sách sản phẩm đang Sale (IsSale = true)
  getSaleProducts: (limit) => {
    const url = '/Products';
    return axiosClient.get(url, { params: { filter: 'sale', take: limit, _t: Date.now() } });
  }
};

export default productService;
