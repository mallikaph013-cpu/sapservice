using Microsoft.AspNetCore.Identity;

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
    }
}
