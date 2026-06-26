import React, { createContext, useState, useEffect, useContext } from 'react';
import favoriteService from '../services/favoriteService';
// Assume there's an AuthContext or way to check if user is logged in
// If not, we will rely on API calls returning 401 Unauthorized to trigger login toasts.

export const FavoriteContext = createContext();

export const useFavorite = () => useContext(FavoriteContext);

export const FavoriteProvider = ({ children }) => {
  const [favoriteCount, setFavoriteCount] = useState(0);

  const fetchFavoriteCount = async () => {
    try {
      const data = await favoriteService.getCount();
      if (data && data.count !== undefined) {
        setFavoriteCount(data.count);
      }
    } catch (error) {
      console.error('Lỗi khi lấy số lượng yêu thích:', error);
    }
  };

  useEffect(() => {
    // Initial fetch of favorite count when app loads
    fetchFavoriteCount();
  }, []);

  const toggleFavorite = async (productId, currentStatus) => {
    try {
      if (currentStatus) {
        // Remove from favorite
        const result = await favoriteService.removeFavorite(productId);
        if (result.success) {
          alert(result.message);
          setFavoriteCount(prev => Math.max(0, prev - 1));
          return false;
        }
      } else {
        // Add to favorite
        const result = await favoriteService.addFavorite(productId);
        if (result.success) {
          alert(result.message);
          setFavoriteCount(prev => prev + 1);
          return true;
        }
      }
    } catch (error) {
      if (error.response && error.response.status === 401) {
        alert('Vui lòng đăng nhập để sử dụng chức năng yêu thích.');
      } else {
        alert('Đã xảy ra lỗi, vui lòng thử lại.');
      }
    }
    return currentStatus; // return unchanged if error
  };

  return (
    <FavoriteContext.Provider value={{ favoriteCount, fetchFavoriteCount, toggleFavorite }}>
      {children}
    </FavoriteContext.Provider>
  );
};
