import React from 'react';

const ContactPage = () => {
  const handleSubmit = (e) => {
    e.preventDefault();
    alert('Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.');
  };

  return (
    <div className="container py-5 my-5" style={{ minHeight: '50vh' }}>
      <h2 className="mb-2 font-weight-bold text-danger text-center">Liên hệ hợp tác</h2>
      <p className="mb-5 text-center text-secondary">Vui lòng để lại thông tin, đội ngũ MyKingdom sẽ chủ động liên hệ lại với bạn.</p>
      
      <div className="row justify-content-center">
        <div className="col-md-7">
          <div className="bg-light p-5 rounded shadow-sm border" style={{ borderColor: '#f0f0f0' }}>
            <h5 className="mb-4 font-weight-bold border-bottom pb-3">Gửi biểu mẫu liên hệ</h5>
            <form onSubmit={handleSubmit}>
              <div className="form-group mb-3">
                <label className="font-weight-bold text-dark mb-1">Họ và tên <span className="text-danger">*</span></label>
                <input type="text" className="form-control py-2" placeholder="Nhập họ tên của bạn" required />
              </div>
              
              <div className="form-group mb-3">
                <label className="font-weight-bold text-dark mb-1">Số điện thoại <span className="text-danger">*</span></label>
                <input type="tel" className="form-control py-2" placeholder="Nhập số điện thoại" required />
              </div>
              
              <div className="form-group mb-3">
                <label className="font-weight-bold text-dark mb-1">Email</label>
                <input type="email" className="form-control py-2" placeholder="Nhập địa chỉ email" />
              </div>
              
              <div className="form-group mb-4">
                <label className="font-weight-bold text-dark mb-1">Nội dung hợp tác <span className="text-danger">*</span></label>
                <textarea className="form-control" rows={5} placeholder="Nhập chi tiết nội dung cần liên hệ..." required></textarea>
              </div>
              
              <button type="submit" className="btn btn-danger w-100 py-2 font-weight-bold text-uppercase" style={{ letterSpacing: '1px' }}>
                Gửi liên hệ
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ContactPage;
