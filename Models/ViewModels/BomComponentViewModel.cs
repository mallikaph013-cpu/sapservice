namespace myapp.Models.ViewModels
{
    public class BomComponentViewModel
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string? Item { get; set; }
        public string? ItemCat { get; set; } 
        public string? ComponentNumber { get; set; }
        public string? Description { get; set; }
        public decimal? ItemQuantity { get; set; }
        public string? Unit { get; set; }
        public string? BomUsage { get; set; }
        public string? Plant { get; set; }
        public string? Sloc { get; set; }
    }
}
