using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace myapp.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        // Navigation properties for related Sections and Plants
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
        public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
    }
}
