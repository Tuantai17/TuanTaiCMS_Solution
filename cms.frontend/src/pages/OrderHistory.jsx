import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import orderService from '../services/orderService';

const OrderHistory = () => {
  const [customer, setCustomer] = useState(null);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const storedCustomer = localStorage.getItem('customer');
    if (!storedCustomer) {
      alert("Vui lòng đăng nhập để xem lịch sử mua hàng!");
      navigate('/login');
      return;
    }

    try {
      const parsedCustomer = JSON.parse(storedCustomer);
      setCustomer(parsedCustomer);
      
      // Tải danh sách đơn hàng
      const fetchOrders = async () => {
        try {
          setLoading(true);
          const data = await orderService.getCustomerOrders(parsedCustomer.customerId);
          setOrders(data);
        } catch (err) {
          console.error("Lỗi tải lịch sử đơn hàng:", err);
          setError("Không thể tải lịch sử đơn hàng. Vui lòng kiểm tra lại kết nối mạng.");
        } finally {
          setLoading(false);
        }
      };

      fetchOrders();
    } catch (e) {
      localStorage.removeItem('customer');
      navigate('/login');
    }
  }, [navigate]);

  if (loading) {
    return (
      <div className="container text-center my-5 py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
        <p className="mt-3 text-secondary">Đang tải lịch sử mua hàng của bạn...</p>
      </div>
    );
  }

  // Định nghĩa màu sắc và nhãn cho các trạng thái đơn hàng
  const getStatusBadge = (status) => {
    switch (status) {
      case 0:
        return <span className="badge badge-warning px-3 py-2 rounded-pill font-weight-bold text-dark"><i className="fa-solid fa-clock mr-1"></i> Chờ duyệt</span>;
      case 1:
        return <span className="badge badge-info px-3 py-2 rounded-pill font-weight-bold text-white"><i className="fa-solid fa-truck mr-1"></i> Đang giao</span>;
      case 2:
        return <span className="badge badge-success px-3 py-2 rounded-pill font-weight-bold text-white"><i className="fa-solid fa-circle-check mr-1"></i> Đã xong</span>;
      default:
        return <span className="badge badge-secondary px-3 py-2 rounded-pill font-weight-bold text-white">Không xác định</span>;
    }
  };

  return (
    <div className="container my-4 animate--fade-in">
      {/* breadcrumb */}
      <nav aria-label="breadcrumb">
        <ol className="breadcrumb bg-transparent p-0 mb-4" style={{ fontSize: '0.85rem' }}>
          <li className="breadcrumb-item"><a href="/" className="text-secondary text-decoration-none">Trang chủ</a></li>
          <li className="breadcrumb-item active text-danger font-weight-bold" aria-current="page">Lịch sử mua hàng</li>
        </ol>
      </nav>

      <div className="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom flex-wrap gap-2">
        <h2 className="font-weight-bold text-dark text-uppercase mb-0">
          <i className="fa-solid fa-clock-rotate-left text-danger mr-2"></i> Lịch Sử Mua Hàng
        </h2>
        {customer && (
          <span className="text-secondary small font-weight-bold">
            Tài khoản: <span className="text-danger">{customer.fullName}</span> ({customer.email})
          </span>
        )}
      </div>

      {error && (
        <div className="alert alert-danger px-4 py-3 rounded-lg mb-4 text-center" role="alert">
          <i className="fa-solid fa-circle-exclamation mr-2"></i> {error}
        </div>
      )}

      {orders.length === 0 ? (
        <div className="text-center py-5 border rounded-lg bg-light my-4">
          <i className="fa-solid fa-folder-open text-muted mb-3" style={{ fontSize: '4rem', opacity: 0.4 }}></i>
          <h5 className="text-secondary font-weight-bold">Bạn chưa có đơn đặt hàng nào!</h5>
          <p className="small text-muted mb-4">Các đơn hàng bạn đã mua sẽ xuất hiện tại đây để theo dõi tiến trình giao hàng.</p>
          <Link to="/products" className="btn btn-danger rounded-pill font-weight-bold text-uppercase px-4 py-2" style={{ fontSize: '0.85rem' }}>
            Mua sắm ngay <i className="fa-solid fa-cart-shopping ml-1"></i>
          </Link>
        </div>
      ) : (
        <div className="d-flex flex-column gap-4">
          {orders.map((order) => {
            // Tính tổng tiền cho từng đơn hàng
            const orderTotal = order.orderDetails.reduce((acc, details) => acc + (details.price || details.unitPrice) * details.quantity, 0) + 35000;

            return (
              <div className="card shadow-sm border border-light rounded-lg overflow-hidden mb-4" key={order.id} style={{ borderRadius: '16px' }}>
                {/* Header đơn hàng: Mã đơn, ngày đặt, trạng thái */}
                <div className="card-header bg-light py-3 px-4 d-flex justify-content-between align-items-center flex-wrap gap-2 border-bottom-0">
                  <div className="d-flex align-items-center gap-3 flex-wrap">
                    <span className="font-weight-extrabold text-dark mr-3" style={{ fontSize: '1rem' }}>
                      Đơn hàng <strong className="text-danger">#{order.id}</strong>
                    </span>
                    <span className="text-secondary small">
                      <i className="fa-regular fa-calendar-days mr-1"></i> Ngày đặt: {new Date(order.orderDate).toLocaleString('vi-VN')}
                    </span>
                  </div>
                  <div>
                    {getStatusBadge(order.status)}
                  </div>
                </div>

                {/* Body đơn hàng: Danh sách các sản phẩm trong đơn */}
                <div className="card-body p-0">
                  {order.orderDetails.map((detail, idx) => (
                    <div className="d-flex align-items-center p-3 px-4 border-bottom" key={detail.id || idx}>
                      <img 
                        src={detail.productImageUrl || "https://placehold.co/100x100/e9ecef/6c757d?text=No+Image"} 
                        alt={detail.productName} 
                        className="rounded border mr-3" 
                        style={{ width: '50px', height: '50px', objectFit: 'cover' }} 
                      />
                      <div className="flex-grow-1">
                        <h6 className="font-weight-bold small text-dark mb-1 text-truncate-2" style={{ maxWidth: '400px' }}>{detail.productName}</h6>
                        <span className="text-secondary small">Số lượng: {detail.quantity} x {new Intl.NumberFormat('vi-VN').format(detail.price || detail.unitPrice)} ₫</span>
                      </div>
                      <span className="font-weight-bold text-secondary small pl-2" style={{ whiteSpace: 'nowrap' }}>
                        {new Intl.NumberFormat('vi-VN').format((detail.price || detail.unitPrice) * detail.quantity)} ₫
                      </span>
                    </div>
                  ))}
                </div>

                {/* Footer đơn hàng: Ghi chú, tổng tiền thanh toán */}
                <div className="card-footer bg-white border-top-0 p-3 px-4 d-flex justify-content-between align-items-center flex-wrap gap-3">
                  <div className="text-muted small flex-grow-1" style={{ maxWidth: '60%', fontSize: '0.8rem' }}>
                    {order.notes && (
                      <>
                        <i className="fa-solid fa-info-circle mr-1"></i> <strong>Ghi chú:</strong> {order.notes}
                      </>
                    )}
                  </div>
                  <div className="text-right">
                    <span className="text-secondary small mr-3">Tổng cộng thanh toán (Gồm ship 35k):</span>
                    <span className="h5 font-weight-extrabold text-danger mb-0">
                      {new Intl.NumberFormat('vi-VN').format(orderTotal)} ₫
                    </span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default OrderHistory;
