using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class CustomerAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProvinceName { get; set; }

        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        [MaxLength(100)]
        public string WardName { get; set; }

        [Required]
        [MaxLength(255)]
        public string AddressLine { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; } // Nhà riêng, Văn phòng, Khác...

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
