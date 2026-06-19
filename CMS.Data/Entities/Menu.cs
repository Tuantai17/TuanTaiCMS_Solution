using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public int? ParentId { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public int Depth { get; set; }

        // Tránh vòng lặp tuần tự hóa JSON (Reference Loop Exception) khi API trả dữ liệu cây Menu về Frontend
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; }
    }
}
