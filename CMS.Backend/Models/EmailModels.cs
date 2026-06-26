namespace CMS.Backend.Models
{
    // Model du lieu cho email xac nhan don hang
    public class OrderEmailModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime OrderDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderEmailItemModel> Items { get; set; } = new();
    }

    public class OrderEmailItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Model du lieu cho email thanh toan thanh cong
    public class PaymentSuccessEmailModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Model du lieu cho email giao hang thanh cong
    public class DeliverySuccessEmailModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime? DeliveredDate { get; set; }
        public string? Address { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Model du lieu cho email quen mat khau
    public class ForgotPasswordEmailModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
    }
}
