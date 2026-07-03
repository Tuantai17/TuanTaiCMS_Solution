import React from 'react';

const PaymentPolicyPage = () => {
  return (
    <div className="container py-5 my-5" style={{ minHeight: '50vh' }}>
      <h2 className="mb-4 font-weight-bold text-danger">Phương thức thanh toán</h2>
      <p>Để mang lại sự tiện lợi tối đa cho quá trình mua sắm, hệ thống MyKingdom hỗ trợ đa dạng các hình thức thanh toán để quý khách dễ dàng lựa chọn:</p>
      <ul>
        <li className="mb-2"><strong className="text-dark">Thanh toán tiền mặt khi nhận hàng (COD):</strong> Khách hàng thanh toán trực tiếp cho nhân viên giao hàng sau khi nhận và kiểm tra kiện hàng.</li>
        <li className="mb-2"><strong className="text-dark">Thanh toán qua thẻ Ngân hàng (ATM/Visa/MasterCard/JCB):</strong> Khách hàng có thể thanh toán trực tuyến qua cổng thanh toán bảo mật.</li>
        <li className="mb-2"><strong className="text-dark">Thanh toán qua Ví điện tử:</strong> Hỗ trợ thanh toán nhanh chóng qua MoMo, ZaloPay, VNPay.</li>
        <li className="mb-2"><strong className="text-dark">Chuyển khoản trực tiếp:</strong> Quý khách có thể chuyển tiền trực tiếp vào tài khoản ngân hàng của công ty theo cú pháp Mã Đơn Hàng.</li>
      </ul>
      <p className="mt-4 text-muted"><em>Lưu ý: Mọi giao dịch trực tuyến trên hệ thống website MyKingdom đều được mã hóa bảo mật theo tiêu chuẩn an toàn thông tin quốc tế.</em></p>
    </div>
  );
};

export default PaymentPolicyPage;
