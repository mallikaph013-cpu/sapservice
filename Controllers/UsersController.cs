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
using System.Collections.Generic;

namespace myapp.Controllers
{
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

        private async Task<CreateUserViewModel> PopulateUserViewModelAsync(CreateUserViewModel model)
        {
            var departments = await _context.Departments.Select(d => d.DepartmentName).Distinct().ToListAsync();
            model.DepartmentList = new SelectList(departments);

            if (!string.IsNullOrEmpty(model.Department))
            {
                var departmentEntity = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == model.Department);
                if (departmentEntity != null)
                {
                    var sections = await _context.Sections
                        .Where(s => s.DepartmentId == departmentEntity.DepartmentId)
                        .Select(s => s.SectionName)
                        .Distinct().ToListAsync();
                    model.SectionList = new SelectList(sections, model.Section);

                    var plants = await _context.Plants
                        .Where(p => p.DepartmentId == departmentEntity.DepartmentId)
                        .Select(p => p.PlantName)
                        .Distinct().ToListAsync();
                    model.PlantList = new SelectList(plants, model.Plant);
                }
            }
            else
            {
                model.SectionList = new SelectList(Enumerable.Empty<string>());
                model.PlantList = new SelectList(Enumerable.Empty<string>());
            }
            return model;
        }

        private async Task<EditUserViewModel> PopulateEditViewModelAsync(EditUserViewModel model)
        {
            var departments = await _context.Departments.Select(d => d.DepartmentName).Distinct().ToListAsync();
            model.DepartmentList = new SelectList(departments, model.Department);

            if (!string.IsNullOrEmpty(model.Department))
            {
                var departmentEntity = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == model.Department);
                if (departmentEntity != null)
                {
                    var sections = await _context.Sections
                        .Where(s => s.DepartmentId == departmentEntity.DepartmentId)
                        .Select(s => s.SectionName)
                        .Distinct().ToListAsync();
                    model.SectionList = new SelectList(sections, model.Section);

                    var plants = await _context.Plants
                        .Where(p => p.DepartmentId == departmentEntity.DepartmentId)
                        .Select(p => p.PlantName)
                        .Distinct().ToListAsync();
                    model.PlantList = new SelectList(plants, model.Plant);
                }
            }
             else
            {
                model.SectionList = new SelectList(Enumerable.Empty<string>());
                model.PlantList = new SelectList(Enumerable.Empty<string>());
            }
            return model;
        }


        public async Task<IActionResult> Create()
        {
            var viewModel = await PopulateUserViewModelAsync(new CreateUserViewModel());
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
                    UserName = model.Username, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Department = model.Department,
                    Section = model.Section,
                    Plant = model.Plant,
                    IsActive = model.IsActive,
                    IsIT = model.IsIT,
                    EmailConfirmed = true 
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var role = model.IsIT ? "Admin" : "User";
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRoleAsync(user, role);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                         ModelState.AddModelError("Username", $"Username '{model.Username}' is already taken.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            await PopulateUserViewModelAsync(model);
            return View(model);
        }

        [HttpGet]
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

            var userRoles = await _userManager.GetRolesAsync(user);

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty, 
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Department = user.Department ?? string.Empty,
                Section = user.Section ?? string.Empty,
                Plant = user.Plant ?? string.Empty,
                IsActive = user.IsActive,
                IsIT = userRoles.Contains("Admin")
            };

            await PopulateEditViewModelAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEditViewModelAsync(model);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            var existingUserWithSameName = await _userManager.FindByNameAsync(model.Username);
            if (existingUserWithSameName != null && existingUserWithSameName.Id != user.Id)
            {
                ModelState.AddModelError("Username", $"Username '{model.Username}' is already taken.");
                await PopulateEditViewModelAsync(model);
                return View(model);
            }

            user.UserName = model.Username;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Department = model.Department;
            user.Section = model.Section;
            user.Plant = model.Plant;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var targetRole = model.IsIT ? "Admin" : "User";

                if (!currentRoles.Contains(targetRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!await _roleManager.RoleExistsAsync(targetRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(targetRole));
                    }
                    await _userManager.AddToRoleAsync(user, targetRole);
                }
                
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                 if (error.Code == "DuplicateUserName")
                {
                    ModelState.AddModelError("Username", $"Username '{model.Username}' is already taken.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await PopulateEditViewModelAsync(model);
            return View(model);
        }


        [HttpGet]
        public async Task<JsonResult> GetSectionsAndPlants(string department)
        {
            var sections = new List<SelectListItem>();
            var plants = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(department))
            {
                var departmentEntity = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == department);
                if (departmentEntity != null)
                {
                    sections = await _context.Sections
                                         .Where(s => s.DepartmentId == departmentEntity.DepartmentId)
                                         .Select(s => new SelectListItem { Value = s.SectionName, Text = s.SectionName })
                                         .ToListAsync();

                    plants = await _context.Plants
                                       .Where(p => p.DepartmentId == departmentEntity.DepartmentId)
                                       .Select(p => new SelectListItem { Value = p.PlantName, Text = p.PlantName })
                                       .ToListAsync();
                }
            }
            return Json(new { sections, plants });
        }
    }
}
