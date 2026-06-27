namespace CMS.Data.Enums
{
    public enum OrderStatus
    {
        PENDING = 0,     // Chờ duyệt
        CONFIRMED = 1,   // Đã duyệt
        PROCESSING = 2,  // Đang chuẩn bị hàng
        SHIPPING = 3,    // Đang giao hàng
        COMPLETED = 4,   // Hoàn thành
        CANCELLED = 5,   // Đã hủy
        AWAITING_CUSTOMER_CONFIRMATION = 6, // Chờ khách xác nhận
        WAITING_FOR_RESTOCK = 7,            // Chờ bổ sung hàng
        READY_TO_SHIP = 8,                  // Sẵn sàng giao
        REFUND_PENDING = 9,                 // Chờ hoàn tiền
        REFUNDED = 10                       // Đã hoàn tiền
    }
}
