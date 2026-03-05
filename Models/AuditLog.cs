using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [StringLength(256)]
        public string? PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; }
    }
}
