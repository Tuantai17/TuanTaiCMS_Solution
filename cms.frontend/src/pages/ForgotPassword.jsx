import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const OTP_LENGTH = 6;
const OTP_EXPIRE_SECONDS = 300; // 5 phút = 300 giây

const ForgotPassword = () => {
  const [step, setStep] = useState(1); // 1: Nhập Email, 2: Nhập OTP, 3: Nhập mật khẩu mới
  const [email, setEmail] = useState('');
  const [otpDigits, setOtpDigits] = useState(Array(OTP_LENGTH).fill(''));
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // Đếm ngược thời gian hiệu lực OTP
  const [countdown, setCountdown] = useState(0);
  const [canResend, setCanResend] = useState(false);
  const countdownRef = useRef(null);

  // Refs cho 6 ô input OTP
  const otpRefs = useRef([]);

  const navigate = useNavigate();

  // Đồng bộ otpDigits -> code mỗi khi otpDigits thay đổi
  useEffect(() => {
    setCode(otpDigits.join(''));
  }, [otpDigits]);

  // Hàm khởi chạy đồng hồ đếm ngược
  const startCountdown = useCallback(() => {
    setCountdown(OTP_EXPIRE_SECONDS);
    setCanResend(false);

    // Xóa interval cũ nếu có
    if (countdownRef.current) clearInterval(countdownRef.current);

    countdownRef.current = setInterval(() => {
      setCountdown(prev => {
        if (prev <= 1) {
          clearInterval(countdownRef.current);
          setCanResend(true);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  }, []);

  // Dọn dẹp interval khi unmount
  useEffect(() => {
    return () => {
      if (countdownRef.current) clearInterval(countdownRef.current);
    };
  }, []);

  // Format giây thành MM:SS
  const formatTime = (seconds) => {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = (seconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  // Bước 1: Gửi mã OTP
  const handleSendCode = async (e) => {
    if (e) e.preventDefault();
    setError('');
    setMessage('');
    setLoading(true);

    try {
      const response = await authService.sendResetCode(email);
      const successMsg = response.message || 'Mã xác minh đã được gửi về Gmail của bạn.';
      setMessage(successMsg);
      setOtpDigits(Array(OTP_LENGTH).fill(''));
      setStep(2);
      startCountdown();

      // Auto-focus ô đầu tiên sau khi render
      setTimeout(() => {
        if (otpRefs.current[0]) otpRefs.current[0].focus();
      }, 100);
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Không thể xử lý yêu cầu. Vui lòng kiểm tra lại email của bạn hoặc kết nối mạng.');
      }
    } finally {
      setLoading(false);
    }
  };

  // Xử lý gửi lại mã OTP
  const handleResendCode = async () => {
    setError('');
    setMessage('');
    setLoading(true);

    try {
      const response = await authService.sendResetCode(email);
      const successMsg = response.message || 'Mã xác minh mới đã được gửi về Gmail của bạn.';
      setMessage(successMsg);
      setOtpDigits(Array(OTP_LENGTH).fill(''));
      startCountdown();

      // Auto-focus ô đầu tiên
      setTimeout(() => {
        if (otpRefs.current[0]) otpRefs.current[0].focus();
      }, 100);
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Không thể gửi lại mã. Vui lòng kiểm tra kết nối mạng.');
      }
    } finally {
      setLoading(false);
    }
  };

  // Xử lý thay đổi giá trị từng ô OTP
  const handleOtpChange = (index, value) => {
    // Chỉ cho phép nhập số
    if (value && !/^\d$/.test(value)) return;

    const newDigits = [...otpDigits];
    newDigits[index] = value;
    setOtpDigits(newDigits);

    // Tự động focus sang ô tiếp theo khi nhập xong
    if (value && index < OTP_LENGTH - 1) {
      otpRefs.current[index + 1]?.focus();
    }
  };

  // Xử lý phím bấm trên ô OTP (Backspace, Arrow keys)
  const handleOtpKeyDown = (index, e) => {
    if (e.key === 'Backspace') {
      if (!otpDigits[index] && index > 0) {
        // Nếu ô hiện tại trống, quay lại ô trước
        const newDigits = [...otpDigits];
        newDigits[index - 1] = '';
        setOtpDigits(newDigits);
        otpRefs.current[index - 1]?.focus();
      } else {
        // Xóa ô hiện tại
        const newDigits = [...otpDigits];
        newDigits[index] = '';
        setOtpDigits(newDigits);
      }
      e.preventDefault();
    } else if (e.key === 'ArrowLeft' && index > 0) {
      otpRefs.current[index - 1]?.focus();
    } else if (e.key === 'ArrowRight' && index < OTP_LENGTH - 1) {
      otpRefs.current[index + 1]?.focus();
    }
  };

  // Xử lý paste mã OTP từ clipboard
  const handleOtpPaste = (e) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text').trim();
    const digits = pastedData.replace(/\D/g, '').slice(0, OTP_LENGTH).split('');

    if (digits.length > 0) {
      const newDigits = Array(OTP_LENGTH).fill('');
      digits.forEach((digit, i) => {
        newDigits[i] = digit;
      });
      setOtpDigits(newDigits);

      // Focus ô cuối cùng đã được điền hoặc ô tiếp theo
      const lastFilledIndex = Math.min(digits.length, OTP_LENGTH) - 1;
      if (digits.length < OTP_LENGTH) {
        otpRefs.current[digits.length]?.focus();
      } else {
        otpRefs.current[lastFilledIndex]?.focus();
      }
    }
  };

  // Bước 2: Xác minh mã OTP
  const handleVerifyCode = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (code.length !== OTP_LENGTH) {
      setError('Vui lòng nhập đủ 6 chữ số mã OTP.');
      return;
    }

    setLoading(true);

    try {
      const response = await authService.verifyResetCode(email, code);
      setMessage(response.message || 'Mã xác minh hợp lệ. Hãy đặt lại mật khẩu mới.');
      if (countdownRef.current) clearInterval(countdownRef.current);
      setStep(3);
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Mã xác minh không chính xác hoặc đã hết hiệu lực.');
      }
    } finally {
      setLoading(false);
    }
  };

  // Bước 3: Đặt mật khẩu mới
  const handleResetPassword = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (newPassword.length < 6) {
      setError('Mật khẩu mới phải có độ dài tối thiểu 6 ký tự.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Mật khẩu nhập lại không khớp. Vui lòng kiểm tra lại.');
      return;
    }

    setLoading(true);

    try {
      const response = await authService.resetPassword(email, code, newPassword);
      setMessage(response.message || 'Thay đổi mật khẩu thành công!');
      // Đợi 2.5 giây rồi điều hướng về trang Login
      setTimeout(() => {
        navigate('/login');
      }, 2500);
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError('Có lỗi xảy ra khi cập nhật mật khẩu mới.');
      }
    } finally {
      setLoading(false);
    }
  };

  // Tính phần trăm thanh tiến trình đếm ngược
  const countdownPercent = (countdown / OTP_EXPIRE_SECONDS) * 100;
  const isExpiringSoon = countdown > 0 && countdown <= 60;

  return (
    <div className="container my-5 animate--fade-in">
      <div className="row justify-content-center">
        <div className="col-12 col-md-6 col-lg-5">
          <div className="card shadow-lg border-0 rounded-4 overflow-hidden">
            {/* Header Form MyKingdom */}
            <div className="bg-danger text-white text-center py-4 px-3" style={{ background: 'linear-gradient(135deg, #CF102D, #ff3d57)' }}>
              <h4 className="font-weight-bold text-uppercase mb-1">
                <i className="fa-solid fa-user-shield mr-2"></i> Khôi Phục Mật Khẩu
              </h4>
              <p className="small mb-0 opacity-75">Hệ thống bảo mật 3 bước xác thực Gmail</p>
            </div>

            {/* Thanh tiến trình Progress Bar (Premium Design) */}
            <div className="d-flex justify-content-between align-items-center bg-light px-4 py-3 border-bottom" style={{ fontSize: '0.8rem' }}>
              <div className="d-flex align-items-center gap-1">
                <span className={`badge rounded-circle d-inline-flex align-items-center justify-content-center ${step >= 1 ? 'bg-danger text-white' : 'bg-secondary text-white-50'}`} style={{ width: '22px', height: '22px' }}>1</span>
                <span className={`font-weight-bold ${step === 1 ? 'text-danger' : 'text-muted'}`}>Nhập Email</span>
              </div>
              <i className="fa-solid fa-chevron-right text-muted opacity-50" style={{ fontSize: '0.7rem' }}></i>
              <div className="d-flex align-items-center gap-1">
                <span className={`badge rounded-circle d-inline-flex align-items-center justify-content-center ${step >= 2 ? 'bg-danger text-white' : 'bg-secondary text-white-50'}`} style={{ width: '22px', height: '22px' }}>2</span>
                <span className={`font-weight-bold ${step === 2 ? 'text-danger' : 'text-muted'}`}>Nhập OTP</span>
              </div>
              <i className="fa-solid fa-chevron-right text-muted opacity-50" style={{ fontSize: '0.7rem' }}></i>
              <div className="d-flex align-items-center gap-1">
                <span className={`badge rounded-circle d-inline-flex align-items-center justify-content-center ${step >= 3 ? 'bg-danger text-white' : 'bg-secondary text-white-50'}`} style={{ width: '22px', height: '22px' }}>3</span>
                <span className={`font-weight-bold ${step === 3 ? 'text-danger' : 'text-muted'}`}>Mật Khẩu Mới</span>
              </div>
            </div>

            <div className="card-body p-4">
              {error && (
                <div className="alert alert-danger rounded-4 px-3 py-2 text-center small mb-3 shadow-sm" role="alert">
                  <i className="fa-solid fa-triangle-exclamation mr-2"></i> {error}
                </div>
              )}

              {message && (
                <div className="alert alert-success rounded-4 px-3 py-2 text-center small mb-3 shadow-sm" role="alert">
                  <i className="fa-solid fa-circle-check mr-2"></i> {message}
                </div>
              )}

              {/* BƯỚC 1: NHẬP EMAIL */}
              {step === 1 && (
                <form onSubmit={handleSendCode}>
                  <div className="mb-4">
                    <label className="small font-weight-bold text-secondary mb-1">Địa chỉ Email đã đăng ký *</label>
                    <div className="input-group">
                      <div className="input-group-prepend">
                        <span className="input-group-text bg-light border-right-0 rounded-left-pill px-3">
                          <i className="fa-regular fa-envelope text-muted"></i>
                        </span>
                      </div>
                      <input
                        type="email"
                        className="form-control border-left-0 rounded-right-pill px-3 shadow-none"
                        placeholder="Nhập email của bạn..."
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                      />
                    </div>
                    <small className="form-text text-muted mt-2">Hệ thống sẽ gửi một mã OTP gồm 6 chữ số tới hộp thư này.</small>
                  </div>

                  <button
                    type="submit"
                    className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3 shadow-sm"
                    style={{ fontSize: '0.85rem' }}
                    disabled={loading}
                  >
                    {loading ? (
                      <>
                        <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>
                        Đang gửi mã xác minh...
                      </>
                    ) : (
                      <>
                        Gửi Mã Xác Minh <i className="fa-solid fa-paper-plane ml-2"></i>
                      </>
                    )}
                  </button>
                </form>
              )}

              {/* BƯỚC 2: XÁC MINH MÃ OTP */}
              {step === 2 && (
                <form onSubmit={handleVerifyCode}>
                  {/* Đồng hồ đếm ngược */}
                  <div className="mb-4">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <span className="small font-weight-bold text-secondary">
                        <i className="fa-regular fa-clock mr-1"></i> Thời gian còn lại
                      </span>
                      <span 
                        className={`font-weight-bold ${isExpiringSoon ? 'text-danger' : countdown > 0 ? 'text-success' : 'text-muted'}`}
                        style={{ fontSize: '1.1rem', fontVariantNumeric: 'tabular-nums' }}
                      >
                        {countdown > 0 ? (
                          <>
                            <i className={`fa-solid fa-stopwatch mr-1 ${isExpiringSoon ? 'fa-beat' : ''}`}></i>
                            {formatTime(countdown)}
                          </>
                        ) : (
                          <>
                            <i className="fa-solid fa-clock mr-1"></i>
                            Hết hạn
                          </>
                        )}
                      </span>
                    </div>
                    {/* Thanh tiến trình đếm ngược */}
                    <div style={{ 
                      height: '4px', 
                      borderRadius: '4px', 
                      backgroundColor: '#e9ecef', 
                      overflow: 'hidden' 
                    }}>
                      <div style={{
                        height: '100%',
                        width: `${countdownPercent}%`,
                        borderRadius: '4px',
                        backgroundColor: isExpiringSoon ? '#CF102D' : '#28a745',
                        transition: 'width 1s linear, background-color 0.3s ease'
                      }} />
                    </div>
                  </div>

                  {/* 6 ô nhập OTP riêng biệt */}
                  <div className="mb-3">
                    <label className="small font-weight-bold text-secondary mb-2 d-block">Nhập mã xác minh OTP *</label>
                    <div 
                      style={{ 
                        display: 'flex', 
                        justifyContent: 'center', 
                        gap: '10px' 
                      }}
                      onPaste={handleOtpPaste}
                    >
                      {otpDigits.map((digit, index) => (
                        <input
                          key={index}
                          ref={el => otpRefs.current[index] = el}
                          type="text"
                          inputMode="numeric"
                          maxLength="1"
                          value={digit}
                          onChange={(e) => handleOtpChange(index, e.target.value)}
                          onKeyDown={(e) => handleOtpKeyDown(index, e)}
                          onFocus={(e) => e.target.select()}
                          className="otp-digit-input"
                          style={{
                            width: '52px',
                            height: '60px',
                            textAlign: 'center',
                            fontSize: '1.5rem',
                            fontWeight: '800',
                            color: '#CF102D',
                            border: digit ? '2px solid #CF102D' : '2px solid #dee2e6',
                            borderRadius: '12px',
                            outline: 'none',
                            backgroundColor: digit ? '#fff5f5' : '#f8f9fa',
                            transition: 'all 0.2s ease',
                            boxShadow: digit ? '0 4px 12px rgba(207, 16, 45, 0.15)' : '0 2px 4px rgba(0,0,0,0.04)',
                            caretColor: '#CF102D'
                          }}
                        />
                      ))}
                    </div>
                    <small className="form-text text-muted mt-2 text-center d-block">
                      <i className="fa-regular fa-envelope mr-1"></i>
                      Kiểm tra Hộp thư đến hoặc Thư rác (Spam) trong Gmail.
                    </small>
                  </div>

                  {/* Nút gửi lại mã */}
                  <div className="text-center mb-4">
                    {canResend ? (
                      <button
                        type="button"
                        className="btn btn-link text-danger p-0 border-0 shadow-none font-weight-bold small text-decoration-none"
                        onClick={handleResendCode}
                        disabled={loading}
                        style={{ fontSize: '0.85rem' }}
                      >
                        <i className="fa-solid fa-rotate-right mr-1"></i> Gửi lại mã xác minh
                      </button>
                    ) : countdown > 0 ? (
                      <span className="text-muted small">
                        <i className="fa-solid fa-hourglass-half mr-1"></i>
                        Gửi lại mã sau <strong className="text-danger">{formatTime(countdown)}</strong>
                      </span>
                    ) : null}
                  </div>

                  <div className="row g-2">
                    <div className="col-4">
                      <button
                        type="button"
                        className="btn btn-outline-secondary btn-block rounded-pill py-3 font-weight-bold text-uppercase"
                        style={{ fontSize: '0.8rem' }}
                        onClick={() => {
                          setStep(1);
                          if (countdownRef.current) clearInterval(countdownRef.current);
                          setCountdown(0);
                        }}
                      >
                        Quay lại
                      </button>
                    </div>
                    <div className="col-8">
                      <button
                        type="submit"
                        className="btn btn-danger btn-block rounded-pill font-weight-bold text-uppercase py-3 shadow-sm"
                        style={{ fontSize: '0.85rem' }}
                        disabled={loading || code.length !== OTP_LENGTH || countdown === 0}
                      >
                        {loading ? (
                          <>
                            <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>
                            Đang xác thực...
                          </>
                        ) : (
                          <>
                            Xác Minh Mã OTP <i className="fa-solid fa-circle-check ml-2"></i>
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                </form>
              )}

              {/* BƯỚC 3: NHẬP MẬT KHẨU MỚI */}
              {step === 3 && (
                <form onSubmit={handleResetPassword}>
                  <div className="mb-3">
                    <label className="small font-weight-bold text-secondary mb-1">Mật khẩu mới *</label>
                    <div className="input-group">
                      <div className="input-group-prepend">
                        <span className="input-group-text bg-light border-right-0 rounded-left-pill px-3">
                          <i className="fa-solid fa-key text-muted"></i>
                        </span>
                      </div>
                      <input
                        type="password"
                        className="form-control border-left-0 rounded-right-pill px-3 shadow-none"
                        placeholder="Tối thiểu 6 ký tự..."
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  <div className="mb-4">
                    <label className="small font-weight-bold text-secondary mb-1">Xác nhận mật khẩu mới *</label>
                    <div className="input-group">
                      <div className="input-group-prepend">
                        <span className="input-group-text bg-light border-right-0 rounded-left-pill px-3">
                          <i className="fa-solid fa-key text-muted"></i>
                        </span>
                      </div>
                      <input
                        type="password"
                        className="form-control border-left-0 rounded-right-pill px-3 shadow-none"
                        placeholder="Nhập lại mật khẩu..."
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    className="btn btn-success btn-block rounded-pill font-weight-bold text-uppercase py-3 shadow-sm"
                    style={{ fontSize: '0.85rem' }}
                    disabled={loading}
                  >
                    {loading ? (
                      <>
                        <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>
                        Đang cập nhật...
                      </>
                    ) : (
                      <>
                        Đặt Lại Mật Khẩu <i className="fa-solid fa-shield-halved ml-2"></i>
                      </>
                    )}
                  </button>
                </form>
              )}

              <div className="text-center mt-4 pt-3 border-top">
                <span className="text-secondary small">Quay lại trang </span>
                <Link to="/login" className="font-weight-bold text-danger text-decoration-none small hover-underline">
                  Đăng nhập tại đây
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
