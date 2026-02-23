using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Routing
    {
        public int Id { get; set; }

        [Required]
        public int RequestItemId { get; set; }
        [ForeignKey("RequestItemId")]
        public RequestItem? RequestItem { get; set; }
        
        public string? Material { get; set; }
        public string? Description { get; set; }
        public string? WorkCenter { get; set; }
        public string? Operation { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? BaseQty { get; set; }
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? DirectLaborCosts { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? DirectExpenses { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? AllocationExpense { get; set; }
        public string? ProductionVersionCode { get; set; }
        public string? Version { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal? MaximumLotSize { get; set; }
        public string? Group { get; set; }
        public string? GroupCounter { get; set; }
    }
}
