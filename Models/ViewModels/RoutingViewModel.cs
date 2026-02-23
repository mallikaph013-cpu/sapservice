using System;

namespace myapp.Models.ViewModels
{
    public class RoutingViewModel
    {
        public int Id { get; set; }
        public string? Material { get; set; }
        public string? Description { get; set; }
        public string? WorkCenter { get; set; }
        public string? Operation { get; set; }
        public decimal? BaseQty { get; set; }
        public string? Unit { get; set; }
        public decimal? DirectLaborCosts { get; set; }
        public decimal? DirectExpenses { get; set; }
        public decimal? AllocationExpense { get; set; }
        public string? ProductionVersionCode { get; set; }
        public string? Version { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal? MaximumLotSize { get; set; }
        public string? Group { get; set; }
        public string? GroupCounter { get; set; }
    }
}
