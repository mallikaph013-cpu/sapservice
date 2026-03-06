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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.Text;

namespace myapp.Controllers
{
    [Authorize]
    public class DocumentRoutingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentRoutingController> _logger;

        public DocumentRoutingController(ApplicationDbContext context, ILogger<DocumentRoutingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<DocumentRoutingViewModel> PopulateViewModelAsync(DocumentRoutingViewModel? viewModel = null)
        {
            viewModel ??= new DocumentRoutingViewModel();

            viewModel.DocumentRoutings = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section)
                .Include(dr => dr.Plant)
                .OrderBy(dr => dr.DocumentType.Name)
                .ThenBy(dr => dr.Step)
                .ToListAsync();

            // Prepare SelectLists
            var documentTypes = Enum.GetNames(typeof(RequestType)).ToList();
            var departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            var sections = await _context.Sections.OrderBy(s => s.SectionName).Select(s => s.SectionName).Distinct().ToListAsync();
            var plants = await _context.Plants.OrderBy(p => p.PlantName).Select(p => p.PlantName).Distinct().ToListAsync();

            viewModel.DocumentTypeList = new SelectList(documentTypes);
            // Use DepartmentId for value and DepartmentName for text
            viewModel.DepartmentList = new SelectList(departments, "DepartmentId", "DepartmentName");
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
        public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] CreateDocumentRoutingViewModel createModel)
        {
            if (ModelState.IsValid)
            {
                var actor = User.Identity?.Name ?? "System";

                var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(d => d.Name == createModel.NewDocumentTypeName);
                if (documentType == null)
                {
                    documentType = new DocumentType { Name = createModel.NewDocumentTypeName };
                    _context.DocumentTypes.Add(documentType);
                }

                // Find department by ID - This is now reliable
                var department = await _context.Departments.FindAsync(createModel.DepartmentId);

                if (department == null)
                {
                    // This should theoretically not happen if the dropdown is populated correctly
                    ModelState.AddModelError("CreateForm.DepartmentId", "Selected department is invalid.");
                    var viewModel = await PopulateViewModelAsync();
                    viewModel.CreateForm = createModel;
                    return View("Index", viewModel);
                }

                var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == createModel.NewSectionName && s.DepartmentId == department.DepartmentId);
                if (section == null)
                {
                    var sectionNow = DateTime.UtcNow;
                    section = new Section
                    {
                        SectionName = createModel.NewSectionName,
                        DepartmentId = department.DepartmentId,
                        CreatedAt = sectionNow,
                        UpdatedAt = sectionNow,
                        CreatedBy = actor,
                        UpdatedBy = actor
                    };
                    _context.Sections.Add(section);
                }

                var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantName == createModel.NewPlantName);
                if (plant == null)
                {
                    plant = new Plant { PlantName = createModel.NewPlantName };
                    _context.Plants.Add(plant);
                }

                await _context.SaveChangesAsync();

                var routingExists = await _context.DocumentRoutings.AnyAsync(dr =>
                    dr.DocumentTypeId == documentType.DocumentTypeId &&
                    dr.DepartmentId == department.DepartmentId &&
                    dr.SectionId == section.SectionId &&
                    dr.PlantId == plant.PlantId);

                if (!routingExists)
                {
                    var now = DateTime.UtcNow;
                    var newRouting = new DocumentRouting
                    {
                        DocumentTypeId = documentType.DocumentTypeId,
                        DepartmentId = department.DepartmentId,
                        SectionId = section.SectionId,
                        PlantId = plant.PlantId,
                        Step = createModel.Step,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = actor,
                        UpdatedBy = actor
                    };
                    _context.DocumentRoutings.Add(newRouting);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Document routing created successfully!";
                }
                else
                {
                    TempData["InfoMessage"] = "This document routing sequence already exists.";
                }

                return RedirectToAction(nameof(Index));
            }

            var invalidViewModel = await PopulateViewModelAsync();
            invalidViewModel.CreateForm = createModel;
            return View("Index", invalidViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentRouting = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section)
                .Include(dr => dr.Plant)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (documentRouting == null)
            {
                return NotFound();
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName), "DepartmentId", "DepartmentName", documentRouting.DepartmentId);
            ViewData["SectionId"] = new SelectList(
                _context.Sections
                    .Where(s => s.DepartmentId == documentRouting.DepartmentId)
                    .OrderBy(s => s.SectionName),
                "SectionId",
                "SectionName",
                documentRouting.SectionId);
            ViewData["PlantId"] = new SelectList(_context.Plants.OrderBy(p => p.PlantName), "PlantId", "PlantName", documentRouting.PlantId);

            return View(documentRouting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DocumentTypeId,DepartmentId,SectionId,PlantId,Step")] DocumentRouting documentRoutingFromForm)
        {
            if (id != documentRoutingFromForm.Id)
            {
                return NotFound();
            }

            var routingToUpdate = await _context.DocumentRoutings.FindAsync(id);

            if (routingToUpdate == null)
            {
                return NotFound();
            }

            routingToUpdate.DepartmentId = documentRoutingFromForm.DepartmentId;
            routingToUpdate.SectionId = documentRoutingFromForm.SectionId;
            routingToUpdate.PlantId = documentRoutingFromForm.PlantId;
            routingToUpdate.Step = documentRoutingFromForm.Step;
            routingToUpdate.UpdatedAt = DateTime.UtcNow;
            routingToUpdate.UpdatedBy = User.Identity?.Name ?? "System";

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Document routing updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Please try again.");
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName), "DepartmentId", "DepartmentName", routingToUpdate.DepartmentId);
            ViewData["SectionId"] = new SelectList(
                _context.Sections
                    .Where(s => s.DepartmentId == routingToUpdate.DepartmentId)
                    .OrderBy(s => s.SectionName),
                "SectionId",
                "SectionName",
                routingToUpdate.SectionId);
            ViewData["PlantId"] = new SelectList(_context.Plants.OrderBy(p => p.PlantName), "PlantId", "PlantName", routingToUpdate.PlantId);
            
            var documentType = await _context.DocumentTypes.FindAsync(routingToUpdate.DocumentTypeId);
            if (documentType != null)
            {
                routingToUpdate.DocumentType = documentType;
            }

            return View(routingToUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByDepartment(int departmentId)
        {
            if (departmentId <= 0)
            {
                return Json(new List<object>());
            }

            var sections = await _context.Sections
                .Where(s => s.DepartmentId == departmentId)
                .OrderBy(s => s.SectionName)
                .Select(s => new { id = s.SectionId, name = s.SectionName })
                .ToListAsync();

            return Json(sections);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentRouting = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section)
                .Include(dr => dr.Plant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (documentRouting == null)
            {
                return NotFound();
            }

            return View(documentRouting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var documentRouting = await _context.DocumentRoutings.FindAsync(id);
            if (documentRouting != null)
            {
                _context.DocumentRoutings.Remove(documentRouting);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Document routing deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
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
                "DocumentRouting import started by {Actor}. File={FileName}, Size={Size}, StartedAt={StartedAt}",
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
                IEnumerable<(int RowNumber, string DocumentTypeName, string DepartmentName, string SectionName, string PlantName, string StepText)> rows;
                if (extension == ".csv")
                {
                    rows = await ReadRoutingRowsFromCsvAsync(importFile);
                }
                else
                {
                    rows = ReadRoutingRowsFromXlsx(importFile);
                }

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.DocumentTypeName) ||
                        string.IsNullOrWhiteSpace(row.DepartmentName) ||
                        string.IsNullOrWhiteSpace(row.SectionName) ||
                        string.IsNullOrWhiteSpace(row.PlantName) ||
                        !int.TryParse(row.StepText, out var step))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Required fields missing or Step is invalid.");
                        continue;
                    }

                    var uniqueKey = $"{row.DocumentTypeName}|{row.DepartmentName}|{row.SectionName}|{row.PlantName}|{step}";
                    if (!seenKeys.Add(uniqueKey))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Duplicate routing entry in file.");
                        continue;
                    }

                    var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(d => d.Name == row.DocumentTypeName);
                    if (documentType == null)
                    {
                        documentType = new DocumentType { Name = row.DocumentTypeName };
                        _context.DocumentTypes.Add(documentType);
                        await _context.SaveChangesAsync();
                    }

                    var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == row.DepartmentName);
                    if (department == null)
                    {
                        var departmentNow = DateTime.UtcNow;
                        department = new Department
                        {
                            DepartmentName = row.DepartmentName,
                            CreatedAt = departmentNow,
                            UpdatedAt = departmentNow,
                            CreatedBy = actor,
                            UpdatedBy = actor
                        };
                        _context.Departments.Add(department);
                        await _context.SaveChangesAsync();
                    }

                    var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == row.SectionName && s.DepartmentId == department.DepartmentId);
                    if (section == null)
                    {
                        var sectionNow = DateTime.UtcNow;
                        section = new Section
                        {
                            SectionName = row.SectionName,
                            DepartmentId = department.DepartmentId,
                            CreatedAt = sectionNow,
                            UpdatedAt = sectionNow,
                            CreatedBy = actor,
                            UpdatedBy = actor
                        };
                        _context.Sections.Add(section);
                        await _context.SaveChangesAsync();
                    }

                    var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantName == row.PlantName);
                    if (plant == null)
                    {
                        plant = new Plant { PlantName = row.PlantName };
                        _context.Plants.Add(plant);
                        await _context.SaveChangesAsync();
                    }

                    var exists = await _context.DocumentRoutings.AnyAsync(dr =>
                        dr.DocumentTypeId == documentType.DocumentTypeId &&
                        dr.DepartmentId == department.DepartmentId &&
                        dr.SectionId == section.SectionId &&
                        dr.PlantId == plant.PlantId &&
                        dr.Step == step);

                    if (exists)
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Routing already exists.");
                        continue;
                    }

                    var routingNow = DateTime.UtcNow;
                    _context.DocumentRoutings.Add(new DocumentRouting
                    {
                        DocumentTypeId = documentType.DocumentTypeId,
                        DepartmentId = department.DepartmentId,
                        SectionId = section.SectionId,
                        PlantId = plant.PlantId,
                        Step = step,
                        CreatedAt = routingNow,
                        UpdatedAt = routingNow,
                        CreatedBy = actor,
                        UpdatedBy = actor
                    });

                    imported++;
                }

                await _context.SaveChangesAsync();
                var finishedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "DocumentRouting import completed by {Actor}. File={FileName}, StartedAt={StartedAt}, FinishedAt={FinishedAt}, Imported={Imported}, Skipped={Skipped}",
                    actor,
                    importFile.FileName,
                    startedAt,
                    finishedAt,
                    imported,
                    skipped);

                foreach (var report in reports.Take(20))
                {
                    _logger.LogWarning("DocumentRouting import row issue: {Issue}", report);
                }

                TempData["SuccessMessage"] = $"Document routing import completed. Imported: {imported}, Skipped: {skipped}";
                if (reports.Count > 0)
                {
                    TempData["ImportReport"] = string.Join("\n", reports.Take(50));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentRouting import failed by {Actor}. File={FileName}", actor, importFile.FileName);
                TempData["ErrorMessage"] = "Document routing import failed. Please verify template/data.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadImportTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DocumentRouting");
            worksheet.Cell(1, 1).Value = "DocumentType";
            worksheet.Cell(1, 2).Value = "DepartmentName";
            worksheet.Cell(1, 3).Value = "SectionName";
            worksheet.Cell(1, 4).Value = "PlantName";
            worksheet.Cell(1, 5).Value = "Step";

            worksheet.Cell(2, 1).Value = "FG";
            worksheet.Cell(2, 2).Value = "Production";
            worksheet.Cell(2, 3).Value = "Assembly";
            worksheet.Cell(2, 4).Value = "6031";
            worksheet.Cell(2, 5).Value = "1";

            worksheet.Cell(3, 1).Value = "FG";
            worksheet.Cell(3, 2).Value = "Production";
            worksheet.Cell(3, 3).Value = "Injection";
            worksheet.Cell(3, 4).Value = "6051";
            worksheet.Cell(3, 5).Value = "2";
            try
            {
                worksheet.Columns().AdjustToContents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-size DocumentRouting import template columns. Returning default column widths.");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "document-routing-template.xlsx");
        }

        private static async Task<IEnumerable<(int RowNumber, string DocumentTypeName, string DepartmentName, string SectionName, string PlantName, string StepText)>> ReadRoutingRowsFromCsvAsync(IFormFile importFile)
        {
            var result = new List<(int RowNumber, string DocumentTypeName, string DepartmentName, string SectionName, string PlantName, string StepText)>();
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

                if (rowNumber == 1 && line.Contains("DocumentType", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = line.Split(',');
                var documentTypeName = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                var departmentName = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                var sectionName = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                var plantName = parts.Length > 3 ? parts[3].Trim() : string.Empty;
                var stepText = parts.Length > 4 ? parts[4].Trim() : string.Empty;
                result.Add((rowNumber, documentTypeName, departmentName, sectionName, plantName, stepText));
            }

            return result;
        }

        private static IEnumerable<(int RowNumber, string DocumentTypeName, string DepartmentName, string SectionName, string PlantName, string StepText)> ReadRoutingRowsFromXlsx(IFormFile importFile)
        {
            using var stream = importFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (var row = 2; row <= lastRow; row++)
            {
                var documentTypeName = worksheet.Cell(row, 1).GetString().Trim();
                var departmentName = worksheet.Cell(row, 2).GetString().Trim();
                var sectionName = worksheet.Cell(row, 3).GetString().Trim();
                var plantName = worksheet.Cell(row, 4).GetString().Trim();
                var stepText = worksheet.Cell(row, 5).GetString().Trim();
                yield return (row, documentTypeName, departmentName, sectionName, plantName, stepText);
            }
        }
    }
}
