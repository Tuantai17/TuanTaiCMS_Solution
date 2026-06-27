const IMAGE_BASE_URL = process.env.REACT_APP_IMAGE_BASE_URL || (process.env.REACT_APP_API_URL || 'https://localhost:7238/api').replace(/\/api\/?$/i, '');

export const getMediaUrl = (url, fallback = '') => {
  if (!url) {
    return fallback;
  }

  if (/^(https?:|data:|blob:)/i.test(url)) {
    return url;
  }

  const normalizedPath = url.startsWith('/') ? url : `/${url}`;
  return `${IMAGE_BASE_URL}${normalizedPath}`;
};
