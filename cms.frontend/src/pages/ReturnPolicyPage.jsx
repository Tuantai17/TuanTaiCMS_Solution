import React, { useEffect } from 'react';

const ReturnPolicyPage = () => {
  useEffect(() => {
    document.title = "Đổi trả trong 7 ngày | MyKingdom";
  }, []);

  return (
    <div className="container py-5" style={{ maxWidth: '900px' }}>
      <h1 className="text-center font-weight-bold text-danger mb-4">CHÍNH SÁCH ĐỔI TRẢ TRONG 7 NGÀY</h1>
      <div className="content-page" style={{ fontSize: '1.05rem', lineHeight: '1.8' }}>
        <p>Với mong muốn mang lại sự hài lòng tuyệt đối và sự an tâm cho khách hàng khi mua sắm, MyKingdom áp dụng chính sách <strong>Đổi trả trong vòng 7 ngày</strong> đối với các sản phẩm đáp ứng đủ điều kiện theo quy định.</p>

        <h3 className="text-danger mt-4 font-weight-bold">1. Điều kiện áp dụng đổi trả</h3>
        <p>Sản phẩm chỉ được chấp nhận đổi/trả khi đáp ứng các điều kiện sau đây:</p>
        <ul>
          <li>Sản phẩm còn trong thời hạn <strong>7 ngày</strong> kể từ ngày khách hàng nhận được hàng (căn cứ theo hóa đơn hoặc biên nhận giao hàng).</li>
          <li>Sản phẩm còn nguyên vẹn bao bì, hộp đựng, tem nhãn mác, tem niêm phong (nếu có).</li>
          <li>Sản phẩm chưa qua sử dụng, không bị trầy xước, nứt vỡ, móp méo, hoặc có dấu hiệu đã qua tác động ngoại lực.</li>
          <li>Còn đầy đủ các phụ kiện, linh kiện, sách hướng dẫn sử dụng và quà tặng kèm (nếu có).</li>
          <li>Cung cấp đầy đủ hóa đơn mua hàng hoặc thông tin chứng minh đơn hàng hợp lệ tại MyKingdom.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">2. Các trường hợp không hỗ trợ đổi trả</h3>
        <p>Chúng tôi rất tiếc không thể hỗ trợ đổi/trả đối với các trường hợp sau:</p>
        <ul>
          <li>Sản phẩm đã quá thời hạn 7 ngày kể từ lúc nhận hàng.</li>
          <li>Sản phẩm đã bị bóc tem niêm phong, rách bao bì hoặc có dấu hiệu đã qua sử dụng (trừ trường hợp lỗi kỹ thuật từ nhà sản xuất).</li>
          <li>Các sản phẩm nằm trong chương trình khuyến mãi đặc biệt, xả hàng (trừ khi có quy định khác).</li>
          <li>Sản phẩm bị hư hỏng do khách hàng bảo quản không đúng cách hoặc sử dụng sai hướng dẫn.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">3. Đổi trả do lỗi từ nhà sản xuất hoặc quá trình vận chuyển</h3>
        <p>Trong trường hợp sản phẩm bị lỗi kỹ thuật, thiếu chi tiết hoặc hư hỏng do quá trình vận chuyển:</p>
        <ul>
          <li>MyKingdom sẽ chịu hoàn toàn chi phí vận chuyển đổi/trả hàng.</li>
          <li>Khách hàng vui lòng cung cấp video mở hộp (unboxing) hoặc hình ảnh rõ nét chứng minh tình trạng sản phẩm ngay khi nhận hàng để được hỗ trợ nhanh nhất.</li>
          <li>Sản phẩm sẽ được đổi lấy sản phẩm mới cùng loại. Trường hợp hết hàng, khách hàng có thể đổi sang sản phẩm khác có giá trị tương đương hoặc nhận hoàn tiền.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">4. Quy trình thực hiện đổi trả</h3>
        <p>Khách hàng vui lòng thực hiện theo các bước sau:</p>
        <ol>
          <li><strong>Bước 1:</strong> Liên hệ với bộ phận CSKH của MyKingdom qua Hotline hoặc Email để thông báo về yêu cầu đổi trả, cung cấp mã đơn hàng và lý do.</li>
          <li><strong>Bước 2:</strong> Nhận hướng dẫn từ nhân viên CSKH về việc đóng gói và địa chỉ gửi hàng hoàn trả.</li>
          <li><strong>Bước 3:</strong> Đóng gói sản phẩm cẩn thận (kèm hóa đơn và phụ kiện/quà tặng) và gửi qua bưu điện/đơn vị vận chuyển.</li>
          <li><strong>Bước 4:</strong> MyKingdom sẽ kiểm tra tình trạng sản phẩm sau khi nhận lại và tiến hành gửi sản phẩm đổi hoặc hoàn tiền trong thời gian sớm nhất.</li>
        </ol>

        <h3 className="text-danger mt-4 font-weight-bold">5. Thời gian xử lý hoàn tiền</h3>
        <p>Trường hợp yêu cầu hoàn tiền được chấp thuận, thời gian khách hàng nhận được tiền sẽ phụ thuộc vào phương thức thanh toán:</p>
        <ul>
          <li><strong>Chuyển khoản / COD:</strong> Nhận lại tiền qua tài khoản ngân hàng trong vòng 3 - 5 ngày làm việc.</li>
          <li><strong>Thẻ tín dụng / Ví điện tử:</strong> Thời gian hoàn tiền tùy thuộc vào quy định của ngân hàng phát hành thẻ hoặc đơn vị cung cấp ví điện tử (có thể từ 7 - 14 ngày làm việc).</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">6. Thông tin liên hệ hỗ trợ</h3>
        <p>Nếu bạn có bất kỳ thắc mắc nào về chính sách đổi trả, xin vui lòng liên hệ:</p>
        <ul>
          <li><strong>Hotline:</strong> 1900 1208</li>
          <li><strong>Email:</strong> <a href="mailto:hotro@mykingdom.vn" className="text-danger">hotro@mykingdom.vn</a></li>
          <li><strong>Thời gian hoạt động:</strong> 08:00 – 21:00 tất cả các ngày trong tuần.</li>
        </ul>
      </div>
    </div>
  );
};

export default ReturnPolicyPage;
