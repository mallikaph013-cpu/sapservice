using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class MasterDataCombination
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The Department field is required.")]
        [Display(Name = "Department")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Section field is required.")]
        [Display(Name = "Section")]
        public string SectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Plant field is required.")]
        [Display(Name = "Plant")]
        public string PlantName { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
