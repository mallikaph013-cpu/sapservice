using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using myapp.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myapp.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateUserViewModel
            {
                DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "DepartmentName", "DepartmentName"),
                SectionList = new SelectList(await _context.Sections.ToListAsync(), "SectionName", "SectionName"),
                PlantList = new SelectList(await _context.Plants.ToListAsync(), "PlantName", "PlantName")
            };
            return View(viewModel);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Department = model.Department,
                    Section = model.Section,
                    Plant = model.Plant,
                    IsActive = model.IsActive,
                    IsIT = model.IsIT,
                    CreatedBy = User.Identity.Name, // Set the creator
                    UpdatedBy = User.Identity.Name, // Set the updater
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "DepartmentName", "DepartmentName");
            model.SectionList = new SelectList(await _context.Sections.ToListAsync(), "SectionName", "SectionName");
            model.PlantList = new SelectList(await _context.Plants.ToListAsync(), "PlantName", "PlantName");
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Department = user.Department ?? string.Empty,
                Section = user.Section ?? string.Empty,
                Plant = user.Plant ?? string.Empty,
                IsActive = user.IsActive,
                IsIT = user.IsIT,
                DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "DepartmentName", "DepartmentName"),
                SectionList = new SelectList(await _context.Sections.ToListAsync(), "SectionName", "SectionName"),
                PlantList = new SelectList(await _context.Plants.ToListAsync(), "PlantName", "PlantName")
            };
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Department = model.Department;
                user.Section = model.Section;
                user.Plant = model.Plant;
                user.IsActive = model.IsActive;
                user.IsIT = model.IsIT;
                user.UpdatedBy = User.Identity.Name; // Set the updater
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "DepartmentName", "DepartmentName");
            model.SectionList = new SelectList(await _context.Sections.ToListAsync(), "SectionName", "SectionName");
            model.PlantList = new SelectList(await _context.Plants.ToListAsync(), "PlantName", "PlantName");
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsAndPlants(string department)
        {
            var sections = await _context.Sections
                .Where(s => s.Department != null && s.Department.DepartmentName == department)
                .Select(s => new { value = s.SectionName, text = s.SectionName })
                .ToListAsync();

            var plants = await _context.Plants
                .Where(p => p.Department != null && p.Department.DepartmentName == department)
                .Select(p => new { value = p.PlantName, text = p.PlantName })
                .ToListAsync();

            return Json(new { sections, plants });
        }
    }
}
