import React, { useEffect } from 'react';

const OriginalToysPage = () => {
  useEffect(() => {
    document.title = "Đồ chơi chính hãng 100% | MyKingdom";
  }, []);

  return (
    <div className="container py-5" style={{ maxWidth: '900px' }}>
      <h1 className="text-center font-weight-bold text-danger mb-4">ĐỒ CHƠI CHÍNH HÃNG 100%</h1>
      <div className="content-page" style={{ fontSize: '1.05rem', lineHeight: '1.8' }}>
        <p>Tại MyKingdom, chúng tôi cam kết cung cấp các sản phẩm đồ chơi chính hãng, có nguồn gốc rõ ràng và đến từ những thương hiệu uy tín trong nước cũng như quốc tế.</p>
        <p>Mỗi sản phẩm đều được kiểm tra kỹ trước khi đưa đến tay khách hàng, nhằm bảo đảm chất lượng, độ an toàn và mang đến trải nghiệm vui chơi tốt nhất cho trẻ em.</p>

        <h3 className="text-danger mt-4 font-weight-bold">1. Nguồn gốc sản phẩm rõ ràng</h3>
        <p>Các sản phẩm được phân phối tại MyKingdom có đầy đủ thông tin về:</p>
        <ul>
          <li>Thương hiệu và nhà sản xuất.</li>
          <li>Xuất xứ sản phẩm.</li>
          <li>Đơn vị nhập khẩu hoặc phân phối.</li>
          <li>Tem nhãn và thông tin sản phẩm theo quy định.</li>
          <li>Hóa đơn, chứng từ liên quan đến nguồn gốc hàng hóa.</li>
        </ul>
        <p>MyKingdom không kinh doanh hàng giả, hàng nhái, hàng không rõ nguồn gốc hoặc sản phẩm không đáp ứng yêu cầu chất lượng.</p>

        <h3 className="text-danger mt-4 font-weight-bold">2. Sản phẩm từ các thương hiệu uy tín</h3>
        <p>MyKingdom lựa chọn sản phẩm từ những thương hiệu đồ chơi được nhiều khách hàng tin tưởng.</p>
        <p>Các sản phẩm được đánh giá dựa trên những tiêu chí như:</p>
        <ul>
          <li>Chất lượng sản xuất.</li>
          <li>Độ bền của sản phẩm.</li>
          <li>Tính giáo dục và khả năng phát triển tư duy.</li>
          <li>Mức độ phù hợp với từng độ tuổi.</li>
          <li>Tiêu chuẩn an toàn dành cho trẻ em.</li>
        </ul>
        <p>Thông tin về thương hiệu, độ tuổi sử dụng và hướng dẫn sử dụng được hiển thị tại trang chi tiết của từng sản phẩm.</p>

        <h3 className="text-danger mt-4 font-weight-bold">3. Kiểm tra chất lượng trước khi giao hàng</h3>
        <p>Trước khi đóng gói và giao đến khách hàng, sản phẩm được kiểm tra các nội dung cơ bản:</p>
        <ul>
          <li>Tình trạng bao bì.</li>
          <li>Tem nhãn sản phẩm.</li>
          <li>Số lượng sản phẩm và phụ kiện đi kèm.</li>
          <li>Màu sắc, mẫu mã và phiên bản sản phẩm.</li>
          <li>Các dấu hiệu hư hỏng hoặc lỗi bên ngoài.</li>
        </ul>
        <p>Trong trường hợp phát hiện sản phẩm không đáp ứng yêu cầu, MyKingdom sẽ thay thế sản phẩm khác trước khi thực hiện giao hàng.</p>

        <h3 className="text-danger mt-4 font-weight-bold">4. An toàn cho trẻ em</h3>
        <p>Các sản phẩm được lựa chọn dựa trên tiêu chí phù hợp với độ tuổi và an toàn trong quá trình sử dụng.</p>
        <p>Khách hàng nên đọc kỹ:</p>
        <ul>
          <li>Độ tuổi khuyến nghị.</li>
          <li>Cảnh báo an toàn.</li>
          <li>Hướng dẫn lắp ráp.</li>
          <li>Hướng dẫn bảo quản.</li>
          <li>Các lưu ý khi trẻ sử dụng sản phẩm.</li>
        </ul>
        <p>Đối với những sản phẩm có chi tiết nhỏ, pin, linh kiện điện tử hoặc yêu cầu lắp ráp, trẻ em nên sử dụng dưới sự hướng dẫn và giám sát của người lớn.</p>

        <h3 className="text-danger mt-4 font-weight-bold">5. Cam kết của MyKingdom</h3>
        <p>MyKingdom cam kết:</p>
        <ul>
          <li>Cung cấp sản phẩm chính hãng và có nguồn gốc rõ ràng.</li>
          <li>Hiển thị thông tin sản phẩm minh bạch.</li>
          <li>Không kinh doanh hàng giả hoặc hàng kém chất lượng.</li>
          <li>Hỗ trợ khách hàng khi sản phẩm có dấu hiệu bất thường.</li>
          <li>Tiếp nhận phản hồi về chất lượng sản phẩm.</li>
          <li>Thực hiện đổi trả theo chính sách hiện hành nếu sản phẩm đủ điều kiện.</li>
        </ul>

        <h3 className="text-danger mt-4 font-weight-bold">6. Cách kiểm tra thông tin sản phẩm</h3>
        <p>Khách hàng có thể kiểm tra thông tin sản phẩm thông qua:</p>
        <ol>
          <li>Tên thương hiệu và nhà sản xuất trên bao bì.</li>
          <li>Tem phụ hoặc thông tin đơn vị nhập khẩu.</li>
          <li>Mã sản phẩm, mã SKU hoặc mã vạch.</li>
          <li>Thông tin được công bố trên trang chi tiết sản phẩm.</li>
          <li>Hóa đơn hoặc thông tin đơn hàng sau khi mua.</li>
        </ol>
        <p>Khi có nghi ngờ về nguồn gốc hoặc chất lượng sản phẩm, khách hàng không nên tiếp tục sử dụng và cần liên hệ với bộ phận chăm sóc khách hàng để được kiểm tra.</p>

        <h3 className="text-danger mt-4 font-weight-bold">7. Liên hệ hỗ trợ</h3>
        <p>Khách hàng cần hỗ trợ kiểm tra sản phẩm có thể liên hệ:</p>
        <ul>
          <li><strong>Hotline:</strong> 1900 1208</li>
          <li><strong>Email:</strong> <a href="mailto:hotro@mykingdom.vn" className="text-danger">hotro@mykingdom.vn</a></li>
          <li><strong>Thời gian hỗ trợ:</strong> 08:00 – 21:00 mỗi ngày</li>
          <li><strong>Kênh hỗ trợ:</strong> Hotline, email hoặc biểu mẫu liên hệ trên website</li>
        </ul>
        <p>Khi liên hệ, khách hàng nên cung cấp mã đơn hàng, tên sản phẩm và hình ảnh thực tế để được hỗ trợ nhanh chóng.</p>
      </div>
    </div>
  );
};

export default OriginalToysPage;
