import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Header from './components/Header';
import Footer from './components/Footer';

// Import các trang nghiệp vụ từ thư mục pages/ theo đúng cấu trúc template chuẩn
import Home from './pages/Home';
import Shop from './pages/Shop';
import ProductDetail from './pages/ProductDetail';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import PostList from './pages/PostList';
import PostDetail from './pages/PostDetail';

// Import CSS từ assets/css/ theo đúng sơ đồ module hóa
import './assets/css/App.css';

function App() {
  return (
    <Router>
      <div className="App d-flex flex-column min-vh-100" style={{ fontFamily: "'Outfit', sans-serif" }}>
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
            <Route path="/blog/:id" element={<PostDetail />} />
          </Routes>
        </main>
        
        {/* Footer chung dưới cùng */}
        <Footer />
      </div>
    </Router>
  );
}

export default App;
