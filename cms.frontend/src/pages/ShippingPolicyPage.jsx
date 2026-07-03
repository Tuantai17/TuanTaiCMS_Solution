import React from 'react';

const ShippingPolicyPage = () => {
  return (
    <div className="container py-5 my-5" style={{ minHeight: '50vh' }}>
      <h2 className="mb-4 font-weight-bold text-danger">Chính sách vận chuyển</h2>
      <p>MyKingdom cung cấp dịch vụ giao hàng tận nơi trên toàn quốc, phối hợp với các đối tác vận chuyển uy tín nhằm mang đồ chơi đến tận tay bé yêu một cách nhanh chóng và an toàn nhất.</p>
      <ul>
        <li><strong className="text-dark">Miễn phí giao hàng:</strong> Áp dụng cho các đơn hàng có giá trị từ 500,000 VNĐ trở lên trên toàn quốc.</li>
        <li><strong className="text-dark">Thời gian giao hàng tiêu chuẩn:</strong> Từ 1-3 ngày làm việc đối với khu vực trung tâm thành phố. Từ 3-5 ngày đối với khu vực ngoại thành và các tỉnh thành khác.</li>
        <li><strong className="text-dark">Giao hàng hỏa tốc:</strong> Hỗ trợ giao nhanh trong vòng 2 giờ đối với các đơn hàng phát sinh tại khu vực nội thành TP.HCM và Hà Nội.</li>
      </ul>
      <p className="mt-4">Trong trường hợp có phát sinh chậm trễ, đội ngũ chăm sóc khách hàng của chúng tôi sẽ chủ động liên hệ để thông báo đến quý khách.</p>
    </div>
  );
};

export default ShippingPolicyPage;
