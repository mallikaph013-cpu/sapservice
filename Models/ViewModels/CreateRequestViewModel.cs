using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models.ViewModels
{
    public class CreateRequestViewModel
    {
        public int Id { get; set; }

        [Required]
        public RequestType RequestType { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        // User Details (Assuming these are pre-filled or from a user session)
        public string RequesterName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Plant { get; set; } = string.Empty;

        // FG, SM, RM Common Fields
        public string? PlantFG { get; set; }
        public string? ItemCode { get; set; }
        public string? EnglishMatDescription { get; set; }
        public string? ModelName { get; set; }
        public string? BaseUnit { get; set; }
        public string? MaterialGroup { get; set; }
        public string? ExternalMaterialGroup { get; set; }
        public string? DivisionCode { get; set; }
        public string? ProfitCenter { get; set; }
        public string? DistributionChannel { get; set; }
        public string? BoiCode { get; set; }
        public string? MrpController { get; set; }
        public string? StorageLocation { get; set; }
        public string? ProductionSupervisor { get; set; }
        public string? CostingLotSize { get; set; }
        public string? ValClass { get; set; }
        public string? StandardPack { get; set; }
        public string? BoiDescription { get; set; }
        public string? MakerMfrPartNumber { get; set; }
        public string? CommCodeTariffCode { get; set; }
        public string? TraffCodePercentage { get; set; }
        public string? StorageLocationB1 { get; set; }
        public string? PriceControl { get; set; }
        public string? Currency { get; set; }
        public string? SupplierCode { get; set; }
        public string? MatType { get; set; }
        public bool Check { get; set; }
        public string? DevicePlant { get; set; }
        public string? AssemblyPlant { get; set; }
        public string? IpoPlant { get; set; }
        public string? AsiOfPlant { get; set; }
        public string? PriceUnit { get; set; }
        public string? StorageLocationEP { get; set; }
        public string? ToolingBSection { get; set; }
        public string? PoNumber { get; set; }
        public string? StatusInA { get; set; }
        public string? DateIn { get; set; }
        public string? QuotationNumber { get; set; }
        public string? ToolingBModel { get; set; }
        public string? TariffCode { get; set; }
        public string? Planner { get; set; }
        public string? CurrentICS { get; set; }
        public string? Level { get; set; }
        public string? Rohs { get; set; }
        public string? CodenMid { get; set; }
        public string? SalesOrg { get; set; }
        public string? TaxTh { get; set; }
        public string? MaterialStatisticsGroup { get; set; }
        public string? AccountAssignment { get; set; }
        public string? GeneralItemCategory { get; set; }
        public string? Availability { get; set; } 
        public string? Transportation { get; set; }
        public string? LoadingGroup { get; set; }
        public string? PlanDelTime { get; set; }
        public string? SchedMargin { get; set; }
        public string? MinLot { get; set; }
        public string? MaxLot { get; set; }
        public string? FixedLot { get; set; }
        public string? Rounding { get; set; }
        public string? Mtlsm { get; set; }
        public string? Effective { get; set; }
        public string? StorageLoc { get; set; }
        public string? ReceiveStorage { get; set; }
        public string? PurchasingGroup { get; set; }
        public string? Price { get; set; }

        // For BOM and Routing
        public List<BomComponentViewModel> Components { get; set; } = new List<BomComponentViewModel>();
        public List<RoutingViewModel> Routings { get; set; } = new List<RoutingViewModel>();
    }
}