using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using myapp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using myapp.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Text;

namespace myapp.Controllers
{
    [Authorize(Roles = "IT")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName");
            ViewBag.Sections = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.Plants = new SelectList(_context.Plants.OrderBy(p => p.PlantName).ToList(), "PlantName", "PlantName");
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
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Department = department.DepartmentName,
                        Section = section.SectionName,
                        Plant = model.Plant,
                        IsIT = model.IsIT,
                        MustChangePasswordOnFirstLogin = true,
                        CreatedBy = User.Identity?.Name ?? "System",
                        UpdatedBy = User.Identity?.Name ?? "System",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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
            ViewBag.Plants = new SelectList(_context.Plants.OrderBy(p => p.PlantName).ToList(), "PlantName", "PlantName", model.Plant);
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
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DepartmentId = department?.DepartmentId ?? 0,
                SectionId = section?.SectionId ?? 0,
                Plant = user.Plant ?? string.Empty,
                IsIT = user.IsIT
            };

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.DepartmentId == model.DepartmentId).OrderBy(s => s.SectionName).ToListAsync(), "SectionId", "SectionName", model.SectionId);
            ViewBag.Plants = new SelectList(_context.Plants.OrderBy(p => p.PlantName).ToList(), "PlantName", "PlantName", model.Plant);
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
                    user.UserName = model.UserName;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Department = department.DepartmentName;
                    user.Section = section.SectionName;
                    user.Plant = model.Plant;
                    user.IsIT = model.IsIT;
                    user.UpdatedBy = User.Identity?.Name ?? "System";
                    user.UpdatedAt = DateTime.UtcNow;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(model.NewPassword))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                            if (!passwordResult.Succeeded)
                            {
                                foreach (var error in passwordResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName).ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
                                ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.DepartmentId == model.DepartmentId).OrderBy(s => s.SectionName).ToListAsync(), "SectionId", "SectionName", model.SectionId);
                                ViewBag.Plants = new SelectList(_context.Plants.OrderBy(p => p.PlantName).ToList(), "PlantName", "PlantName", model.Plant);
                                return View(model);
                            }

                            user.MustChangePasswordOnFirstLogin = true;
                            user.UpdatedBy = User.Identity?.Name ?? "System";
                            user.UpdatedAt = DateTime.UtcNow;
                            await _userManager.UpdateAsync(user);
                        }

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
            ViewBag.Plants = new SelectList(_context.Plants.OrderBy(p => p.PlantName).ToList(), "PlantName", "PlantName", model.Plant);
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
                "Users import started by {Actor}. File={FileName}, Size={Size}, StartedAt={StartedAt}",
                actor,
                importFile.FileName,
                importFile.Length,
                startedAt);

            var imported = 0;
            var skipped = 0;
            var reports = new List<string>();
            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                IEnumerable<(int RowNumber, string UserName, string Email, string FirstName, string LastName, string DepartmentName, string SectionName, string Password, bool IsIT, string Plant)> rows;
                if (extension == ".csv")
                {
                    rows = await ReadUserRowsFromCsvAsync(importFile);
                }
                else
                {
                    rows = ReadUserRowsFromXlsx(importFile);
                }

                foreach (var row in rows)
                {
                    var userName = string.IsNullOrWhiteSpace(row.UserName) ? row.Email : row.UserName;

                    if (string.IsNullOrWhiteSpace(row.Email) || string.IsNullOrWhiteSpace(row.Password))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Email and Password are required.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Username is required.");
                        continue;
                    }

                    if (!seenEmails.Add(row.Email))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Duplicate email '{row.Email}' in file.");
                        continue;
                    }

                    if (!seenUserNames.Add(userName))
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Duplicate username '{userName}' in file.");
                        continue;
                    }

                    var existingUser = await _userManager.FindByEmailAsync(row.Email);
                    if (existingUser != null)
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: User '{row.Email}' already exists.");
                        continue;
                    }

                    var existingByUserName = await _userManager.FindByNameAsync(userName);
                    if (existingByUserName != null)
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Username '{userName}' already exists.");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(row.DepartmentName))
                    {
                        var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == row.DepartmentName);
                        if (department == null)
                        {
                            skipped++;
                            reports.Add($"Row {row.RowNumber}: Department '{row.DepartmentName}' does not exist.");
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(row.SectionName))
                        {
                            var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionName == row.SectionName && s.DepartmentId == department.DepartmentId);
                            if (section == null)
                            {
                                skipped++;
                                reports.Add($"Row {row.RowNumber}: Section '{row.SectionName}' does not exist in department '{row.DepartmentName}'.");
                                continue;
                            }
                        }
                    }

                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = row.Email,
                        FirstName = row.FirstName,
                        LastName = row.LastName,
                        Department = row.DepartmentName,
                        Section = row.SectionName,
                        Plant = row.Plant,
                        IsIT = row.IsIT,
                        MustChangePasswordOnFirstLogin = true,
                        CreatedBy = actor,
                        UpdatedBy = actor,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, row.Password);
                    if (result.Succeeded)
                    {
                        imported++;
                    }
                    else
                    {
                        skipped++;
                        reports.Add($"Row {row.RowNumber}: Failed to create '{row.Email}' ({string.Join("; ", result.Errors.Select(e => e.Description))}).");
                    }
                }

                var finishedAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Users import completed by {Actor}. File={FileName}, StartedAt={StartedAt}, FinishedAt={FinishedAt}, Imported={Imported}, Skipped={Skipped}",
                    actor,
                    importFile.FileName,
                    startedAt,
                    finishedAt,
                    imported,
                    skipped);

                foreach (var report in reports.Take(20))
                {
                    _logger.LogWarning("Users import row issue: {Issue}", report);
                }

                TempData["SuccessMessage"] = $"User import completed. Imported: {imported}, Skipped: {skipped}";
                if (reports.Count > 0)
                {
                    TempData["ImportReport"] = string.Join("\n", reports.Take(50));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Users import failed by {Actor}. File={FileName}", actor, importFile.FileName);
                TempData["ErrorMessage"] = "User import failed. Please verify template/data.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadImportTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");
            worksheet.Cell(1, 1).Value = "Username";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "FirstName";
            worksheet.Cell(1, 4).Value = "LastName";
            worksheet.Cell(1, 5).Value = "Department";
            worksheet.Cell(1, 6).Value = "Section";
            worksheet.Cell(1, 7).Value = "Password";
            worksheet.Cell(1, 8).Value = "IsIT";
            worksheet.Cell(1, 9).Value = "Plant";

            worksheet.Cell(2, 1).Value = "user1";
            worksheet.Cell(2, 2).Value = "user1@example.com";
            worksheet.Cell(2, 3).Value = "John";
            worksheet.Cell(2, 4).Value = "Doe";
            worksheet.Cell(2, 5).Value = "Production";
            worksheet.Cell(2, 6).Value = "Assembly";
            worksheet.Cell(2, 7).Value = "Abcd1234!";
            worksheet.Cell(2, 8).Value = "false";
            worksheet.Cell(2, 9).Value = "6031";

            worksheet.Cell(3, 1).Value = "itadmin";
            worksheet.Cell(3, 2).Value = "itadmin@example.com";
            worksheet.Cell(3, 3).Value = "Jane";
            worksheet.Cell(3, 4).Value = "Admin";
            worksheet.Cell(3, 5).Value = "IT";
            worksheet.Cell(3, 6).Value = "System";
            worksheet.Cell(3, 7).Value = "Abcd1234!";
            worksheet.Cell(3, 8).Value = "true";
            worksheet.Cell(3, 9).Value = "6021";
            try
            {
                worksheet.Columns().AdjustToContents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-size Users import template columns. Returning default column widths.");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "users-template.xlsx");
        }

        private static async Task<IEnumerable<(int RowNumber, string UserName, string Email, string FirstName, string LastName, string DepartmentName, string SectionName, string Password, bool IsIT, string Plant)>> ReadUserRowsFromCsvAsync(IFormFile importFile)
        {
            var result = new List<(int RowNumber, string UserName, string Email, string FirstName, string LastName, string DepartmentName, string SectionName, string Password, bool IsIT, string Plant)>();
            using var reader = new StreamReader(importFile.OpenReadStream());
            var rowNumber = 0;
            var hasUsernameColumn = false;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (rowNumber == 1)
                {
                    hasUsernameColumn = line.Contains("Username", StringComparison.OrdinalIgnoreCase);
                    if (line.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var parts = line.Split(',');
                string userName;
                string email;
                string firstName;
                string lastName;
                string departmentName;
                string sectionName;
                string password;
                bool isIt;
                string plant;

                if (hasUsernameColumn)
                {
                    userName = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                    email = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                    firstName = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                    lastName = parts.Length > 3 ? parts[3].Trim() : string.Empty;
                    departmentName = parts.Length > 4 ? parts[4].Trim() : string.Empty;
                    sectionName = parts.Length > 5 ? parts[5].Trim() : string.Empty;
                    password = parts.Length > 6 ? parts[6].Trim() : string.Empty;
                    isIt = parts.Length > 7 && bool.TryParse(parts[7].Trim(), out var parsedIsIt) && parsedIsIt;
                    plant = parts.Length > 8 ? parts[8].Trim() : string.Empty;
                }
                else
                {
                    email = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                    userName = email;
                    firstName = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                    lastName = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                    departmentName = parts.Length > 3 ? parts[3].Trim() : string.Empty;
                    sectionName = parts.Length > 4 ? parts[4].Trim() : string.Empty;
                    password = parts.Length > 5 ? parts[5].Trim() : string.Empty;
                    isIt = parts.Length > 6 && bool.TryParse(parts[6].Trim(), out var parsedLegacyIsIt) && parsedLegacyIsIt;
                    plant = parts.Length > 7 ? parts[7].Trim() : string.Empty;
                }

                result.Add((rowNumber, userName, email, firstName, lastName, departmentName, sectionName, password, isIt, plant));
            }

            return result;
        }

        private static IEnumerable<(int RowNumber, string UserName, string Email, string FirstName, string LastName, string DepartmentName, string SectionName, string Password, bool IsIT, string Plant)> ReadUserRowsFromXlsx(IFormFile importFile)
        {
            using var stream = importFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            var firstHeader = worksheet.Cell(1, 1).GetString().Trim();
            var hasUsernameColumn = firstHeader.Equals("Username", StringComparison.OrdinalIgnoreCase);

            for (var row = 2; row <= lastRow; row++)
            {
                string userName;
                string email;
                string firstName;
                string lastName;
                string departmentName;
                string sectionName;
                string password;
                string isItText;
                string plant;

                if (hasUsernameColumn)
                {
                    userName = worksheet.Cell(row, 1).GetString().Trim();
                    email = worksheet.Cell(row, 2).GetString().Trim();
                    firstName = worksheet.Cell(row, 3).GetString().Trim();
                    lastName = worksheet.Cell(row, 4).GetString().Trim();
                    departmentName = worksheet.Cell(row, 5).GetString().Trim();
                    sectionName = worksheet.Cell(row, 6).GetString().Trim();
                    password = worksheet.Cell(row, 7).GetString().Trim();
                    isItText = worksheet.Cell(row, 8).GetString().Trim();
                    plant = worksheet.Cell(row, 9).GetString().Trim();
                }
                else
                {
                    email = worksheet.Cell(row, 1).GetString().Trim();
                    userName = email;
                    firstName = worksheet.Cell(row, 2).GetString().Trim();
                    lastName = worksheet.Cell(row, 3).GetString().Trim();
                    departmentName = worksheet.Cell(row, 4).GetString().Trim();
                    sectionName = worksheet.Cell(row, 5).GetString().Trim();
                    password = worksheet.Cell(row, 6).GetString().Trim();
                    isItText = worksheet.Cell(row, 7).GetString().Trim();
                    plant = worksheet.Cell(row, 8).GetString().Trim();
                }

                var isIt = bool.TryParse(isItText, out var parsed) && parsed;
                yield return (row, userName, email, firstName, lastName, departmentName, sectionName, password, isIt, plant);
            }
        }
    }
}
