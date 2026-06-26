import React, { useEffect } from 'react';

const SupportPage = () => {
  useEffect(() => {
    document.title = "Hỗ trợ khách hàng 24/7 | MyKingdom";
  }, []);

  return (
    <div className="container py-5" style={{ maxWidth: '900px' }}>
      <h1 className="text-center font-weight-bold text-danger mb-4">HỖ TRỢ KHÁCH HÀNG 24/7</h1>
      <div className="content-page" style={{ fontSize: '1.05rem', lineHeight: '1.8' }}>
        <p>Tại MyKingdom, trải nghiệm mua sắm của bạn luôn là ưu tiên hàng đầu của chúng tôi. Với mong muốn mang lại dịch vụ tốt nhất và giải quyết mọi thắc mắc của khách hàng một cách nhanh chóng, chúng tôi tự hào cung cấp dịch vụ <strong>Hỗ trợ khách hàng 24/7</strong>.</p>

        <h3 className="text-danger mt-4 font-weight-bold">1. Các kênh hỗ trợ trực tuyến</h3>
        <p>Bất kể ngày đêm hay dịp Lễ/Tết, đội ngũ chăm sóc khách hàng của MyKingdom luôn sẵn sàng lắng nghe và hỗ trợ bạn qua các kênh sau:</p>
        <ul>
          <li><strong>Hotline (Miễn phí cước gọi):</strong> 1900 1208</li>
          <li><strong>Email Hỗ Trợ:</strong> <a href="mailto:hotro@mykingdom.vn" className="text-danger">hotro@mykingdom.vn</a></li>
          <li><strong>Chat Trực Tuyến:</strong> Tính năng live chat ngay trên website hoạt động 24/7, luôn có nhân viên túc trực để phản hồi bạn.</li>
          <li><strong>Fanpage Facebook:</strong> Gửi tin nhắn qua Fanpage chính thức của MyKingdom để được hỗ trợ giải đáp nhanh.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">2. Nội dung hỗ trợ</h3>
        <p>Đội ngũ CSKH của chúng tôi sẵn sàng giải đáp và xử lý các vấn đề bao gồm (nhưng không giới hạn):</p>
        <ul>
          <li>Tư vấn thông tin chi tiết về sản phẩm, độ tuổi phù hợp và hướng dẫn sử dụng.</li>
          <li>Hỗ trợ hướng dẫn đặt hàng, thanh toán và sử dụng mã giảm giá.</li>
          <li>Cập nhật tình trạng đơn hàng và theo dõi hành trình giao hàng.</li>
          <li>Tiếp nhận và xử lý các yêu cầu đổi trả hàng hóa, bảo hành sản phẩm.</li>
          <li>Giải quyết khiếu nại, phản hồi chất lượng dịch vụ hoặc thái độ nhân viên giao hàng.</li>
          <li>Tư vấn về chương trình thẻ thành viên, tích lũy điểm và các ưu đãi đặc quyền.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">3. Cam kết thời gian phản hồi</h3>
        <p>MyKingdom cam kết đem lại sự hỗ trợ nhanh nhất có thể:</p>
        <ul>
          <li><strong>Kênh Hotline & Live Chat:</strong> Phản hồi ngay lập tức hoặc trong vòng 5 phút sau khi kết nối.</li>
          <li><strong>Kênh Email & Fanpage:</strong> Phản hồi chậm nhất trong vòng 2 - 4 giờ làm việc. Riêng các yêu cầu xử lý khiếu nại chuyên sâu có thể mất từ 1 - 2 ngày làm việc để kiểm tra và xử lý dứt điểm.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">4. Câu hỏi thường gặp (FAQ)</h3>
        <p>Trước khi liên hệ trực tiếp, bạn cũng có thể tham khảo mục Câu Hỏi Thường Gặp (FAQ) trên website của chúng tôi để tìm thấy những câu trả lời nhanh nhất cho các vấn đề phổ biến như:</p>
        <ul>
          <li>Cách sử dụng mã giảm giá.</li>
          <li>Phí vận chuyển và thời gian giao hàng dự kiến.</li>
          <li>Cách kiểm tra điểm tích lũy của thành viên.</li>
        </ul>

        <div className="alert alert-danger mt-5 text-center">
          <strong>Cảm ơn bạn đã tin tưởng và mua sắm tại MyKingdom!</strong>
          <br/>
          Nếu bạn cần bất kỳ sự trợ giúp nào, đừng ngần ngại nhấc máy và gọi ngay cho chúng tôi theo số Hotline <strong>1900 1208</strong>.
        </div>
      </div>
    </div>
  );
};

export default SupportPage;
