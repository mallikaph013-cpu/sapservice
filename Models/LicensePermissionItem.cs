using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class LicensePermissionItem
    {
        public int Id { get; set; }

        [Required]
        public int RequestItemId { get; set; }

        [ForeignKey("RequestItemId")]
        public RequestItem? RequestItem { get; set; }

        public string? SapUsername { get; set; }
        public string? TCode { get; set; }
    }
}