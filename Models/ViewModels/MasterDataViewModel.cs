using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using myapp.Models;

namespace myapp.Models.ViewModels
{
    public class MasterDataViewModel
    {
        public MasterDataCombination NewCombination { get; set; } = new MasterDataCombination();
        public IEnumerable<MasterDataCombination> MasterDataCombinations { get; set; } = new List<MasterDataCombination>();
        public SelectList DepartmentList { get; set; } = new SelectList(new List<string>());
        public SelectList SectionList { get; set; } = new SelectList(new List<string>());
        public SelectList PlantList { get; set; } = new SelectList(new List<string>());
    }
}
