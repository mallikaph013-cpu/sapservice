using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using myapp.Data;

namespace myapp.Controllers
{
    [Authorize]
    public class MasterDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MasterDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new MasterDataViewModel
            {
                MasterDataCombinations = await _context.MasterDataCombinations.ToListAsync(),
                DepartmentList = await _context.MasterDataCombinations
                                               .Select(m => m.DepartmentName)
                                               .Distinct()
                                               .Select(d => new SelectListItem { Text = d, Value = d })
                                               .ToListAsync(),
                SectionList = await _context.MasterDataCombinations
                                            .Select(m => m.SectionName)
                                            .Distinct()
                                            .Select(s => new SelectListItem { Text = s, Value = s })
                                            .ToListAsync(),
                PlantList = await _context.MasterDataCombinations
                                          .Select(m => m.PlantName)
                                          .Distinct()
                                          .Select(p => new SelectListItem { Text = p, Value = p })
                                          .ToListAsync(),
                NewCombination = new MasterDataCombination()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMasterData(MasterDataViewModel viewModel)
        {
            // Check if the viewModel or NewCombination is null, which is good practice.
            if (viewModel?.NewCombination == null)
            {
                // Optionally handle this case, maybe return a bad request
                return BadRequest("Invalid master data submitted.");
            }

            // Since the form only submits NewCombination, we don't need to check the full model state
            // if we are only concerned with the new item. A more robust way is to use a specific model for creation.
            if (string.IsNullOrEmpty(viewModel.NewCombination.DepartmentName) ||
                string.IsNullOrEmpty(viewModel.NewCombination.SectionName) ||
                string.IsNullOrEmpty(viewModel.NewCombination.PlantName))
            {
                 ModelState.AddModelError("NewCombination", "Department, Section, and Plant names are required.");
            }

            if (ModelState.IsValid)
            {
                var combinationExists = await _context.MasterDataCombinations.AnyAsync(m =>
                    m.DepartmentName == viewModel.NewCombination.DepartmentName &&
                    m.SectionName == viewModel.NewCombination.SectionName &&
                    m.PlantName == viewModel.NewCombination.PlantName);

                if (combinationExists)
                {
                    ModelState.AddModelError("", "This combination already exists.");
                }
                else
                {
                    var username = User.Identity?.Name ?? "System"; // Null check

                    viewModel.NewCombination.CreatedAt = DateTime.UtcNow;
                    viewModel.NewCombination.UpdatedAt = DateTime.UtcNow;
                    viewModel.NewCombination.CreatedBy = username;
                    viewModel.NewCombination.UpdatedBy = username;

                    _context.MasterDataCombinations.Add(viewModel.NewCombination);
                    await _context.SaveChangesAsync(true);

                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got here, something went wrong. Re-populate the necessary data for the view.
            viewModel.MasterDataCombinations = await _context.MasterDataCombinations.ToListAsync();
            viewModel.DepartmentList = await _context.MasterDataCombinations.Select(m => m.DepartmentName).Distinct().Select(d => new SelectListItem { Text = d, Value = d }).ToListAsync();
            viewModel.SectionList = await _context.MasterDataCombinations.Select(m => m.SectionName).Distinct().Select(s => new SelectListItem { Text = s, Value = s }).ToListAsync();
            viewModel.PlantList = await _context.MasterDataCombinations.Select(m => m.PlantName).Distinct().Select(p => new SelectListItem { Text = p, Value = p }).ToListAsync();
            return View("Index", viewModel);
        }
    }
}
