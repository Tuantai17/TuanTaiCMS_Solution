import React from 'react';
import { Link } from 'react-router-dom';

const OrderSuccess = () => {
  return (
    <div className="container text-center my-5 py-5 animate--fade-in">
      <div className="mb-4">
        <div 
          className="d-inline-flex align-items-center justify-content-center bg-success text-white rounded-circle shadow-sm"
          style={{ width: '80px', height: '80px' }}
        >
          <i className="fa-solid fa-check fa-3x"></i>
        </div>
      </div>
      <h2 className="text-success font-weight-bold mb-3" style={{ fontSize: '2rem' }}>Đặt hàng thành công!</h2>
      <p className="text-secondary mb-5" style={{ maxWidth: '500px', margin: '0 auto', fontSize: '1.1rem', lineHeight: '1.6' }}>
        Cảm ơn bạn đã đặt hàng tại MyKingdom! Đơn hàng của bạn đang được xử lý và sẽ sớm được giao đến.
      </p>
      
      <div className="d-flex justify-content-center flex-wrap" style={{ gap: '15px' }}>
        <Link 
          to="/account/orders" 
          className="btn text-white px-4 py-2 font-weight-bold rounded shadow-sm" 
          style={{ backgroundColor: '#c92127', borderColor: '#c92127' }}
        >
          <i className="fa-solid fa-box-open mr-2"></i> Xem đơn hàng của tôi
        </Link>
        <Link 
          to="/products" 
          className="btn bg-white px-4 py-2 font-weight-bold rounded shadow-sm" 
          style={{ color: '#c92127', borderColor: '#c92127', borderWidth: '1px', borderStyle: 'solid' }}
        >
          <i className="fa-solid fa-cart-shopping mr-2"></i> Tiếp tục mua sắm
        </Link>
      </div>
    </div>
  );
};

export default OrderSuccess;
