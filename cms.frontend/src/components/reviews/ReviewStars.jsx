import React from 'react';

function ReviewStars({ value = 0, onChange, size = '1rem', interactive = false }) {
  return (
    <div className={`review-stars ${interactive ? 'interactive' : ''}`}>
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          className="review-star-button"
          onClick={() => interactive && onChange?.(star)}
          style={{ fontSize: size, cursor: interactive ? 'pointer' : 'default' }}
          disabled={!interactive}
          aria-label={`${star} sao`}
        >
          <i className={`fa-star ${star <= value ? 'fa-solid active' : 'fa-regular'}`}></i>
        </button>
      ))}
    </div>
  );
}

export default ReviewStars;
