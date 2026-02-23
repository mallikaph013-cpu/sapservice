using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class BomComponent
    {
        public int Id { get; set; }

        [Required]
        public int RequestItemId { get; set; }
        [ForeignKey("RequestItemId")]
        public RequestItem? RequestItem { get; set; }

        public int Level { get; set; }
        public string? Item { get; set; }
        public string? ItemCat { get; set; } 
        public string? ComponentNumber { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? ItemQuantity { get; set; }
        public string? Unit { get; set; }
        public string? BomUsage { get; set; }
        public string? Plant { get; set; }
        public string? Sloc { get; set; }
    }
}
