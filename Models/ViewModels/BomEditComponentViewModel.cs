namespace myapp.Models.ViewModels
{
    public class BomEditComponentViewModel
    {
        public int Id { get; set; }
        public string? ItemCodeFrom { get; set; }
        public string? DescriptionFrom { get; set; }
        public decimal? ItemQuantityFrom { get; set; }
        public string? UnitFrom { get; set; }
        public string? BomUsageFrom { get; set; }
        public string? SlocFrom { get; set; }

        // ----- TO -----
        public string? ItemCodeTo { get; set; }
        public string? DescriptionTo { get; set; }
        public decimal? ItemQuantityTo { get; set; }
        public string? UnitTo { get; set; }
        public string? BomUsageTo { get; set; }
        public string? SlocTo { get; set; }
        public string? PlantTo { get; set; }
    }
}

