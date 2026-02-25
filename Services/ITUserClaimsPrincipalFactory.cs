using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using myapp.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myapp.Services
{
    public class ITUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ITUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor) 
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            if (user.IsIT)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "IT"));
            }
            return identity;
        }
    }
}
