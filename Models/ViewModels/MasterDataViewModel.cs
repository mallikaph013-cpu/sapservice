using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myapp.Models.ViewModels
{
    public class MasterDataViewModel
    {
        public IEnumerable<MasterDataCombination> MasterDataCombinations { get; set; } = new List<MasterDataCombination>();
        public MasterDataCombination NewCombination { get; set; } = new MasterDataCombination();
        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SectionList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PlantList { get; set; } = new List<SelectListItem>();
    }
}
