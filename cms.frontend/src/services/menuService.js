import axiosClient from '../api/axiosClient';

const menuService = {
  getMenus: () => {
    const url = '/Menus';
    return axiosClient.get(url);
  },

  getMenuHierarchy: () => {
    const url = '/Menus/hierarchy';
    return axiosClient.get(url);
  }
};

export default menuService;
