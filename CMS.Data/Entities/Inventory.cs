using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int CurrentStock { get; set; } = 0;
        
        public int ReservedStock { get; set; } = 0;
        
        public int AlertThreshold { get; set; } = 10;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Helper property (Not mapped to DB)
        [NotMapped]
        public int AvailableStock => Math.Max(CurrentStock - ReservedStock, 0);
    }
}
