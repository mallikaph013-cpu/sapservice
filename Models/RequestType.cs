using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public enum RequestType
    {
        FG,
        SM,
        RM,
        [Display(Name = "Tooling B")]
        ToolingB,
        [Display(Name = "Tooling B_FG")]
        ToolingB_FG,
        [Display(Name = "Tooling B_PU")]
        ToolingB_PU,
        BOM,
        Routing,
        [Display(Name = "Add Storage")]
        AddStorage,
        [Display(Name = "Distribution Chanel")]
        DistributionChanel,
        IPO
    }
}
