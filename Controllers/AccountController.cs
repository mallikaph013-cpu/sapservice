using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using myapp.Models;
using myapp.Data;
using System.Threading.Tasks;
using System;

namespace myapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        await AddAuditLogAsync(
                            action: "LoginInactive",
                            performedBy: user.UserName,
                            details: "Login blocked because account is inactive.");

                        ModelState.AddModelError(string.Empty, "User account is inactive.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        if (user.MustChangePasswordOnFirstLogin)
                        {
                            await AddAuditLogAsync(
                                action: "LoginFirstTimePasswordChangeRequired",
                                performedBy: user.UserName,
                                details: "User login blocked until first password change is completed.");

                            return RedirectToAction(nameof(ChangePasswordFirstLogin));
                        }

                        await AddAuditLogAsync(
                            action: "LoginSuccess",
                            performedBy: user.UserName,
                            details: $"RememberMe={model.RememberMe}");

                        return RedirectToAction("Index", "Home");
                    }
                }

                await AddAuditLogAsync(
                    action: "LoginFailed",
                    performedBy: model.UserName,
                    details: "Invalid username or password.");

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePasswordFirstLogin()
        {
            return View(new FirstLoginChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordFirstLogin(FirstLoginChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                user.MustChangePasswordOnFirstLogin = false;
                user.UpdatedBy = user.UserName;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);

                await AddAuditLogAsync(
                    action: "PasswordChangedFirstLogin",
                    performedBy: user.UserName,
                    details: "User completed mandatory first-login password change.");

                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var currentUserName = User?.Identity?.Name;

            await _signInManager.SignOutAsync();

            await AddAuditLogAsync(
                action: "Logout",
                performedBy: currentUserName,
                details: "User signed out.");

            return RedirectToAction("Index", "Home");
        }

        private async Task AddAuditLogAsync(string action, string? performedBy, string? details)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Authentication",
                    EntityId = null,
                    Action = action,
                    PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "Unknown" : performedBy,
                    PerformedAt = DateTime.UtcNow,
                    Details = details
                });

                await _context.SaveChangesAsync();
            }
            catch
            {
                // Keep login flow stable even if audit logging fails.
            }
        }
    }
}
