import axiosClient from '../api/axiosClient';

const bannerService = {
  getBanners: () => {
    const url = '/Banners';
    return axiosClient.get(url);
  }
};

export default bannerService;
