using myapp.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myapp.Models.ViewModels
{
    public class DocumentRoutingViewModel
    {
        public IEnumerable<DocumentRouting> DocumentRoutings { get; set; } = new List<DocumentRouting>();
        public CreateDocumentRoutingViewModel CreateForm { get; set; } = new CreateDocumentRoutingViewModel();

        public SelectList? DocumentTypeList { get; set; }
        public SelectList? DepartmentList { get; set; }
        public SelectList? SectionList { get; set; }
        public SelectList? PlantList { get; set; }
    }
}
