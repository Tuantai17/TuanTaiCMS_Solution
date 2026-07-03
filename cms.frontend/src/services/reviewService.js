import axiosClient from '../api/axiosClient';

const reviewService = {
  getEligibility(orderDetailId) {
    return axiosClient.get(`/reviews/eligibility/${orderDetailId}`);
  },

  createReview(formData) {
    return axiosClient.post('/reviews', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  getProductReviews(productId, params) {
    return axiosClient.get(`/reviews/product/${productId}`, { params: { ...params, _t: Date.now() } });
  },

  getProductReviewSummary(productId) {
    return axiosClient.get(`/reviews/product/${productId}/summary`, { params: { _t: Date.now() } });
  },

  getMyReviews(params) {
    return axiosClient.get('/reviews/my', { params: { ...params, _t: Date.now() } });
  },

  getMyReviewById(reviewId) {
    return axiosClient.get(`/reviews/my/${reviewId}`);
  },
};

export default reviewService;
