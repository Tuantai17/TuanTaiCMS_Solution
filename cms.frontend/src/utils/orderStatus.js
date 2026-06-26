export const ORDER_STATUS = {
  PENDING: 0,
  CONFIRMED: 1,
  PREPARING: 2,
  SHIPPING: 3,
  COMPLETED: 4,
  CANCELLED: 5,
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
    key: 'confirmed',
    label: 'Đã duyệt',
    value: String(ORDER_STATUS.CONFIRMED),
    icon: 'fa-solid fa-check',
    tone: 'pending', // You can add new tones in CSS if you want
  },
  {
    key: 'preparing',
    label: 'Đang chuẩn bị',
    value: String(ORDER_STATUS.PREPARING),
    icon: 'fa-solid fa-box-open',
    tone: 'shipping',
  },
  {
    key: 'shipping',
    label: 'Đang giao hàng',
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
    case ORDER_STATUS.CONFIRMED:
      return {
        label: 'Đã duyệt',
        badgeClass: 'pending', // Reusing pending badge style for now
        icon: 'fa-solid fa-check',
      };
    case ORDER_STATUS.PREPARING:
      return {
        label: 'Đang chuẩn bị',
        badgeClass: 'shipping', // Reusing shipping badge style
        icon: 'fa-solid fa-box-open',
      };
    case ORDER_STATUS.SHIPPING:
      return {
        label: 'Đang giao hàng',
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

export const parseOrderNotes = (rawNotes) => {
  if (!rawNotes) return { deliveryInfo: '', paymentMethod: '', customerNotes: '' };
  
  let deliveryInfo = '';
  let paymentMethod = '';
  let customerNotes = '';
  
  const deliveryMatch = rawNotes.match(/\[Giao tới:(.*?)\]/);
  if (deliveryMatch) {
    deliveryInfo = deliveryMatch[1].trim();
  }
  
  const ptttMatch = rawNotes.match(/\[PTTT:(.*?)\]/);
  if (ptttMatch) {
    paymentMethod = ptttMatch[1].trim();
  }
  
  const notesMatch = rawNotes.match(/Ghi chú KH:\s*(.*)/);
  if (notesMatch) {
    customerNotes = notesMatch[1].trim();
  } else {
    if (!deliveryMatch && !ptttMatch) {
      customerNotes = rawNotes;
    }
  }
  
  return { deliveryInfo, paymentMethod, customerNotes };
};
