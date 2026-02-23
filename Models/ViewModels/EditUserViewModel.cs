using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace myapp.Models.ViewModels
{
    public class EditUserViewModel
    {
        public User User { get; set; } = new User();
        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SectionList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PlantList { get; set; } = new List<SelectListItem>();
    }
}
