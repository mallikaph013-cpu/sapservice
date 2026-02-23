using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class User
    {
        public long Id { get; set; }

        // Foreign Key to the AspNetUsers table (IdentityUser)
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        // PasswordHash has been removed. It will be handled by ASP.NET Core Identity.

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Department { get; set; }
        public string? Section { get; set; }
        public string? Plant { get; set; }

        public bool IsActive { get; set; }

        public bool IsIT { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
