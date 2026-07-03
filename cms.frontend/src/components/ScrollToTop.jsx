import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

/**
 * Component tiện ích (Utility Component) ScrollToTop
 * Chức năng: Tự động cuộn trang lên trên cùng (Top) mỗi khi người dùng chuyển trang (chuyển URL)
 * 
 * Thường được đặt ở file App.jsx cao nhất để bắt mọi sự kiện thay đổi Route.
 */
const ScrollToTop = () => {
  // Lấy ra pathname (đường dẫn) và search (query string) hiện tại từ React Router
  const { pathname, search } = useLocation();

  // Effect chạy mỗi khi pathname hoặc search thay đổi
  useEffect(() => {
    // Kích hoạt hàm cuộn trang của trình duyệt (x=0, y=0)
    window.scrollTo(0, 0);
  }, [pathname, search]);

  // Component này không hiển thị bất kỳ giao diện nào
  return null;
};

export default ScrollToTop;
