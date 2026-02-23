using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class MasterDataCombination
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ฝ่าย")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "แผนก")]
        public string SectionName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Plant")]
        public string PlantName { get; set; } = string.Empty;

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "วันที่อัพเดท")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "ผู้สร้าง")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "ผู้ที่อัพเดท")]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
