import React from 'react';

const StoresPage = () => {
  const stores = [
    { name: 'MyKingdom Quận 8', address: '180 Cao Lỗ, Phường 4, Quận 8, TP.HCM', phone: '1900 1208' },
    { name: 'MyKingdom Quận 1', address: 'Vincom Center, 72 Lê Thánh Tôn, Bến Nghé, Quận 1, TP.HCM', phone: '1900 1208' },
    { name: 'MyKingdom Hà Nội', address: 'Vincom Mega Mall Royal City, 72A Nguyễn Trãi, Hà Nội', phone: '1900 1208' },
    { name: 'MyKingdom Đà Nẵng', address: 'Vincom Plaza, 910A Ngô Quyền, Sơn Trà, Đà Nẵng', phone: '1900 1208' },
    { name: 'MyKingdom Cần Thơ', address: 'Sense City, 01 Hòa Bình, Ninh Kiều, Cần Thơ', phone: '1900 1208' },
    { name: 'MyKingdom Hải Phòng', address: 'Aeon Mall, Số 10 Võ Nguyên Giáp, Lê Chân, Hải Phòng', phone: '1900 1208' },
  ];

  return (
    <div className="container py-5 my-5" style={{ minHeight: '50vh' }}>
      <h2 className="mb-4 font-weight-bold text-danger text-center">Hệ thống cửa hàng MyKingdom</h2>
      <p className="mb-5 text-center text-secondary">Khám phá vương quốc đồ chơi tại các cửa hàng của chúng tôi trên toàn quốc.</p>
      
      <div className="row">
        {stores.map((store, index) => (
          <div className="col-md-4 mb-4" key={index}>
            <div className="card h-100 shadow-sm border-0" style={{ borderRadius: '12px', overflow: 'hidden' }}>
              <div className="card-body p-4">
                <h5 className="font-weight-bold text-dark mb-3">
                  <i className="fa-solid fa-store text-danger mr-2"></i> {store.name}
                </h5>
                <p className="text-secondary mb-2" style={{ fontSize: '0.95rem' }}>
                  <i className="fa-solid fa-location-dot text-danger mr-2" style={{ width: '16px' }}></i> {store.address}
                </p>
                <p className="text-secondary mb-0" style={{ fontSize: '0.95rem' }}>
                  <i className="fa-solid fa-phone text-success mr-2" style={{ width: '16px' }}></i> Hotline: <strong>{store.phone}</strong>
                </p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default StoresPage;
