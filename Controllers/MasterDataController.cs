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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using ClosedXML.Excel;
using System.Text;

namespace myapp.Controllers
{
    [Authorize]
    public class MasterDataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MasterDataController> _logger;

        public MasterDataController(ApplicationDbContext context, ILogger<MasterDataController> logger)
        {
            _context = context;
            _logger = logger;
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
            var actor = User.Identity?.Name ?? "System";
            var requestedDepartment = viewModel.NewCombination?.DepartmentName;
            var requestedSection = viewModel.NewCombination?.SectionName;
            var requestedPlant = viewModel.NewCombination?.PlantName;

            _logger.LogInformation(
                "CreateMasterData started by {Actor}. Department={Department}, Section={Section}, Plant={Plant}",
                actor,
                requestedDepartment,
                requestedSection,
                requestedPlant);

            if (!ModelState.IsValid)
            {
                var errorCount = ModelState.Values.Sum(v => v.Errors.Count);
                _logger.LogWarning(
                    "CreateMasterData validation failed by {Actor}. ErrorCount={ErrorCount}, Department={Department}, Section={Section}, Plant={Plant}",
                    actor,
                    errorCount,
                    requestedDepartment,
                    requestedSection,
                    requestedPlant);
            }

            if (ModelState.IsValid)
            {
                var combination = viewModel.NewCombination;
                var createdDepartment = false;
                var createdSection = false;
                var createdPlant = false;

                var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == combination.DepartmentName);
                if (department == null)
                {
                    department = new Department { DepartmentName = combination.DepartmentName };
                    _context.Departments.Add(department);
                    createdDepartment = true;
                }

                var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == combination.SectionName);
                if (section == null)
                {
                    section = new Section { SectionName = combination.SectionName, Department = department };
                    _context.Sections.Add(section);
                    createdSection = true;
                }

                var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantName == combination.PlantName);
                if (plant == null)
                {
                    plant = new Plant { PlantName = combination.PlantName, Department = department };
                    _context.Plants.Add(plant);
                    createdPlant = true;
                }

                combination.CreatedAt = DateTime.UtcNow;
                combination.UpdatedAt = DateTime.UtcNow;
                combination.CreatedBy = User.Identity?.Name ?? "System";
                combination.UpdatedBy = User.Identity?.Name ?? "System";

                _context.MasterDataCombinations.Add(combination);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "CreateMasterData succeeded. CombinationId={CombinationId}, Actor={Actor}, Department={Department}, Section={Section}, Plant={Plant}, CreatedDepartment={CreatedDepartment}, CreatedSection={CreatedSection}, CreatedPlant={CreatedPlant}",
                    combination.Id,
                    actor,
                    combination.DepartmentName,
                    combination.SectionName,
                    combination.PlantName,
                    createdDepartment,
                    createdSection,
                    createdPlant);

                TempData["SuccessMessage"] = "Master data combination created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var populatedViewModel = await PopulateViewModelAsync(viewModel);
            return View("Index", populatedViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportMasterData(IFormFile? importFile)
        {
            var actor = User.Identity?.Name ?? "System";

            if (importFile == null || importFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to import.";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(importFile.FileName).ToLowerInvariant();
            if (extension != ".csv" && extension != ".xlsx")
            {
                TempData["ErrorMessage"] = "Unsupported file type. Please upload .csv or .xlsx file.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("ImportMasterData started by {Actor}. FileName={FileName}, Size={Size}", actor, importFile.FileName, importFile.Length);

            var importedCount = 0;
            var skippedCount = 0;
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (extension == ".csv")
                {
                    using var reader = new StreamReader(importFile.OpenReadStream());
                    var rowNumber = 0;

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        rowNumber++;

                        if (rowNumber == 1 && line != null && line.Contains("Department", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        var parts = line.Split(',');
                        if (parts.Length < 3)
                        {
                            skippedCount++;
                            continue;
                        }

                        var departmentName = parts[0].Trim();
                        var sectionName = parts[1].Trim();
                        var plantName = parts[2].Trim();

                        var wasImported = await TryImportCombinationAsync(departmentName, sectionName, plantName, actor, processedKeys);
                        if (wasImported) importedCount++; else skippedCount++;
                    }
                }
                else
                {
                    using var stream = importFile.OpenReadStream();
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "The uploaded Excel file does not contain any worksheet.";
                        return RedirectToAction(nameof(Index));
                    }

                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                    for (var row = 2; row <= lastRow; row++)
                    {
                        var departmentName = worksheet.Cell(row, 1).GetString().Trim();
                        var sectionName = worksheet.Cell(row, 2).GetString().Trim();
                        var plantName = worksheet.Cell(row, 3).GetString().Trim();

                        var wasImported = await TryImportCombinationAsync(departmentName, sectionName, plantName, actor, processedKeys);
                        if (wasImported) importedCount++; else skippedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "ImportMasterData completed by {Actor}. Imported={ImportedCount}, Skipped={SkippedCount}, FileName={FileName}",
                    actor,
                    importedCount,
                    skippedCount,
                    importFile.FileName);

                TempData["SuccessMessage"] = $"Import completed. Imported: {importedCount}, Skipped: {skippedCount}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportMasterData failed for {Actor}. FileName={FileName}", actor, importFile.FileName);
                TempData["ErrorMessage"] = "Import failed. Please verify the file format and data.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadImportTemplate()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Department,Section,Plant");
            csvBuilder.AppendLine("Production,Assembly,6031");
            csvBuilder.AppendLine("Production,Injection,6051");

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", "master-data-import-template.csv");
        }

        private async Task<bool> TryImportCombinationAsync(
            string departmentName,
            string sectionName,
            string plantName,
            string actor,
            HashSet<string> processedKeys)
        {
            if (string.IsNullOrWhiteSpace(departmentName) || string.IsNullOrWhiteSpace(sectionName) || string.IsNullOrWhiteSpace(plantName))
            {
                return false;
            }

            var key = $"{departmentName}|{sectionName}|{plantName}";
            if (!processedKeys.Add(key))
            {
                return false;
            }

            var exists = await _context.MasterDataCombinations.AnyAsync(c =>
                c.DepartmentName == departmentName &&
                c.SectionName == sectionName &&
                c.PlantName == plantName);

            if (exists)
            {
                return false;
            }

            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == departmentName);
            if (department == null)
            {
                department = new Department { DepartmentName = departmentName };
                _context.Departments.Add(department);
            }

            var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == sectionName);
            if (section == null)
            {
                section = new Section { SectionName = sectionName, Department = department };
                _context.Sections.Add(section);
            }

            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantName == plantName);
            if (plant == null)
            {
                plant = new Plant { PlantName = plantName, Department = department };
                _context.Plants.Add(plant);
            }

            var now = DateTime.UtcNow;
            var combination = new MasterDataCombination
            {
                DepartmentName = departmentName,
                SectionName = sectionName,
                PlantName = plantName,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = actor,
                UpdatedBy = actor
            };

            _context.MasterDataCombinations.Add(combination);
            return true;
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
            if (combination != null)
            {
                _context.MasterDataCombinations.Remove(combination);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Master data combination deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Master data combination not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
