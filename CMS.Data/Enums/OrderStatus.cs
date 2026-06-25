namespace CMS.Data.Enums
{
    public enum OrderStatus
    {
        PENDING = 0,     // Chờ duyệt
        CONFIRMED = 1,   // Đã duyệt
        PROCESSING = 2,  // Đang chuẩn bị hàng
        SHIPPING = 3,    // Đang giao hàng
        COMPLETED = 4,   // Hoàn thành
        CANCELLED = 5    // Đã hủy
    }
}
