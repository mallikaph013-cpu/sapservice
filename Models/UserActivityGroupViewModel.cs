using System;
using System.Collections.Generic;

namespace myapp.Models
{
    public class AuditActivityItemViewModel
    {
        public DateTime PerformedAtUtc { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public class UserActivityGroupViewModel
    {
        public string UserName { get; set; } = "Unknown";
        public int TotalActions { get; set; }
        public DateTime? LastActionAtUtc { get; set; }
        public List<AuditActivityItemViewModel> Activities { get; set; } = new List<AuditActivityItemViewModel>();
    }
}
