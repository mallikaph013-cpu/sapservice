using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myapp.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = string.Empty;

        [Required]
        public string Plant { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsIT { get; set; }

        public SelectList DepartmentList { get; set; } = new SelectList(new List<string>());
        public SelectList SectionList { get; set; } = new SelectList(new List<string>());
        public SelectList PlantList { get; set; } = new SelectList(new List<string>());
    }
}
