using Microsoft.AspNetCore.Identity;
using System;

namespace myapp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? Section { get; set; }
        public string? Plant { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsIT { get; set; } = false;

        // Audit Fields from the old User model
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
