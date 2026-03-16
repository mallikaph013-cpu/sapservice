using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace myapp.Controllers
{
    [Authorize(Roles = "IT")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(ApplicationDbContext context, ILogger<DepartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync());
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId,DepartmentName")] Department department)
        {
            var normalizedDepartmentName = department.DepartmentName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedDepartmentName))
            {
                ModelState.AddModelError(nameof(department.DepartmentName), "Department name is required.");
            }

            var isDuplicate = await _context.Departments
                .AnyAsync(d => d.DepartmentName.ToLower() == normalizedDepartmentName.ToLower());
            if (isDuplicate)
            {
                ModelState.AddModelError(nameof(department.DepartmentName), "Department name already exists.");
            }

            if (ModelState.IsValid)
            {
                var actor = User.Identity?.Name ?? "System";
                var now = DateTime.UtcNow;
                department.DepartmentName = normalizedDepartmentName;
                department.IsActive = true;
                department.CreatedAt = now;
                department.UpdatedAt = now;
                department.CreatedBy = actor;
                department.UpdatedBy = actor;

                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentName,IsActive")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Departments.FindAsync(id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.DepartmentName = department.DepartmentName;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Department updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
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
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            department.IsActive = !department.IsActive;
            department.UpdatedAt = DateTime.UtcNow;
            department.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = department.IsActive
                ? "Department has been activated."
                : "Department has been deactivated.";

            return RedirectToAction(nameof(Edit), new { id = department.DepartmentId });
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                var hasSections = await _context.Sections.AnyAsync(s => s.DepartmentId == id);
                var hasPlants = await _context.Plants.AnyAsync(p => p.DepartmentId == id);
                var hasDocumentRoutings = await _context.DocumentRoutings.AnyAsync(dr => dr.DepartmentId == id);

                if (hasSections || hasPlants || hasDocumentRoutings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this department because it is referenced by sections, plants, or document routings.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Department deleted successfully!";
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqliteException)
                {
                    TempData["ErrorMessage"] = "Cannot delete this department because it is being used by related data.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Department not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? importFile)
        {
            var actor = User.Identity?.Name ?? "System";
            var startedAt = DateTime.UtcNow;

            if (importFile == null || importFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a CSV file.";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(importFile.FileName).ToLowerInvariant();
            if (extension != ".csv" && extension != ".xlsx")
            {
                TempData["ErrorMessage"] = "Only .csv and .xlsx files are supported.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation(
                "Departments import started by {Actor}. File={FileName}, Size={Size}, StartedAt={StartedAt}",
                actor,
                importFile.FileName,
                importFile.Length,
                startedAt);

            var imported = 0;
            var skipped = 0;
            var reports = new List<string>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                IEnumerable<(int RowNumber, string DepartmentName)> rows;
                if (extension == ".csv")
                {
                    rows = await ReadDepartmentRowsFromCsvAsync(importFile);
                }
                else
                {
                    rows = ReadDepartmentRowsFromXlsx(importFile);
                }

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.DepartmentName))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: DepartmentName is required.");
                        continue;
                    }

                    if (!seenNames.Add(row.DepartmentName))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Duplicate department '{row.DepartmentName}' in file.");
                        continue;
                    }

                    var exists = await _context.Departments.AnyAsync(d => d.DepartmentName == row.DepartmentName);
                    if (exists)
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Department '{row.DepartmentName}' already exists.");
                        continue;
                    }

                    var now = DateTime.UtcNow;
                    _context.Departments.Add(new Department
                    {
                        DepartmentName = row.DepartmentName,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = actor,
                        UpdatedBy = actor
                    });
                    imported++;
                }

                await _context.SaveChangesAsync();
                var finishedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Departments import completed by {Actor}. File={FileName}, StartedAt={StartedAt}, FinishedAt={FinishedAt}, Imported={Imported}, Skipped={Skipped}",
                    actor,
                    importFile.FileName,
                    startedAt,
                    finishedAt,
                    imported,
                    skipped);

                foreach (var report in reports.Take(20))
                {
                    _logger.LogWarning("Departments import row issue: {Issue}", report);
                }

                TempData["SuccessMessage"] = $"Department import completed. Imported: {imported}, Skipped: {skipped}";
                if (reports.Count > 0)
                {
                    TempData["ImportReport"] = string.Join("\n", reports.Take(50));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Departments import failed by {Actor}. File={FileName}", actor, importFile.FileName);
                TempData["ErrorMessage"] = "Department import failed. Please verify template/data.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadImportTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");
            worksheet.Cell(1, 1).Value = "DepartmentName";
            worksheet.Cell(2, 1).Value = "Production";
            worksheet.Cell(3, 1).Value = "Quality";
            try
            {
                worksheet.Columns().AdjustToContents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-size Departments import template columns. Returning default column widths.");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "departments-template.xlsx");
        }

        private static async Task<IEnumerable<(int RowNumber, string DepartmentName)>> ReadDepartmentRowsFromCsvAsync(IFormFile importFile)
        {
            var result = new List<(int RowNumber, string DepartmentName)>();
            using var reader = new StreamReader(importFile.OpenReadStream());
            var rowNumber = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (rowNumber == 1 && line.Contains("Department", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var departmentName = line.Split(',')[0].Trim();
                result.Add((rowNumber, departmentName));
            }

            return result;
        }

        private static IEnumerable<(int RowNumber, string DepartmentName)> ReadDepartmentRowsFromXlsx(IFormFile importFile)
        {
            using var stream = importFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (var row = 2; row <= lastRow; row++)
            {
                var departmentName = worksheet.Cell(row, 1).GetString().Trim();
                yield return (row, departmentName);
            }
        }
    }
}
