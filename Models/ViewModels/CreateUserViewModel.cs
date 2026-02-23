using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Required for [Required]
using Microsoft.AspNetCore.Mvc.Rendering;
using myapp.Models;

namespace myapp.Models.ViewModels
{
    public class CreateUserViewModel
    {
        public User User { get; set; } = new User();

        // Add a separate Password field for the form
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SectionList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PlantList { get; set; } = new List<SelectListItem>();
    }
}
