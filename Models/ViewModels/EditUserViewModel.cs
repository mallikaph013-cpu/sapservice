using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Section { get; set; } = string.Empty;

        public string Plant { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsIT { get; set; }

        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SectionList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PlantList { get; set; } = new List<SelectListItem>();
    }
}
