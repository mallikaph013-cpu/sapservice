
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public enum DistributionChannel
    {
        [Display(Name = "10 = Domestic")]
        Domestic = 10,

        [Display(Name = "30 = Export")]
        Export = 30,

        [Display(Name = "20 = Indirect")]
        Indirect = 20,

        [Display(Name = "40 = Subcontractor")]
        Subcontractor = 40,

        [Display(Name = "90 = Other Sales")]
        OtherSales = 90
    }
}
