
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
// Removed duplicate using myapp.Models;

namespace myapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            var viewModel = new CreateUserViewModel
            {
                DepartmentList = new SelectList(_context.Departments.ToList(), "Name", "Name"), // Use Name for both value and text
                SectionList = new SelectList(Enumerable.Empty<SelectListItem>()),
                PlantList = new SelectList(Enumerable.Empty<SelectListItem>())
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.User.Username,
                    Email = model.User.Username,
                    FirstName = model.User.FirstName,
                    LastName = model.User.LastName,
                    Department = model.User.Department,
                    Section = model.User.Section,
                    Plant = model.User.Plant,
                    IsActive = model.User.IsActive,
                    IsIT = model.User.IsIT,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var role = model.User.IsIT ? "Admin" : "User";
                    await _userManager.AddToRoleAsync(user, role);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.DepartmentList = new SelectList(_context.Departments.ToList(), "Name", "Name", model.User.Department);
            return View(model);
        }

        [HttpGet]
        public JsonResult GetSectionsAndPlants(string department)
        {
            // Find the department ID from its name to correctly filter sections and plants
            var departmentEntity = _context.Departments.FirstOrDefault(d => d.Name == department);
            var sections = Enumerable.Empty<SelectListItem>();
            var plants = Enumerable.Empty<SelectListItem>();

            if (departmentEntity != null)
            {
                sections = _context.Sections
                                     .Where(s => s.DepartmentId == departmentEntity.Id)
                                     .Select(s => new SelectListItem { Value = s.Name, Text = s.Name })
                                     .ToList();

                plants = _context.Plants
                                   .Where(p => p.DepartmentId == departmentEntity.Id)
                                   .Select(p => new SelectListItem { Value = p.Name, Text = p.Name })
                                   .ToList();
            }

            return Json(new { sections, plants });
        }
    }
}
