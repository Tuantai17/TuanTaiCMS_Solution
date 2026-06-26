using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        public int InventoryId { get; set; }
        
        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }

        [MaxLength(100)]
        public string? TransactionCode { get; set; }

        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty; // IMPORT, EXPORT, RESERVE, RELEASE, ORDER_COMPLETED, ADJUSTMENT_IN, ADJUSTMENT_OUT, THRESHOLD_CHANGE

        public int QuantityChange { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public int ReservedBefore { get; set; }
        public int ReservedAfter { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } = 0;

        public int? ReferenceId { get; set; } // e.g. OrderId
        
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        [MaxLength(255)]
        public string? Reason { get; set; }

        public string? Note { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
