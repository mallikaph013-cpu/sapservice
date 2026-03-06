using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace myapp.Models
{
    public class Section
    {
        [Key]
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(256)]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
