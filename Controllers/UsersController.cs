using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using myapp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using myapp.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myapp.Controllers
{
    [Authorize(Roles = "IT")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName");
            ViewBag.Sections = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text"); // Start with empty list
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetSections(int departmentId)
        {
            var sections = await _context.Sections
                                       .Where(s => s.DepartmentId == departmentId)
                                       .OrderBy(s => s.SectionName)
                                       .Select(s => new { s.SectionId, s.SectionName })
                                       .ToListAsync();
            return Json(new SelectList(sections, "SectionId", "SectionName"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var department = await _context.Departments.FindAsync(model.DepartmentId);
                var section = await _context.Sections.FindAsync(model.SectionId);

                if (department == null || section == null || section.DepartmentId != department.DepartmentId)
                {
                    ModelState.AddModelError("", "Invalid Department or Section selection.");
                }
                else
                {
                    var user = new ApplicationUser 
                    {
                        UserName = model.Email, 
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Department = department.DepartmentName,
                        Section = section.SectionName,
                        IsIT = model.IsIT
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "User created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.DepartmentId == model.DepartmentId).OrderBy(s => s.SectionName).ToListAsync(), "SectionId", "SectionName", model.SectionId);
            return View(model);
        }

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

            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == user.Department);
            var section = await _context.Sections
                                        .Include(s => s.Department)
                                        .FirstOrDefaultAsync(s => s.SectionName == user.Section && s.Department != null && s.Department.DepartmentName == user.Department);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DepartmentId = department?.DepartmentId ?? 0,
                SectionId = section?.SectionId ?? 0,
                IsIT = user.IsIT
            };

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.DepartmentId == model.DepartmentId).OrderBy(s => s.SectionName).ToListAsync(), "SectionId", "SectionName", model.SectionId);
            return View(model);
        }

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
                if (user == null) {
                    return NotFound();
                }

                var department = await _context.Departments.FindAsync(model.DepartmentId);
                var section = await _context.Sections.FindAsync(model.SectionId);

                if (department == null || section == null || section.DepartmentId != department.DepartmentId)
                {
                     ModelState.AddModelError("", "Invalid Department or Section selection.");
                } 
                else 
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Department = department.DepartmentName;
                    user.Section = section.SectionName;
                    user.IsIT = model.IsIT;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                     foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.DepartmentId == model.DepartmentId).OrderBy(s => s.SectionName).ToListAsync(), "SectionId", "SectionName", model.SectionId);
            return View(model);
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null) {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(user);
            }
            return NotFound();
        }
    }
}
