namespace myapp.Models.ViewModels
{
    public class CreateDocumentRoutingViewModel
    {
        // Existing properties for dropdowns
        public int SelectedDocumentTypeId { get; set; }
        public int SelectedDepartmentId { get; set; }
        public int SelectedSectionId { get; set; }
        public string NewDocumentTypeName { get; set; } = string.Empty;
        public bool IsNewDocumentType { get; set; }

        // New properties for Plant
        public int SelectedPlantId { get; set; }
        public bool IsNewPlant { get; set; }
        public string NewPlantName { get; set; } = string.Empty;

        // New properties for Department and Section
        public bool IsNewDepartment { get; set; }
        public string NewDepartmentName { get; set; } = string.Empty;
        public bool IsNewSection { get; set; }
        public string NewSectionName { get; set; } = string.Empty;

        // Added back properties
        public int DepartmentId { get; set; }
        public int Step { get; set; }
    }
}
