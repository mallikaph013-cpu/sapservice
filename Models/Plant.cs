using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Plant
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Plant Name")]
        public string Name { get; set; } = string.Empty;

        // Foreign key for Department
        [Required]
        public int DepartmentId { get; set; }

        // Navigation property
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }
}
