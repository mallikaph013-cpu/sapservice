using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

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

        private async Task<MasterDataViewModel> PopulateViewModelAsync(MasterDataViewModel? viewModel = null)
        {
            viewModel ??= new MasterDataViewModel();

            var departments = await _context.Departments.Select(d => d.DepartmentName).Distinct().ToListAsync();
            var sections = await _context.Sections.Select(s => s.SectionName).Distinct().ToListAsync();
            var plants = await _context.Plants.Select(p => p.PlantName).Distinct().ToListAsync();

            viewModel.MasterDataCombinations = await _context.MasterDataCombinations.OrderByDescending(c => c.CreatedAt).ToListAsync();
            viewModel.DepartmentList = new SelectList(departments);
            viewModel.SectionList = new SelectList(sections);
            viewModel.PlantList = new SelectList(plants);

            return viewModel;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await PopulateViewModelAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMasterData(MasterDataViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var combination = viewModel.NewCombination;

                var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == combination.DepartmentName);
                if (department == null)
                {
                    department = new Department { DepartmentName = combination.DepartmentName };
                    _context.Departments.Add(department);
                }

                var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == combination.SectionName);
                if (section == null)
                {
                    section = new Section { SectionName = combination.SectionName, Department = department };
                    _context.Sections.Add(section);
                }

                var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantName == combination.PlantName);
                if (plant == null)
                {
                    plant = new Plant { PlantName = combination.PlantName, Department = department };
                    _context.Plants.Add(plant);
                }

                combination.CreatedAt = DateTime.UtcNow;
                combination.UpdatedAt = DateTime.UtcNow;
                combination.CreatedBy = User.Identity?.Name ?? "System";
                combination.UpdatedBy = User.Identity?.Name ?? "System";

                _context.MasterDataCombinations.Add(combination);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Master data combination created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var populatedViewModel = await PopulateViewModelAsync(viewModel);
            return View("Index", populatedViewModel);
        }

        // GET: MasterData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combination = await _context.MasterDataCombinations.FindAsync(id);
            if (combination == null)
            {
                return NotFound();
            }
            return View(combination);
        }

        // POST: MasterData/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MasterDataCombination combination)
        {
            if (id != combination.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    combination.UpdatedAt = DateTime.UtcNow;
                    combination.UpdatedBy = User.Identity?.Name ?? "System";
                    _context.Update(combination);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MasterDataCombinations.Any(e => e.Id == combination.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(combination);
        }

        // GET: MasterData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combination = await _context.MasterDataCombinations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (combination == null)
            {
                return NotFound();
            }

            return View(combination);
        }

        // POST: MasterData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combination = await _context.MasterDataCombinations.FindAsync(id);
            _context.MasterDataCombinations.Remove(combination);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
