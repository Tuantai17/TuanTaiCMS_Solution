using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class ProductFavorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
