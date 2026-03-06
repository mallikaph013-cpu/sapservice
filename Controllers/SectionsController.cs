using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace myapp.Controllers
{
    [Authorize(Roles = "IT")]
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SectionsController> _logger;

        public SectionsController(ApplicationDbContext context, ILogger<SectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Sections
        public async Task<IActionResult> Index()
        {
            var sections = _context.Sections.Include(s => s.Department);
            return View(await sections.ToListAsync());
        }

        // GET: Sections/Create
        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: Sections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SectionName,DepartmentId")] Section section)
        {
            if (ModelState.IsValid)
            {
                var actor = User.Identity?.Name ?? "System";
                var now = DateTime.UtcNow;
                section.CreatedAt = now;
                section.UpdatedAt = now;
                section.CreatedBy = actor;
                section.UpdatedBy = actor;

                _context.Add(section);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Section created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", section.DepartmentId);
            return View(section);
        }

        // GET: Sections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", section.DepartmentId);
            return View(section);
        }

        // POST: Sections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SectionId,SectionName,DepartmentId")] Section section)
        {
            if (id != section.SectionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Sections.FindAsync(id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.SectionName = section.SectionName;
                    existing.DepartmentId = section.DepartmentId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Section updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.SectionId))
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
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", section.DepartmentId);
            return View(section);
        }

        // GET: Sections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.SectionId == id);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // POST: Sections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section != null)
            {
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Section deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.SectionId == id);
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
                "Sections import started by {Actor}. File={FileName}, Size={Size}, StartedAt={StartedAt}",
                actor,
                importFile.FileName,
                importFile.Length,
                startedAt);

            var imported = 0;
            var skipped = 0;
            var reports = new List<string>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                IEnumerable<(int RowNumber, string SectionName, string DepartmentName)> rows;
                if (extension == ".csv")
                {
                    rows = await ReadSectionRowsFromCsvAsync(importFile);
                }
                else
                {
                    rows = ReadSectionRowsFromXlsx(importFile);
                }

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.SectionName) || string.IsNullOrWhiteSpace(row.DepartmentName))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: SectionName and DepartmentName are required.");
                        continue;
                    }

                    var uniqueKey = $"{row.SectionName}|{row.DepartmentName}";
                    if (!seenKeys.Add(uniqueKey))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Duplicate section mapping '{row.SectionName}'/'{row.DepartmentName}' in file.");
                        continue;
                    }

                    var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == row.DepartmentName);
                    if (department == null)
                    {
                        var now = DateTime.UtcNow;
                        department = new Department
                        {
                            DepartmentName = row.DepartmentName,
                            CreatedAt = now,
                            UpdatedAt = now,
                            CreatedBy = actor,
                            UpdatedBy = actor
                        };
                        _context.Departments.Add(department);
                        await _context.SaveChangesAsync();
                    }

                    var exists = await _context.Sections.AnyAsync(s => s.SectionName == row.SectionName && s.DepartmentId == department.DepartmentId);
                    if (exists)
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Section '{row.SectionName}' already exists in department '{row.DepartmentName}'.");
                        continue;
                    }

                    var sectionNow = DateTime.UtcNow;
                    _context.Sections.Add(new Section
                    {
                        SectionName = row.SectionName,
                        DepartmentId = department.DepartmentId,
                        CreatedAt = sectionNow,
                        UpdatedAt = sectionNow,
                        CreatedBy = actor,
                        UpdatedBy = actor
                    });
                    imported++;
                }

                await _context.SaveChangesAsync();
                var finishedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Sections import completed by {Actor}. File={FileName}, StartedAt={StartedAt}, FinishedAt={FinishedAt}, Imported={Imported}, Skipped={Skipped}",
                    actor,
                    importFile.FileName,
                    startedAt,
                    finishedAt,
                    imported,
                    skipped);

                foreach (var report in reports.Take(20))
                {
                    _logger.LogWarning("Sections import row issue: {Issue}", report);
                }

                TempData["SuccessMessage"] = $"Section import completed. Imported: {imported}, Skipped: {skipped}";
                if (reports.Count > 0)
                {
                    TempData["ImportReport"] = string.Join("\n", reports.Take(50));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sections import failed by {Actor}. File={FileName}", actor, importFile.FileName);
                TempData["ErrorMessage"] = "Section import failed. Please verify template/data.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadImportTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sections");
            worksheet.Cell(1, 1).Value = "SectionName";
            worksheet.Cell(1, 2).Value = "DepartmentName";
            worksheet.Cell(2, 1).Value = "Assembly";
            worksheet.Cell(2, 2).Value = "Production";
            worksheet.Cell(3, 1).Value = "Injection";
            worksheet.Cell(3, 2).Value = "Production";
            try
            {
                worksheet.Columns().AdjustToContents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-size Sections import template columns. Returning default column widths.");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "sections-template.xlsx");
        }

        private static async Task<IEnumerable<(int RowNumber, string SectionName, string DepartmentName)>> ReadSectionRowsFromCsvAsync(IFormFile importFile)
        {
            var result = new List<(int RowNumber, string SectionName, string DepartmentName)>();
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

                if (rowNumber == 1 && line.Contains("Section", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = line.Split(',');
                var sectionName = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                var departmentName = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                result.Add((rowNumber, sectionName, departmentName));
            }

            return result;
        }

        private static IEnumerable<(int RowNumber, string SectionName, string DepartmentName)> ReadSectionRowsFromXlsx(IFormFile importFile)
        {
            using var stream = importFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (var row = 2; row <= lastRow; row++)
            {
                var sectionName = worksheet.Cell(row, 1).GetString().Trim();
                var departmentName = worksheet.Cell(row, 2).GetString().Trim();
                yield return (row, sectionName, departmentName);
            }
        }
    }
}
