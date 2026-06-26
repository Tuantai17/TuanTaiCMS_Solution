import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Header from './components/Header';
import Footer from './components/Footer';
import ScrollToTop from './components/ScrollToTop';

// Import các trang nghiệp vụ từ thư mục pages/ theo đúng cấu trúc template chuẩn
import Home from './pages/Home';
import Shop from './pages/Shop';
import ProductDetail from './pages/ProductDetail';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import PostList from './pages/PostList';
import PostDetail from './pages/PostDetail';
import Login from './pages/Login';
import Register from './pages/Register';
import OrderHistory from './pages/OrderHistory';
import OrderDetailPage from './pages/OrderDetailPage';
import ForgotPassword from './pages/ForgotPassword';
import Profile from './pages/Profile';
import AddressesPage from './pages/AddressesPage';
import ChangePassword from './pages/ChangePassword';
import Notifications from './pages/Notifications';
import OriginalToysPage from './pages/OriginalToysPage';
import ReturnPolicyPage from './pages/ReturnPolicyPage';
import SupportPage from './pages/SupportPage';
import AboutPage from './pages/AboutPage';
import FavoriteProductsPage from './pages/FavoriteProductsPage';

// Import CSS từ assets/css/ theo đúng sơ đồ module hóa
import './assets/css/App.css';

function App() {
  return (
    <Router>
      <ScrollToTop />
      <div className="App d-flex flex-column min-vh-100">
        {/* Header chung trên cùng */}
        <Header />
        
        {/* Các trang hiển thị thay đổi linh hoạt theo Route */}
        <main className="flex-grow-1" style={{ backgroundColor: '#ffffff' }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/products" element={<Shop />} />
            <Route path="/products/:id" element={<ProductDetail />} />
            <Route path="/cart" element={<Cart />} />
            <Route path="/checkout" element={<Checkout />} />
            <Route path="/blog" element={<PostList />} />
            <Route path="/blog/category/:categoryId" element={<PostList />} />
            <Route path="/blog/:id" element={<PostDetail />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/order-history" element={<OrderHistory />} />
            <Route path="/my-orders" element={<OrderHistory />} />
            <Route path="/account/orders" element={<OrderHistory />} />
            <Route path="/account/orders/:id" element={<OrderDetailPage />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/profile/change-password" element={<ChangePassword />} />
            <Route path="/profile/favorites" element={<FavoriteProductsPage />} />
            <Route path="/account/addresses" element={<AddressesPage />} />
            <Route path="/notifications" element={<Notifications />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/do-choi-chinh-hang" element={<OriginalToysPage />} />
            <Route path="/doi-tra-trong-7-ngay" element={<ReturnPolicyPage />} />
            <Route path="/ho-tro-khach-hang" element={<SupportPage />} />
            <Route path="/gioi-thieu" element={<AboutPage />} />
          </Routes>
        </main>
        
        {/* Footer chung dưới cùng */}
        <Footer />
      </div>
    </Router>
  );
}

export default App;
