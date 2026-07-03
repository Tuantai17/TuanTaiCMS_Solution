import axiosClient from '../api/axiosClient';
import { getMediaUrl } from '../utils/mediaUrl';

export const SUPPORT_CATEGORY_OPTIONS = [
  { value: 'order', label: 'Đơn hàng' },
  { value: 'product', label: 'Sản phẩm' },
  { value: 'payment', label: 'Thanh toán' },
  { value: 'shipping', label: 'Giao hàng' },
  { value: 'account', label: 'Tài khoản' },
  { value: 'promotion', label: 'Khuyến mãi' },
  { value: 'other', label: 'Khác' },
];

export const SUPPORT_STATUS_OPTIONS = [
  { value: 'all', label: 'Tất cả' },
  { value: 'new', label: 'Mới tiếp nhận' },
  { value: 'in-progress', label: 'Đang xử lý' },
  { value: 'waiting-customer', label: 'Chờ phản hồi' },
  { value: 'resolved', label: 'Đã giải quyết' },
  { value: 'closed', label: 'Đã đóng' },
];

export const SUPPORT_STICKERS = [
  { code: 'bear-wave', label: 'Gấu chào', emoji: '🧸' },
  { code: 'bear-love', label: 'Gấu tim', emoji: '💗' },
  { code: 'rabbit-smile', label: 'Thỏ vui', emoji: '🐰' },
  { code: 'party', label: 'Ăn mừng', emoji: '🎉' },
  { code: 'star', label: 'Ngôi sao', emoji: '⭐' },
  { code: 'gift', label: 'Quà tặng', emoji: '🎁' },
];

export const SUPPORT_EMOJIS = ['😀', '😁', '😍', '🥰', '😂', '😢', '😮', '😕', '😡', '👍', '🙏', '❤️'];

const getCategoryLabel = (category) =>
  SUPPORT_CATEGORY_OPTIONS.find((item) => item.value === category)?.label || 'Khác';

export const getSupportStatusMeta = (status) => {
  const map = {
    new: { label: 'Mới tiếp nhận', tone: 'new' },
    'in-progress': { label: 'Đang xử lý', tone: 'progress' },
    'waiting-customer': { label: 'Chờ phản hồi', tone: 'waiting' },
    resolved: { label: 'Đã giải quyết', tone: 'resolved' },
    closed: { label: 'Đã đóng', tone: 'closed' },
  };

  return map[status] || map.new;
};

export const getSupportPriorityMeta = (priority) => {
  const map = {
    low: { label: 'Thấp', tone: 'low' },
    normal: { label: 'Bình thường', tone: 'normal' },
    high: { label: 'Cao', tone: 'high' },
    urgent: { label: 'Khẩn cấp', tone: 'urgent' },
  };

  return map[priority] || map.normal;
};

const buildTicketFormData = (payload = {}) => {
  const formData = new FormData();

  formData.append('subject', payload.subject?.trim() || '');
  formData.append('category', payload.category || 'other');
  formData.append('content', payload.content || '');

  if (payload.relatedOrderId) {
    formData.append('relatedOrderId', String(payload.relatedOrderId));
  }

  if (payload.relatedProductId) {
    formData.append('relatedProductId', String(payload.relatedProductId));
  }

  if (payload.stickerCode) {
    formData.append('stickerCode', payload.stickerCode);
  }

  (payload.images || []).forEach((file) => {
    formData.append('images', file);
  });

  return formData;
};

const buildMessageFormData = (payload = {}) => {
  const formData = new FormData();
  formData.append('content', payload.content || '');

  if (payload.stickerCode) {
    formData.append('stickerCode', payload.stickerCode);
  }

  (payload.images || []).forEach((file) => {
    formData.append('images', file);
  });

  return formData;
};

const normalizeAttachments = (attachments = []) =>
  attachments.map((attachment) => ({
    ...attachment,
    url: getMediaUrl(attachment.url),
  }));

const mapTicket = (ticket) => ({
  ...ticket,
  categoryLabel: ticket.categoryLabel || getCategoryLabel(ticket.category),
  statusMeta: getSupportStatusMeta(ticket.status),
  priorityMeta: getSupportPriorityMeta(ticket.priority),
  lastMessagePreview: ticket.lastMessagePreview || 'Chưa có nội dung trao đổi',
  unreadCount: Number(ticket.unreadCount || 0),
  messages: Array.isArray(ticket.messages)
    ? ticket.messages.map((message) => ({
        ...message,
        attachments: normalizeAttachments(message.attachments),
      }))
    : undefined,
});

const normalizePagedResult = (response) => ({
  items: Array.isArray(response?.items) ? response.items.map(mapTicket) : [],
  page: Number(response?.page || 1),
  pageSize: Number(response?.pageSize || 10),
  totalItems: Number(response?.totalItems || 0),
  totalPages: Math.max(Number(response?.totalPages || 1), 1),
  stats: {
    all: Number(response?.stats?.all || 0),
    new: Number(response?.stats?.new || 0),
    'in-progress': Number(response?.stats?.inProgress || 0),
    'waiting-customer': Number(response?.stats?.waitingCustomer || 0),
    resolved: Number(response?.stats?.resolved || 0),
    closed: Number(response?.stats?.closed || 0),
    unreadTickets: Number(response?.stats?.unreadTickets || 0),
  },
});

const supportService = {
  async getCustomerTickets({ keyword = '', status = 'all', category = '', page = 1, pageSize = 10 } = {}) {
    const response = await axiosClient.get('/support/tickets', {
      params: {
        keyword,
        status,
        category,
        page,
        pageSize,
        _t: Date.now(),
      },
    });

    return normalizePagedResult(response);
  },

  async getCustomerTicketDetail(ticketId) {
    const response = await axiosClient.get(`/support/tickets/${ticketId}`, {
      params: { _t: Date.now() },
    });

    return mapTicket(response);
  },

  async createTicket(payload) {
    const response = await axiosClient.post('/support/tickets', buildTicketFormData(payload), {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return mapTicket(response);
  },

  async sendCustomerMessage(ticketId, payload) {
    const response = await axiosClient.post(`/support/tickets/${ticketId}/messages`, buildMessageFormData(payload), {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return mapTicket(response);
  },

  async markCustomerTicketRead(ticketId) {
    const response = await axiosClient.post(`/support/tickets/${ticketId}/read`);
    return mapTicket(response);
  },

  async reopenCustomerTicket(ticketId) {
    const response = await axiosClient.post(`/support/tickets/${ticketId}/reopen`);
    return mapTicket(response);
  },

  async getSupportBadgeCount() {
    const response = await axiosClient.get('/support/tickets/unread-count', {
      params: { _t: Date.now() },
    });

    return Number(response?.count || 0);
  },
};

export default supportService;
