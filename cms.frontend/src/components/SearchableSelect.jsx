import React, { useState, useEffect, useRef } from 'react';
import '../assets/css/SearchableSelect.css';

const SearchableSelect = ({ options, value, onChange, placeholder, disabled, icon, hasError }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const wrapperRef = useRef(null);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const filteredOptions = options.filter(opt => 
    opt.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSelect = (val) => {
    onChange(val);
    setIsOpen(false);
    setSearchTerm('');
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
