export const ORDER_STATUS = {
  PENDING: 0,
  SHIPPING: 1,
  COMPLETED: 2,
  CANCELLED: 3,
};

export const ORDER_STATUS_OPTIONS = [
  {
    key: 'all',
    label: 'Tất cả',
    value: '',
    icon: 'fa-solid fa-border-all',
    tone: 'all',
  },
  {
    key: 'pending',
    label: 'Chờ duyệt',
    value: String(ORDER_STATUS.PENDING),
    icon: 'fa-regular fa-hourglass-half',
    tone: 'pending',
  },
  {
    key: 'shipping',
    label: 'Đang giao',
    value: String(ORDER_STATUS.SHIPPING),
    icon: 'fa-solid fa-truck-fast',
    tone: 'shipping',
  },
  {
    key: 'completed',
    label: 'Hoàn thành',
    value: String(ORDER_STATUS.COMPLETED),
    icon: 'fa-regular fa-circle-check',
    tone: 'completed',
  },
  {
    key: 'cancelled',
    label: 'Đã hủy',
    value: String(ORDER_STATUS.CANCELLED),
    icon: 'fa-regular fa-circle-xmark',
    tone: 'cancelled',
  },
];

export const getOrderStatusMeta = (status) => {
  switch (Number(status)) {
    case ORDER_STATUS.PENDING:
      return {
        label: 'Chờ duyệt',
        badgeClass: 'pending',
        icon: 'fa-regular fa-hourglass-half',
      };
    case ORDER_STATUS.SHIPPING:
      return {
        label: 'Đang giao',
        badgeClass: 'shipping',
        icon: 'fa-solid fa-truck-fast',
      };
    case ORDER_STATUS.COMPLETED:
      return {
        label: 'Hoàn thành',
        badgeClass: 'completed',
        icon: 'fa-regular fa-circle-check',
      };
    case ORDER_STATUS.CANCELLED:
      return {
        label: 'Đã hủy',
        badgeClass: 'cancelled',
        icon: 'fa-regular fa-circle-xmark',
      };
    default:
      return {
        label: 'Không xác định',
        badgeClass: 'unknown',
        icon: 'fa-regular fa-circle-question',
      };
  }
};

export const formatOrderCode = (orderId) => {
  return `MKD${String(orderId).padStart(8, '0')}`;
};
