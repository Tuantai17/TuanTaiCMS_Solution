import React, { useState, useEffect, useRef } from 'react';
import '../assets/css/SearchableSelect.css';

/**
 * Component SearchableSelect - Dropdown tùy chỉnh hỗ trợ tìm kiếm
 * Cho phép người dùng gõ từ khóa để lọc các tùy chọn (ví dụ: chọn Tỉnh/Thành phố)
 */
const SearchableSelect = ({ options, value, onChange, placeholder, disabled, icon, hasError }) => {
  // State quản lý việc đóng/mở danh sách dropdown
  const [isOpen, setIsOpen] = useState(false);
  // State lưu trữ từ khóa tìm kiếm do người dùng nhập vào
  const [searchTerm, setSearchTerm] = useState('');
  // Tham chiếu đến vùng div bao bọc component để xử lý click ra ngoài
  const wrapperRef = useRef(null);

  // Effect: Đóng dropdown khi người dùng click chuột ra ngoài khu vực của component
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };
    // Đăng ký sự kiện lắng nghe click chuột toàn trang
    document.addEventListener('mousedown', handleClickOutside);
    // Hủy đăng ký sự kiện khi component bị unmount để tránh rò rỉ bộ nhớ
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Lọc danh sách tùy chọn dựa trên từ khóa tìm kiếm (không phân biệt hoa/thường)
  const filteredOptions = options.filter(opt => 
    opt.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Xử lý khi người dùng chọn một mục trong danh sách
  const handleSelect = (val) => {
    onChange(val); // Truyền giá trị đã chọn ra component cha
    setIsOpen(false); // Đóng dropdown
    setSearchTerm(''); // Xóa từ khóa tìm kiếm
  };

  return (
    <div className={`modern-searchable-select ${disabled ? 'disabled' : ''}`} ref={wrapperRef}>
      <div 
        className={`mss-header ${hasError ? 'has-error' : ''}`} 
        onClick={() => !disabled && setIsOpen(!isOpen)}
      >
        <span className="mss-icon"><i className={icon}></i></span>
        <span className={`mss-value ${!value ? 'placeholder' : ''}`}>
          {value || placeholder}
        </span>
        <span className={`mss-arrow ${isOpen ? 'open' : ''}`}><i className="fa-solid fa-chevron-down"></i></span>
      </div>
      
      {isOpen && (
        <div className="mss-dropdown">
          <div className="mss-search-box">
            <i className="fa-solid fa-magnifying-glass"></i>
            <input 
              type="text" 
              placeholder="Tìm kiếm..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              autoFocus
            />
          </div>
          <ul className="mss-list">
            {filteredOptions.length > 0 ? (
              filteredOptions.map(opt => (
                <li 
                  key={opt.code} 
                  className={`mss-item ${value === opt.name ? 'selected' : ''}`}
                  onClick={() => handleSelect(opt.name)}
                >
                  {opt.name}
                </li>
              ))
            ) : (
              <li className="mss-empty">Không tìm thấy kết quả</li>
            )}
          </ul>
        </div>
      )}
    </div>
  );
};

export default SearchableSelect;
