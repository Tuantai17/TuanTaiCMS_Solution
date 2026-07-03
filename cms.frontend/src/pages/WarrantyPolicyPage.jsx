import React from 'react';

const WarrantyPolicyPage = () => {
  return (
    <div className="container py-5 my-5" style={{ minHeight: '50vh' }}>
      <h2 className="mb-4 font-weight-bold text-danger">Chính sách bảo hành</h2>
      <p>MyKingdom cam kết bảo hành các sản phẩm đồ chơi chính hãng theo đúng quy định của nhà sản xuất.</p>
      <ul>
        <li><strong className="text-dark">Thời gian bảo hành:</strong> Tùy thuộc vào từng loại sản phẩm và thương hiệu.</li>
        <li><strong className="text-dark">Điều kiện bảo hành:</strong> Sản phẩm bị lỗi kỹ thuật phát sinh từ phía nhà sản xuất. Sản phẩm phải còn nguyên tem mác và hóa đơn mua hàng.</li>
        <li><strong className="text-dark">Không bảo hành:</strong> Các trường hợp rơi vỡ, vào nước, cháy nổ, hao mòn tự nhiên hoặc sử dụng sai hướng dẫn.</li>
      </ul>
      <p className="mt-4">Quý khách vui lòng liên hệ Hotline <strong>1900 1208</strong> để được hỗ trợ chi tiết nhất về quy trình gửi trả bảo hành.</p>
    </div>
  );
};

export default WarrantyPolicyPage;
