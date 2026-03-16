using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Threading.Tasks;
using System.Text.Json;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace myapp.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public RequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<RequestsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _environment = environment;
        }

        private void SetBomComponentProperty(BomComponent component, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                var propertyInfo = GetPropertyByHeader(typeof(BomComponent), propertyName);
                if (propertyInfo == null || !propertyInfo.CanWrite) return;

                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                object convertedValue;

                if (targetType == typeof(decimal))
                    convertedValue = decimal.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(int))
                    convertedValue = int.Parse(value, CultureInfo.InvariantCulture);
                else
                    convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                propertyInfo.SetValue(component, convertedValue, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set BOM component property {PropertyName} with value {Value}", propertyName, value);
            }
        }

        private void SetBomEditComponentProperty(BomEditComponent component, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                var propertyInfo = GetPropertyByHeader(typeof(BomEditComponent), propertyName);
                if (propertyInfo == null || !propertyInfo.CanWrite) return;

                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                object convertedValue;

                if (targetType == typeof(decimal))
                    convertedValue = decimal.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(int))
                    convertedValue = int.Parse(value, CultureInfo.InvariantCulture);
                else
                    convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                propertyInfo.SetValue(component, convertedValue, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set Edit BOM component property {PropertyName} with value {Value}", propertyName, value);
            }
        }

        private void SetRoutingProperty(Routing routing, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                var propertyInfo = GetPropertyByHeader(typeof(Routing), propertyName);
                if (propertyInfo == null || !propertyInfo.CanWrite) return;

                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                object convertedValue;

                if (targetType == typeof(decimal))
                    convertedValue = decimal.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(int))
                    convertedValue = int.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(DateTime))
                    convertedValue = DateTime.Parse(value, CultureInfo.InvariantCulture);
                else
                    convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                propertyInfo.SetValue(routing, convertedValue, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set Routing property {PropertyName} with value {Value}", propertyName, value);
            }
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Should not happen for authorized users
            }

            var allRequests = await _context.RequestItems
                .Where(r => r.UsageStatus != 9)
                .ToListAsync();

            List<RequestItem> requests;

            if (User.IsInRole("IT"))
            {
                requests = allRequests;
            }
            else
            {
                var currentUserName = $"{user.FirstName} {user.LastName}".Trim();
                var currentUserId = user.Id;
                var currentActor = (User?.Identity?.Name ?? string.Empty).Trim();
                var currentUserDepartment = (user.Department ?? string.Empty).Trim();
                var currentUserSection = (user.Section ?? string.Empty).Trim();

                var participatedRequestIds = await _context.AuditLogs
                    .Where(a => a.EntityName == nameof(RequestItem)
                        && (a.Action == "Create" || a.Action == "Update")
                        && (
                            (!string.IsNullOrWhiteSpace(currentActor) && a.PerformedBy == currentActor)
                            || a.PerformedBy == currentUserName))
                    .Select(a => a.EntityId)
                    .ToListAsync();

                var participatedIdSet = participatedRequestIds
                    .Where(id => int.TryParse(id, out _))
                    .Select(id => int.Parse(id!))
                    .ToHashSet();

                var routingRules = await _context.DocumentRoutings
                    .Include(dr => dr.DocumentType)
                    .Include(dr => dr.Department)
                    .Include(dr => dr.Section)
                    .Include(dr => dr.Plant)
                    .ToListAsync();

                bool IsUserInvolvedByRouting(RequestItem request)
                {
                    if (string.IsNullOrWhiteSpace(request.RequestType)) return false;

                    var normalizedPlant = (request.Plant ?? string.Empty).Trim();

                    var matchedRules = routingRules.Where(dr =>
                        dr.DocumentType?.Name == request.RequestType &&
                        (string.IsNullOrWhiteSpace(normalizedPlant)
                            || dr.Plant.PlantName == normalizedPlant
                            || dr.Plant.PlantName.StartsWith(normalizedPlant + " ")
                            || dr.Plant.PlantName.StartsWith(normalizedPlant + "(")));

                    return matchedRules.Any(dr =>
                    {
                        var departmentName = (dr.Department?.DepartmentName ?? string.Empty).Trim();
                        var sectionName = (dr.Section?.SectionName ?? string.Empty).Trim();

                        var departmentMatched = string.IsNullOrWhiteSpace(departmentName)
                            || string.Equals(departmentName, currentUserDepartment, StringComparison.OrdinalIgnoreCase);
                        var sectionMatched = string.IsNullOrWhiteSpace(sectionName)
                            || string.Equals(sectionName, currentUserSection, StringComparison.OrdinalIgnoreCase);

                        return departmentMatched && sectionMatched;
                    });
                }

                requests = allRequests
                    .Where(r =>
                        string.Equals(r.Requester, currentUserName, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(r.NextApproverId, currentUserId, StringComparison.Ordinal)
                        || (!string.IsNullOrWhiteSpace(r.UpdatedBy)
                            && (!string.IsNullOrWhiteSpace(currentActor) && string.Equals(r.UpdatedBy, currentActor, StringComparison.OrdinalIgnoreCase)))
                        || participatedIdSet.Contains(r.Id)
                        || IsUserInvolvedByRouting(r))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                var isDateSearch = DateTime.TryParse(searchTerm, out var parsedDate);
                var searchDate = parsedDate.Date;

                requests = requests.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.RequestType) && r.RequestType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(r.Status) && r.Status.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(r.ItemCode) && r.ItemCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    || (isDateSearch && r.RequestDate.Date == searchDate)
                ).ToList();
            }

            ViewBag.SearchTerm = searchTerm;

            requests = requests.OrderByDescending(r => r.RequestDate).ToList();

            var requestIds = requests.Select(r => r.Id.ToString()).ToList();
            var updateAuditLogs = await _context.AuditLogs
                .Where(a => a.EntityName == nameof(RequestItem)
                    && a.Action == "Update"
                    && a.EntityId != null
                    && requestIds.Contains(a.EntityId))
                .Select(a => new { a.EntityId, a.PerformedBy })
                .ToListAsync();

            var allUsers = await _userManager.Users.ToListAsync();
            var usersById = allUsers.ToDictionary(u => u.Id, u => u);

            bool IsUpdatedByNextApprover(RequestItem request)
            {
                if (string.IsNullOrWhiteSpace(request.NextApproverId))
                {
                    return false;
                }

                if (!usersById.TryGetValue(request.NextApproverId, out var nextApprover))
                {
                    return false;
                }

                var nextApproverCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(nextApprover.UserName)) nextApproverCandidates.Add(nextApprover.UserName.Trim());
                if (!string.IsNullOrWhiteSpace(nextApprover.Email)) nextApproverCandidates.Add(nextApprover.Email.Trim());

                var fullName = $"{nextApprover.FirstName} {nextApprover.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(fullName)) nextApproverCandidates.Add(fullName);

                if (!nextApproverCandidates.Any())
                {
                    return false;
                }

                return updateAuditLogs.Any(a =>
                    int.TryParse(a.EntityId, out var auditRequestId)
                    && auditRequestId == request.Id
                    && !string.IsNullOrWhiteSpace(a.PerformedBy)
                    && nextApproverCandidates.Contains(a.PerformedBy.Trim()));
            }

            var nextApproverNameByRequestId = requests.ToDictionary(
                r => r.Id,
                r =>
                {
                    if (string.IsNullOrWhiteSpace(r.NextApproverId))
                    {
                        return "-";
                    }

                    return usersById.TryGetValue(r.NextApproverId, out var nextApprover)
                        ? $"{nextApprover.FirstName} {nextApprover.LastName}".Trim()
                        : "-";
                });

            var allRoutingRules = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section)
                .Include(dr => dr.Plant)
                .OrderBy(dr => dr.Step)
                .ThenBy(dr => dr.Id)
                .ToListAsync();

            bool IsAtFinalStep(RequestItem request)
            {
                if (string.IsNullOrWhiteSpace(request.RequestType) || string.IsNullOrWhiteSpace(request.NextApproverId))
                {
                    return false;
                }

                if (!usersById.TryGetValue(request.NextApproverId, out var currentApprover))
                {
                    return false;
                }

                var normalizedPlant = (request.Plant ?? string.Empty).Trim();
                var matchedRules = allRoutingRules
                    .Where(dr => dr.DocumentType?.Name == request.RequestType)
                    .Where(dr => string.IsNullOrWhiteSpace(normalizedPlant)
                        || dr.Plant.PlantName == normalizedPlant
                        || dr.Plant.PlantName.StartsWith(normalizedPlant + " ")
                        || dr.Plant.PlantName.StartsWith(normalizedPlant + "("))
                    .ToList();

                if (!matchedRules.Any())
                {
                    return false;
                }

                var stepCandidates = new List<(int Step, List<ApplicationUser> Users)>();
                foreach (var rule in matchedRules)
                {
                    var departmentName = (rule.Department?.DepartmentName ?? string.Empty).Trim();
                    var sectionName = (rule.Section?.SectionName ?? string.Empty).Trim();

                    var usersInRule = allUsers.AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(departmentName))
                    {
                        usersInRule = usersInRule.Where(u => !string.IsNullOrWhiteSpace(u.Department)
                            && string.Equals(u.Department.Trim(), departmentName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (!string.IsNullOrWhiteSpace(sectionName))
                    {
                        usersInRule = usersInRule.Where(u => !string.IsNullOrWhiteSpace(u.Section)
                            && string.Equals(u.Section.Trim(), sectionName, StringComparison.OrdinalIgnoreCase));
                    }

                    var ruleUsers = usersInRule.ToList();
                    if (ruleUsers.Any())
                    {
                        stepCandidates.Add((rule.Step, ruleUsers));
                    }
                }

                if (!stepCandidates.Any())
                {
                    return false;
                }

                var currentStep = stepCandidates
                    .Where(c => c.Users.Any(u => u.Id == currentApprover.Id))
                    .Select(c => c.Step)
                    .DefaultIfEmpty(0)
                    .Min();

                if (currentStep == 0)
                {
                    return false;
                }

                return !stepCandidates.Any(c => c.Step > currentStep);
            }

            var hideDeleteIdSet = requests
                .Where(r => IsUpdatedByNextApprover(r) || IsAtFinalStep(r))
                .Select(r => r.Id)
                .ToHashSet();

            ViewBag.HideDeleteIdSet = hideDeleteIdSet;
            ViewBag.NextApproverNameByRequestId = nextApproverNameByRequestId;
            
            return View(requests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestItem = await _context.RequestItems
                .Include(r => r.BomComponents)
                .Include(r => r.Routings)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (requestItem == null)
            {
                return NotFound();
            }

            var nextApproverUser = !string.IsNullOrWhiteSpace(requestItem.NextApproverId)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.Id == requestItem.NextApproverId)
                : null;

            ViewBag.NextResponsibleUserName = nextApproverUser != null
                ? $"{nextApproverUser.FirstName} {nextApproverUser.LastName}"
                : "-";

            return View(requestItem);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int id)
        {
            var requestItem = await _context.RequestItems
                .Include(r => r.BomComponents)
                .Include(r => r.bomEditComponents)
                .Include(r => r.Routings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestItem == null)
            {
                return NotFound();
            }

            using var workbook = new XLWorkbook();

            var requestSheet = workbook.Worksheets.Add("Request");
            var metadataHeaders = new List<string>
            {
                "Request ID",
                "Request Type",
                "Status",
                "Requester",
                "Request Date",
                "Description"
            };

            var requestTypeHeaders = new List<string>();
            if (Enum.TryParse<RequestType>(requestItem.RequestType, true, out var parsedRequestType))
            {
                requestTypeHeaders = GetHeadersForRequestType(parsedRequestType);
            }

            var requestHeaders = metadataHeaders
                .Concat(requestTypeHeaders)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            object? GetRequestValue(string header)
            {
                return header switch
                {
                    "Request ID" => requestItem.Id,
                    "Request Type" => requestItem.RequestType,
                    "Status" => requestItem.Status,
                    "Requester" => requestItem.Requester,
                    "Request Date" => requestItem.RequestDate,
                    "Description" => requestItem.Description,
                    _ => GetPropertyByHeader(typeof(RequestItem), header)?.GetValue(requestItem)
                };
            }

            var requestTypeKey = (requestItem.RequestType ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();

            var horizontalExportTypes = new HashSet<string>
            {
                "fg",
                "sm",
                "rm",
                "toolingb",
                "toolingbfg",
                "toolingbpu",
                "routing",
                "addstorage",
                "distributionchanel",
                "ipo",
                "passthrough",
                "passthought",
                "crossplantpurchase"
            };

            if (horizontalExportTypes.Contains(requestTypeKey))
            {
                for (var i = 0; i < requestHeaders.Count; i++)
                {
                    var header = requestHeaders[i];
                    requestSheet.Cell(1, i + 1).Value = header;
                    requestSheet.Cell(2, i + 1).Value = GetRequestValue(header)?.ToString() ?? string.Empty;
                }

                var requestHeaderHorizontal = requestSheet.Range(1, 1, 1, requestHeaders.Count);
                requestHeaderHorizontal.Style.Font.Bold = true;
                requestHeaderHorizontal.Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            else
            {
                requestSheet.Cell(1, 1).Value = "Field";
                requestSheet.Cell(1, 2).Value = "Value";

                for (var i = 0; i < requestHeaders.Count; i++)
                {
                    var header = requestHeaders[i];
                    requestSheet.Cell(i + 2, 1).Value = header;
                    requestSheet.Cell(i + 2, 2).Value = GetRequestValue(header)?.ToString() ?? string.Empty;
                }

                var requestHeaderVertical = requestSheet.Range(1, 1, 1, 2);
                requestHeaderVertical.Style.Font.Bold = true;
                requestHeaderVertical.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            requestSheet.Columns().AdjustToContents();

            if (requestItem.BomComponents != null && requestItem.BomComponents.Any())
            {
                var bomSheet = workbook.Worksheets.Add("BOM Components");
                var bomHeaders = new[] { "Level", "Item", "Item Cat", "Component", "Description", "Quantity", "Unit", "Usage", "Plant", "Sloc" };
                for (var i = 0; i < bomHeaders.Length; i++)
                {
                    bomSheet.Cell(1, i + 1).Value = bomHeaders[i];
                }

                var bomRow = 2;
                foreach (var item in requestItem.BomComponents.OrderBy(c => c.Level).ThenBy(c => c.Item))
                {
                    bomSheet.Cell(bomRow, 1).Value = item.Level;
                    bomSheet.Cell(bomRow, 2).Value = item.Item ?? string.Empty;
                    bomSheet.Cell(bomRow, 3).Value = item.ItemCat ?? string.Empty;
                    bomSheet.Cell(bomRow, 4).Value = item.ComponentNumber ?? string.Empty;
                    bomSheet.Cell(bomRow, 5).Value = item.Description ?? string.Empty;
                    bomSheet.Cell(bomRow, 6).Value = item.ItemQuantity;
                    bomSheet.Cell(bomRow, 7).Value = item.Unit ?? string.Empty;
                    bomSheet.Cell(bomRow, 8).Value = item.BomUsage ?? string.Empty;
                    bomSheet.Cell(bomRow, 9).Value = item.Plant ?? string.Empty;
                    bomSheet.Cell(bomRow, 10).Value = item.Sloc ?? string.Empty;
                    bomRow++;
                }

                var bomHeader = bomSheet.Range(1, 1, 1, bomHeaders.Length);
                bomHeader.Style.Font.Bold = true;
                bomHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                bomSheet.Columns().AdjustToContents();
            }

            if (requestItem.bomEditComponents != null && requestItem.bomEditComponents.Any())
            {
                var editBomSheet = workbook.Worksheets.Add("Edit BOM Components");
                var editBomHeaders = GetHeadersForRequestType(RequestType.EditBOM);
                for (var i = 0; i < editBomHeaders.Count; i++)
                {
                    editBomSheet.Cell(1, i + 1).Value = editBomHeaders[i];
                }

                var editBomRow = 2;
                foreach (var item in requestItem.bomEditComponents)
                {
                    for (var i = 0; i < editBomHeaders.Count; i++)
                    {
                        var header = editBomHeaders[i];
                        var propertyValue = GetPropertyByHeader(typeof(BomEditComponent), header)?.GetValue(item);
                        editBomSheet.Cell(editBomRow, i + 1).Value = propertyValue?.ToString() ?? string.Empty;
                    }
                    editBomRow++;
                }

                var editBomHeader = editBomSheet.Range(1, 1, 1, editBomHeaders.Count);
                editBomHeader.Style.Font.Bold = true;
                editBomHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                editBomSheet.Columns().AdjustToContents();
            }

            if (requestItem.Routings != null && requestItem.Routings.Any())
            {
                var routingSheet = workbook.Worksheets.Add("Routings");
                var routingHeaders = GetHeadersForRequestType(RequestType.Routing);

                for (var i = 0; i < routingHeaders.Count; i++)
                {
                    routingSheet.Cell(1, i + 1).Value = routingHeaders[i];
                }

                var routingRow = 2;
                foreach (var item in requestItem.Routings)
                {
                    var unitHeaderSeen = 0;
                    for (var i = 0; i < routingHeaders.Count; i++)
                    {
                        var header = routingHeaders[i];
                        if (string.IsNullOrWhiteSpace(header))
                        {
                            routingSheet.Cell(routingRow, i + 1).Value = string.Empty;
                            continue;
                        }

                        object? propertyValue;
                        if (string.Equals(header, "Unit", StringComparison.OrdinalIgnoreCase))
                        {
                            unitHeaderSeen++;
                            // First Unit column is Routing.Unit, second Unit column mirrors Unit/BomUsage display in UI.
                            propertyValue = unitHeaderSeen == 1
                                ? item.Unit
                                : (string.IsNullOrWhiteSpace(item.Unit) ? item.BomUsage : item.Unit);
                        }
                        else if (string.Equals(header, "BomUsage", StringComparison.OrdinalIgnoreCase))
                        {
                            propertyValue = string.IsNullOrWhiteSpace(item.BomUsage) ? "1" : item.BomUsage;
                        }
                        else if (string.Equals(header, "Alternative", StringComparison.OrdinalIgnoreCase))
                        {
                            propertyValue = string.IsNullOrWhiteSpace(item.Alternative) ? "1" : item.Alternative;
                        }
                        else if (string.Equals(header, "ValidTo", StringComparison.OrdinalIgnoreCase))
                        {
                            propertyValue = (item.ValidTo ?? new DateTime(9999, 12, 13)).ToString("yyyy-MM-dd");
                        }
                        else if (string.Equals(header, "MaximumLotSize", StringComparison.OrdinalIgnoreCase))
                        {
                            propertyValue = item.MaximumLotSize ?? 99999999m;
                        }
                        else
                        {
                            propertyValue = GetPropertyByHeader(typeof(Routing), header)?.GetValue(item);
                        }

                        routingSheet.Cell(routingRow, i + 1).Value = propertyValue?.ToString() ?? string.Empty;
                    }

                    routingRow++;
                }

                var routingHeader = routingSheet.Range(1, 1, 1, routingHeaders.Count);
                routingHeader.Style.Font.Bold = true;
                routingHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                routingSheet.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Request_{requestItem.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // User not found, should not happen for authorized users
            }

            var viewModel = new CreateRequestViewModel
            {
                RequesterName = $"{user.FirstName} {user.LastName}",
                Department = user.Department ?? string.Empty,
                Section = user.Section ?? string.Empty,
                RequesterPlant = user.Plant ?? string.Empty,
                Status = "Pending"
            };

            return View(viewModel);
        }

        private void ValidateRequest(CreateRequestViewModel viewModel)
        {
            if (viewModel.RequestType == RequestType.LicensePermission ||
                viewModel.RequestType == RequestType.Routing ||
                viewModel.RequestType == RequestType.Request)
            {
                return;
            }

            // ItemCode: Not required for SM, ToolingB, ToolingB_FG, ToolingB_PU
            if (viewModel.RequestType != RequestType.SM && 
                viewModel.RequestType != RequestType.ToolingB &&
                viewModel.RequestType != RequestType.ToolingB_FG &&
                viewModel.RequestType != RequestType.ToolingB_PU &&
                string.IsNullOrWhiteSpace(viewModel.ItemCode))
            {
                ModelState.AddModelError(nameof(viewModel.ItemCode), "The Item Code field is required for this request type.");
            }

            // DivisionCode & ProfitCenter: Not required for RM
            if (viewModel.RequestType != RequestType.RM)
            {
                if ((viewModel.RequestType == RequestType.SM || viewModel.RequestType == RequestType.FG) && string.IsNullOrWhiteSpace(viewModel.DivisionCode))
                    ModelState.AddModelError(nameof(viewModel.DivisionCode), "Division Code is required for this request type.");

                if ((viewModel.RequestType == RequestType.SM || viewModel.RequestType == RequestType.FG) && string.IsNullOrWhiteSpace(viewModel.ProfitCenter))
                    ModelState.AddModelError(nameof(viewModel.ProfitCenter), "Profit Center is required for this request type.");
            }

            // MrpController & StorageLocation: Exempt for specific types
            if (viewModel.RequestType != RequestType.FG && 
                viewModel.RequestType != RequestType.RM && 
                viewModel.RequestType != RequestType.IPO && 
                viewModel.RequestType != RequestType.ToolingB_FG &&
                viewModel.RequestType != RequestType.ToolingB_PU)
            {
                if (string.IsNullOrWhiteSpace(viewModel.MrpController))
                    ModelState.AddModelError(nameof(viewModel.MrpController), "MRP Controller is required for this request type.");
                if (string.IsNullOrWhiteSpace(viewModel.StorageLocation))
                    ModelState.AddModelError(nameof(viewModel.StorageLocation), "Storage Location is required for this request type.");
            }

            // ProductionSupervisor & CostingLotSize: Exempt for specific types
            if (viewModel.RequestType != RequestType.FG && 
                viewModel.RequestType != RequestType.SM && 
                viewModel.RequestType != RequestType.ToolingB_FG &&
                viewModel.RequestType != RequestType.ToolingB_PU &&
                viewModel.RequestType != RequestType.Passthrough &&
                viewModel.RequestType != RequestType.CrossPlantPurchase)
            {
                if (string.IsNullOrWhiteSpace(viewModel.ProductionSupervisor))
                    ModelState.AddModelError(nameof(viewModel.ProductionSupervisor), "Production Supervisor is required for this request type.");
                if (string.IsNullOrWhiteSpace(viewModel.CostingLotSize))
                    ModelState.AddModelError(nameof(viewModel.CostingLotSize), "Costing Lot Size is required for this request type.");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel, IFormFile? requestAttachment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isITUser = User.IsInRole("IT");

            _logger.LogInformation(
                "Create POST started by {Actor}. RequestType={RequestType}, Requester={Requester}, NextResponsible={NextResponsibleUserId}",
                User?.Identity?.Name ?? "Unknown",
                viewModel.RequestType,
                viewModel.RequesterName,
                viewModel.NextResponsibleUserId);

            // IT users can create requests with partial data and complete details later in Edit.
            if (!isITUser)
            {
                ValidateRequest(viewModel);
            }
            else
            {
                // Plant has [Required] on the view model; allow IT to submit incomplete data in Create.
                ModelState.Remove(nameof(viewModel.Plant));
            }

            if (viewModel.RequestType == RequestType.Request)
            {
                // Plant is not required for the generic Request flow.
                ModelState.Remove(nameof(viewModel.Plant));

                if (requestAttachment == null || requestAttachment.Length == 0)
                {
                    ModelState.AddModelError("requestAttachment", "Attachment file is required for this request type.");
                }

                if (!isITUser && string.IsNullOrWhiteSpace(viewModel.NextResponsibleUserId))
                {
                    ModelState.AddModelError(nameof(viewModel.NextResponsibleUserId), "Please select the next responsible user.");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorCount = ModelState.Values.Sum(v => v.Errors.Count);
                _logger.LogWarning(
                    "Create POST validation failed. ErrorCount={ErrorCount}, RequestType={RequestType}, Requester={Requester}",
                    errorCount,
                    viewModel.RequestType,
                    viewModel.RequesterName);
            }

            if (ModelState.IsValid)
            {
                string? attachmentFileName = null;
                string? attachmentPath = null;
                string? documentNumber = null;
                var requestDateUtc = DateTime.UtcNow;

                if (requestAttachment is { Length: > 0 } uploadedFile)
                {
                    var uploadsRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadsDirectory = Path.Combine(uploadsRoot, "uploads", "requests");
                    Directory.CreateDirectory(uploadsDirectory);

                    var extension = Path.GetExtension(uploadedFile.FileName);
                    var storedFileName = $"{Guid.NewGuid():N}{extension}";
                    var fullFilePath = Path.Combine(uploadsDirectory, storedFileName);

                    await using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }

                    attachmentFileName = uploadedFile.FileName;
                    attachmentPath = $"/uploads/requests/{storedFileName}";

                    _logger.LogInformation(
                        "Create POST received attachment. FileName={FileName}, Size={Size}",
                        uploadedFile.FileName,
                        uploadedFile.Length);
                }

                string? nextApproverId = null;
                if (isITUser && currentUser != null)
                {
                    // Business rule: IT creators route to themselves as next responsible user.
                    nextApproverId = currentUser.Id;
                }
                else if (!string.IsNullOrEmpty(viewModel.NextResponsibleUserId))
                {
                    var parts = viewModel.NextResponsibleUserId.Split('|');
                    if (parts.Length > 0)
                    {
                        nextApproverId = parts[0];
                    }
                }

                if (viewModel.RequestType == RequestType.Request)
                {
                    var yearPart = requestDateUtc.ToString("yy", CultureInfo.InvariantCulture);
                    var prefix = $"SR-{yearPart}-";

                    var existingNumbers = await _context.RequestItems
                        .AsNoTracking()
                        .Where(r => r.DocumentNumber != null && r.DocumentNumber.StartsWith(prefix))
                        .Select(r => r.DocumentNumber!)
                        .ToListAsync();

                    var maxSequence = 0;
                    foreach (var existingNumber in existingNumbers)
                    {
                        var parts = existingNumber.Split('-', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 3 && int.TryParse(parts[2], out var parsedSequence) && parsedSequence > maxSequence)
                        {
                            maxSequence = parsedSequence;
                        }
                    }

                    documentNumber = $"{prefix}{maxSequence + 1:000}";
                }

                 var requestItem = new RequestItem
                {
                    RequestType = viewModel.RequestType.ToString(),
                    Description = viewModel.Description,
                    Requester = viewModel.RequesterName,
                    Status = User.IsInRole("IT") ? viewModel.Status : "Pending",
                    UsageStatus = 1,
                    RequestDate = requestDateUtc,
                    NextApproverId = nextApproverId, // Set the next approver
                    AttachmentFileName = attachmentFileName,
                    AttachmentPath = attachmentPath,
                    DocumentNumber = documentNumber,

                    // Correctly parse and assign values
                    Plant = viewModel.Plant,
                    ItemCode = viewModel.ItemCode,
                    EnglishMatDescription = viewModel.EnglishMatDescription,
                    ModelName = viewModel.ModelName,
                    BaseUnit = viewModel.BaseUnit,
                    MaterialGroup = viewModel.MaterialGroup,
                    ExternalMaterialGroup = viewModel.ExternalMaterialGroup,
                    DivisionCode = viewModel.DivisionCode,
                    ProfitCenter = viewModel.ProfitCenter,
                    DistributionChannel = viewModel.DistributionChannel,
                    BoiCode = viewModel.BoiCode,
                    MrpController = viewModel.MrpController,
                    StorageLocation = viewModel.StorageLocation,
                    ProductionSupervisor = viewModel.ProductionSupervisor,
                    CostingLotSize = int.TryParse(viewModel.CostingLotSize, out var costingLotSize) ? costingLotSize : null,
                    ValClass = viewModel.ValClass,
                    StandardPack = viewModel.StandardPack,
                    BoiDescription = viewModel.BoiDescription,
                    MakerMfrPartNumber = viewModel.MakerMfrPartNumber,
                    CommCodeTariffCode = viewModel.CommCodeTariffCode,
                    TraffCodePercentage = decimal.TryParse(viewModel.TraffCodePercentage, out var traffCodePercentage) ? traffCodePercentage : null,
                    StorageLocationB1 = viewModel.StorageLocationB1,
                    PriceControl = viewModel.PriceControl,
                    Currency = viewModel.Currency,
                    SupplierCode = viewModel.SupplierCode,
                    MatType = viewModel.MatType,
                    Check = viewModel.Check,
                    DevicePlant = viewModel.DevicePlant,
                    AssemblyPlant = viewModel.AssemblyPlant,
                    IpoPlant = viewModel.IpoPlant,
                    AsiOfPlant = viewModel.AsiOfPlant,
                    PriceUnit = int.TryParse(viewModel.PriceUnit, out var priceUnit) ? priceUnit : null,
                    StorageLocationEP = viewModel.StorageLocationEP,
                    ToolingBSection = viewModel.ToolingBSection,
                    PoNumber = viewModel.PoNumber,
                    DateIn = DateTime.TryParse(viewModel.DateIn, out var dateIn) ? dateIn : null,
                    QuotationNumber = viewModel.QuotationNumber,
                    ToolingBModel = viewModel.ToolingBModel,
                    TariffCode = viewModel.TariffCode,
                    Planner = viewModel.Planner,
                    PurchasingGroup = viewModel.PurchasingGroup,
                    EditBomFg = viewModel.EditBomFg,
                    EditBomAllFg = viewModel.EditBomAllFg,
                    Price = decimal.TryParse(viewModel.Price, out var price) ? price : null,

                    BomComponents = (viewModel.Components ?? new List<BomComponentViewModel>()).Select(c => new BomComponent
                    {
                        Level = c.Level,
                        Item = c.Item,
                        ItemCat = c.ItemCat,
                        ComponentNumber = c.ComponentNumber,
                        Description = c.Description,
                        ItemQuantity = c.ItemQuantity,
                        Unit = c.Unit,
                        BomUsage = c.BomUsage,
                        Plant = c.Plant,
                        Sloc = c.Sloc
                    }).ToList(),

                    Routings = (viewModel.Routings ?? new List<RoutingViewModel>()).Select(r => new Routing
                    {
                        Material = r.Material,
                        Description = r.Description,
                        Counter = r.Counter,
                        Plant = r.Plant,
                        WorkCenter = r.WorkCenter,
                        Operation = r.Operation,
                        BaseQty = r.BaseQty,
                        Unit = r.Unit,
                        DirectLaborCosts = r.DirectLaborCosts,
                        DirectExpenses = r.DirectExpenses,
                        AllocationExpense = r.AllocationExpense,
                        ProductionVersionCode = r.ProductionVersionCode,
                        Version = r.Version,
                        ValidFrom = r.ValidFrom,
                        ValidTo = r.ValidTo,
                        MaximumLotSize = r.MaximumLotSize,
                        Alternative = r.Alternative,
                        BomUsage = r.BomUsage,
                        Group = r.Group,
                        GroupCounter = r.GroupCounter
                    }).ToList(),

                    LicensePermissions = (viewModel.LicensePermissions ?? new List<LicensePermissionViewModel>())
                        .Where(lp => !string.IsNullOrWhiteSpace(lp.TCode))
                        .Select(lp => new LicensePermissionItem
                        {
                            SapUsername = lp.SapUsername,
                            TCode = lp.TCode
                        }).ToList()
                };

                _context.Add(requestItem);
                await _context.SaveChangesAsync();

                await AddAuditLogAsync(
                    entityName: nameof(RequestItem),
                    entityId: requestItem.Id.ToString(),
                    action: "Create",
                    details: $"RequestType={requestItem.RequestType}; Status={requestItem.Status}; NextApproverId={requestItem.NextApproverId}");

                _logger.LogInformation(
                    "Create POST succeeded. RequestId={RequestId}, RequestType={RequestType}, Requester={Requester}, NextApproverId={NextApproverId}, DocumentNumber={DocumentNumber}",
                    requestItem.Id,
                    requestItem.RequestType,
                    requestItem.Requester,
                    requestItem.NextApproverId,
                    requestItem.DocumentNumber);

                TempData["SuccessMessage"] = "Request created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id, bool fromImport = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestItem = await _context.RequestItems
                .Include(r => r.BomComponents)
                .Include(r => r.Routings)
                .Include(r => r.LicensePermissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestItem == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserFullName = currentUser == null
                ? string.Empty
                : $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            var isRequesterEditor = !string.IsNullOrWhiteSpace(currentUserFullName)
                && string.Equals(currentUserFullName, requestItem.Requester?.Trim(), StringComparison.OrdinalIgnoreCase);

            // Find the original requester to populate their details
            var requesterUser = await _userManager.Users.FirstOrDefaultAsync(u => (u.FirstName + " " + u.LastName) == requestItem.Requester);
            var currentApproverUser = !string.IsNullOrWhiteSpace(requestItem.NextApproverId)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.Id == requestItem.NextApproverId)
                : null;
            
            var viewModel = new CreateRequestViewModel
            {
                Id = requestItem.Id,
                RequestType = Enum.Parse<RequestType>(requestItem.RequestType, true),
                Description = requestItem.Description,
                RequesterName = requestItem.Requester,
                // Populate from the found user, or leave empty if not found
                Department = requesterUser?.Department ?? "",
                Section = requesterUser?.Section ?? "",
                RequesterPlant = requesterUser?.Plant ?? "",
                Status = requestItem.Status,
                NextResponsibleUserId = requestItem.NextApproverId,

                // Convert model properties back to strings for display
                Plant = requestItem.Plant,
                ItemCode = requestItem.ItemCode,
                EnglishMatDescription = requestItem.EnglishMatDescription,
                ModelName = requestItem.ModelName,
                BaseUnit = requestItem.BaseUnit,
                MaterialGroup = requestItem.MaterialGroup,
                ExternalMaterialGroup = requestItem.ExternalMaterialGroup,
                DivisionCode = requestItem.DivisionCode,
                ProfitCenter = requestItem.ProfitCenter,
                DistributionChannel = requestItem.DistributionChannel,
                BoiCode = requestItem.BoiCode,
                MrpController = requestItem.MrpController,
                StorageLocation = requestItem.StorageLocation,
                ProductionSupervisor = requestItem.ProductionSupervisor,
                CostingLotSize = requestItem.CostingLotSize?.ToString(),
                ValClass = requestItem.ValClass,
                StandardPack = requestItem.StandardPack,
                BoiDescription = requestItem.BoiDescription,
                MakerMfrPartNumber = requestItem.MakerMfrPartNumber,
                CommCodeTariffCode = requestItem.CommCodeTariffCode,
                TraffCodePercentage = requestItem.TraffCodePercentage?.ToString(),
                StorageLocationB1 = requestItem.StorageLocationB1,
                PriceControl = requestItem.PriceControl,
                Currency = requestItem.Currency,
                SupplierCode = requestItem.SupplierCode,
                MatType = requestItem.MatType,
                Check = requestItem.Check,
                DevicePlant = requestItem.DevicePlant,
                AssemblyPlant = requestItem.AssemblyPlant,
                IpoPlant = requestItem.IpoPlant,
                AsiOfPlant = requestItem.AsiOfPlant,
                PriceUnit = requestItem.PriceUnit?.ToString(),
                StorageLocationEP = requestItem.StorageLocationEP,
                ToolingBSection = requestItem.ToolingBSection,
                PoNumber = requestItem.PoNumber,
                DateIn = requestItem.DateIn?.ToString("yyyy-MM-dd"),
                QuotationNumber = requestItem.QuotationNumber,
                ToolingBModel = requestItem.ToolingBModel,
                TariffCode = requestItem.TariffCode,
                Planner = requestItem.Planner,
                PurchasingGroup = requestItem.PurchasingGroup,
                EditBomFg = requestItem.EditBomFg,
                EditBomAllFg = requestItem.EditBomAllFg,
                Price = requestItem.Price?.ToString(),

                Components = requestItem.BomComponents.Select(c => new BomComponentViewModel
                {
                    Id = c.Id,
                    Level = c.Level,
                    Item = c.Item,
                    ItemCat = c.ItemCat,
                    ComponentNumber = c.ComponentNumber,
                    Description = c.Description,
                    ItemQuantity = c.ItemQuantity,
                    Unit = c.Unit,
                    BomUsage = c.BomUsage,
                    Plant = c.Plant,
                    Sloc = c.Sloc
                }).ToList(),

                Routings = requestItem.Routings.Select(r => new RoutingViewModel
                {
                    Id = r.Id,
                    Material = r.Material,
                    Description = r.Description,
                    Counter = r.Counter,
                    Plant = r.Plant,
                    WorkCenter = r.WorkCenter,
                    Operation = r.Operation,
                    BaseQty = r.BaseQty,
                    Unit = r.Unit,
                    DirectLaborCosts = r.DirectLaborCosts,
                    DirectExpenses = r.DirectExpenses,
                    AllocationExpense = r.AllocationExpense,
                    ProductionVersionCode = r.ProductionVersionCode,
                    Version = r.Version,
                    ValidFrom = r.ValidFrom,
                    ValidTo = r.ValidTo,
                    MaximumLotSize = r.MaximumLotSize,
                    Alternative = r.Alternative,
                    BomUsage = r.BomUsage,
                    Group = r.Group,
                    GroupCounter = r.GroupCounter
                }).ToList(),

                LicensePermissions = requestItem.LicensePermissions.Select(lp => new LicensePermissionViewModel
                {
                    SapUsername = lp.SapUsername,
                    TCode = lp.TCode
                }).ToList()
            };

            // Mark viewModel when redirected from import flow
            viewModel.FromImport = fromImport || (TempData["FromImport"] as string) == "true";
            if (viewModel.FromImport)
            {
                ViewBag.ImportNotice = "This request was created from imported data. Some validations may be relaxed for initial save. Please verify fields before finalizing.";
            }

            ViewBag.CurrentNextApproverName = currentApproverUser != null
                ? $"{currentApproverUser.FirstName} {currentApproverUser.LastName}"
                : "Current Responsible User";
            ViewBag.CurrentAttachmentFileName = requestItem.AttachmentFileName;
            ViewBag.CurrentAttachmentPath = requestItem.AttachmentPath;
            ViewBag.IsRequesterEditor = isRequesterEditor;

            return View("Edit", viewModel); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateRequestViewModel viewModel, IFormFile? requestAttachment)
        {
            _logger.LogInformation("Edit POST called for id={Id}", id);
            try { _logger.LogDebug("Posted viewModel: {@ViewModel}", viewModel); } catch { }
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var existingRequest = await _context.RequestItems
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new { r.AttachmentFileName, r.AttachmentPath })
                .FirstOrDefaultAsync();

            if (existingRequest == null)
            {
                return NotFound();
            }

            if (viewModel.RequestType == RequestType.Request && requestAttachment == null && string.IsNullOrWhiteSpace(existingRequest.AttachmentPath))
            {
                ModelState.AddModelError("requestAttachment", "Attachment file is required for this request type.");
            }

            if (viewModel.RequestType == RequestType.Request && string.IsNullOrWhiteSpace(viewModel.NextResponsibleUserId))
            {
                ModelState.AddModelError(nameof(viewModel.NextResponsibleUserId), "Please select the next responsible user.");
            }

            if (User.IsInRole("IT")
                && string.Equals(viewModel.Status, "Rejected", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(viewModel.Description))
            {
                ModelState.AddModelError(nameof(viewModel.Description), "Please provide a remark before rejecting.");
                ViewBag.IsRequesterEditor = false;
                ViewBag.CurrentNextApproverName = "Current Responsible User";
                ViewBag.CurrentAttachmentFileName = existingRequest.AttachmentFileName;
                ViewBag.CurrentAttachmentPath = existingRequest.AttachmentPath;
                return View("Edit", viewModel);
            }

            // Only validate strictly when not coming from import preview
            if (!viewModel.FromImport)
            {
                ValidateRequest(viewModel);
            }

            if (viewModel.FromImport)
            {
                // Clear ModelState errors that commonly come from imported data so user can save and then update later.
                var fieldsToClear = new[] { "ItemCode", "MrpController", "StorageLocation", "ProductionSupervisor", "CostingLotSize", "DivisionCode", "ProfitCenter", "Plant" };
                try
                {
                    var keys = ModelState.Keys.ToList();
                    var matched = keys.Where(k => fieldsToClear.Any(f => k.EndsWith(f, StringComparison.OrdinalIgnoreCase))).ToList();
                    foreach (var k in matched)
                    {
                        ModelState[k].Errors.Clear();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear ModelState errors for imported record id={Id}", id);
                }
                _logger.LogInformation("Edit POST: relaxed validation for imported record id={Id}", id);
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var errors = ModelState.Where(ms => ms.Value.Errors.Any())
                        .Select(ms => new
                        {
                            Key = ms.Key,
                            Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToArray();
                    _logger.LogWarning("ModelState invalid on Edit POST: {@Errors}", errors);
                }
                catch
                {
                }

                var requestItemForView = await _context.RequestItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserFullName = currentUser == null
                    ? string.Empty
                    : $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                var isRequesterEditor = requestItemForView != null
                    && !string.IsNullOrWhiteSpace(currentUserFullName)
                    && string.Equals(currentUserFullName, requestItemForView.Requester?.Trim(), StringComparison.OrdinalIgnoreCase);

                var currentApproverUser = requestItemForView != null && !string.IsNullOrWhiteSpace(requestItemForView.NextApproverId)
                    ? await _userManager.Users.FirstOrDefaultAsync(u => u.Id == requestItemForView.NextApproverId)
                    : null;

                ViewBag.CurrentNextApproverName = currentApproverUser != null
                    ? $"{currentApproverUser.FirstName} {currentApproverUser.LastName}"
                    : "Current Responsible User";
                ViewBag.IsRequesterEditor = isRequesterEditor;
                ViewBag.CurrentAttachmentFileName = requestItemForView?.AttachmentFileName ?? existingRequest.AttachmentFileName;
                ViewBag.CurrentAttachmentPath = requestItemForView?.AttachmentPath ?? existingRequest.AttachmentPath;

                return View("Edit", viewModel);
            }

           // if (!ModelState.IsValid)
            //{
            //    // Log validation errors to help debug why the form can't be saved
            //    try
            //    {
            //        var errors = ModelState.Where(ms => ms.Value.Errors.Any()).Select(ms => new { Key = ms.Key, Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray() }).ToArray();
            //        _logger.LogWarning("ModelState invalid on Edit POST: {@Errors}", errors);
            //    }
            //    catch { }
            //    return View(viewModel);
            //}

            // proceed when valid
            
           // if (ModelState.IsValid)
           {

                 var requestItemToUpdate = await _context.RequestItems
                    .Include(r => r.BomComponents)
                    .Include(r => r.Routings)
                          .Include(r => r.LicensePermissions)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (requestItemToUpdate == null)
                {
                    return NotFound();
                }

                try
                {
                    var previousStatus = requestItemToUpdate.Status;
                    var previousNextApproverId = requestItemToUpdate.NextApproverId;
                    var currentUser = await _userManager.GetUserAsync(User);
                    var currentUserFullName = currentUser == null
                        ? string.Empty
                        : $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                    var isRequesterEditor = !string.IsNullOrWhiteSpace(currentUserFullName)
                        && string.Equals(currentUserFullName, requestItemToUpdate.Requester?.Trim(), StringComparison.OrdinalIgnoreCase);

                    // Update scalar properties from the view model
                    if (isRequesterEditor)
                    {
                        requestItemToUpdate.RequestType = viewModel.RequestType.ToString();
                        requestItemToUpdate.Plant = viewModel.Plant;
                    }
                    requestItemToUpdate.Description = viewModel.Description;
                    requestItemToUpdate.Status = User.IsInRole("IT") ? viewModel.Status : requestItemToUpdate.Status;
                    requestItemToUpdate.UpdatedAt = DateTime.UtcNow;
                    requestItemToUpdate.UpdatedBy = User?.Identity?.Name ?? "Unknown";

                    if (requestAttachment is { Length: > 0 } uploadedFile)
                    {
                        var uploadsRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadsDirectory = Path.Combine(uploadsRoot, "uploads", "requests");
                        Directory.CreateDirectory(uploadsDirectory);

                        var extension = Path.GetExtension(uploadedFile.FileName);
                        var storedFileName = $"{Guid.NewGuid():N}{extension}";
                        var fullFilePath = Path.Combine(uploadsDirectory, storedFileName);

                        await using (var stream = new FileStream(fullFilePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(stream);
                        }

                        requestItemToUpdate.AttachmentFileName = uploadedFile.FileName;
                        requestItemToUpdate.AttachmentPath = $"/uploads/requests/{storedFileName}";
                    }

                    if (!string.IsNullOrWhiteSpace(viewModel.NextResponsibleUserId))
                    {
                        var nextApproverParts = viewModel.NextResponsibleUserId.Split('|');
                        requestItemToUpdate.NextApproverId = nextApproverParts.Length > 0 ? nextApproverParts[0] : viewModel.NextResponsibleUserId;
                    }
                    requestItemToUpdate.ItemCode = viewModel.ItemCode;
                    requestItemToUpdate.EnglishMatDescription = viewModel.EnglishMatDescription;
                    requestItemToUpdate.ModelName = viewModel.ModelName;
                    requestItemToUpdate.BaseUnit = viewModel.BaseUnit;
                    requestItemToUpdate.MaterialGroup = viewModel.MaterialGroup;
                    requestItemToUpdate.ExternalMaterialGroup = viewModel.ExternalMaterialGroup;
                    requestItemToUpdate.DivisionCode = viewModel.DivisionCode;
                    requestItemToUpdate.ProfitCenter = viewModel.ProfitCenter;
                    requestItemToUpdate.DistributionChannel = viewModel.DistributionChannel;
                    requestItemToUpdate.BoiCode = viewModel.BoiCode;
                    requestItemToUpdate.MrpController = viewModel.MrpController;
                    requestItemToUpdate.StorageLocation = viewModel.StorageLocation;
                    requestItemToUpdate.ProductionSupervisor = viewModel.ProductionSupervisor;
                    requestItemToUpdate.CostingLotSize = int.TryParse(viewModel.CostingLotSize, out var costingLotSize) ? costingLotSize : null;
                    requestItemToUpdate.ValClass = viewModel.ValClass;
                    requestItemToUpdate.StandardPack = viewModel.StandardPack;
                    requestItemToUpdate.BoiDescription = viewModel.BoiDescription;
                    requestItemToUpdate.MakerMfrPartNumber = viewModel.MakerMfrPartNumber;
                    requestItemToUpdate.CommCodeTariffCode = viewModel.CommCodeTariffCode;
                    requestItemToUpdate.TraffCodePercentage = decimal.TryParse(viewModel.TraffCodePercentage, out var traffCodePercentage) ? traffCodePercentage : null;
                    requestItemToUpdate.StorageLocationB1 = viewModel.StorageLocationB1;
                    requestItemToUpdate.PriceControl = viewModel.PriceControl;
                    requestItemToUpdate.Currency = viewModel.Currency;
                    requestItemToUpdate.SupplierCode = viewModel.SupplierCode;
                    requestItemToUpdate.MatType = viewModel.MatType;
                    requestItemToUpdate.Check = viewModel.Check;
                    requestItemToUpdate.DevicePlant = viewModel.DevicePlant;
                    requestItemToUpdate.AssemblyPlant = viewModel.AssemblyPlant;
                    requestItemToUpdate.IpoPlant = viewModel.IpoPlant;
                    requestItemToUpdate.AsiOfPlant = viewModel.AsiOfPlant;
                    requestItemToUpdate.PriceUnit = int.TryParse(viewModel.PriceUnit, out var priceUnit) ? priceUnit : null;
                    requestItemToUpdate.StorageLocationEP = viewModel.StorageLocationEP;
                    requestItemToUpdate.ToolingBSection = viewModel.ToolingBSection;
                    requestItemToUpdate.PoNumber = viewModel.PoNumber;
                    requestItemToUpdate.DateIn = DateTime.TryParse(viewModel.DateIn, out var dateIn) ? dateIn : null;
                    requestItemToUpdate.QuotationNumber = viewModel.QuotationNumber;
                    requestItemToUpdate.ToolingBModel = viewModel.ToolingBModel;
                    requestItemToUpdate.TariffCode = viewModel.TariffCode;
                    requestItemToUpdate.Planner = viewModel.Planner;
                    requestItemToUpdate.PurchasingGroup = viewModel.PurchasingGroup;
                    requestItemToUpdate.EditBomFg = viewModel.EditBomFg;
                    requestItemToUpdate.EditBomAllFg = viewModel.EditBomAllFg;
                    requestItemToUpdate.Price = decimal.TryParse(viewModel.Price, out var price) ? price : null;

                    // Remove existing related entities and add updated ones
                    _context.BomComponents.RemoveRange(requestItemToUpdate.BomComponents);
                    _context.Routings.RemoveRange(requestItemToUpdate.Routings);
                    _context.LicensePermissionItems.RemoveRange(requestItemToUpdate.LicensePermissions);
                    
                    requestItemToUpdate.BomComponents = (viewModel.Components ?? new List<BomComponentViewModel>()).Select(c => new BomComponent
                    {
                        Level = c.Level,
                        Item = c.Item,
                        ItemCat = c.ItemCat,
                        ComponentNumber = c.ComponentNumber,
                        Description = c.Description,
                        ItemQuantity = c.ItemQuantity,
                        Unit = c.Unit,
                        BomUsage = c.BomUsage,
                        Plant = c.Plant,
                        Sloc = c.Sloc
                    }).ToList();

                    requestItemToUpdate.Routings = (viewModel.Routings ?? new List<RoutingViewModel>()).Select(r => new Routing
                    {
                        Material = r.Material,
                        Description = r.Description,
                        Counter = r.Counter,
                        Plant = r.Plant,
                        WorkCenter = r.WorkCenter,
                        Operation = r.Operation,
                        BaseQty = r.BaseQty,
                        Unit = r.Unit,
                        DirectLaborCosts = r.DirectLaborCosts,
                        DirectExpenses = r.DirectExpenses,
                        AllocationExpense = r.AllocationExpense,
                        ProductionVersionCode = r.ProductionVersionCode,
                        Version = r.Version,
                        ValidFrom = r.ValidFrom,
                        ValidTo = r.ValidTo,
                        MaximumLotSize = r.MaximumLotSize,
                        Alternative = r.Alternative,
                        BomUsage = r.BomUsage,
                        Group = r.Group,
                        GroupCounter = r.GroupCounter
                    }).ToList();

                    requestItemToUpdate.LicensePermissions = (viewModel.LicensePermissions ?? new List<LicensePermissionViewModel>())
                        .Where(lp => !string.IsNullOrWhiteSpace(lp.TCode))
                        .Select(lp => new LicensePermissionItem
                        {
                            SapUsername = lp.SapUsername,
                            TCode = lp.TCode
                        }).ToList();

                    await _context.SaveChangesAsync();

                    await AddAuditLogAsync(
                        entityName: nameof(RequestItem),
                        entityId: requestItemToUpdate.Id.ToString(),
                        action: "Update",
                        details: $"RequestType={requestItemToUpdate.RequestType}; Status:{previousStatus}->{requestItemToUpdate.Status}; NextApproverId:{previousNextApproverId}->{requestItemToUpdate.NextApproverId}");

                    _logger.LogInformation(
                        "Edit POST succeeded for RequestId={RequestId} by {Actor}. RequestType={RequestType}, Status={Status}, NextApproverId={NextApproverId}",
                        requestItemToUpdate.Id,
                        User?.Identity?.Name ?? "Unknown",
                        requestItemToUpdate.RequestType,
                        requestItemToUpdate.Status,
                        requestItemToUpdate.NextApproverId);

                    TempData["SuccessMessage"] = "Request updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Edit POST concurrency conflict for RequestId={RequestId}", viewModel.Id);
                    if (!RequestItemExists(viewModel.Id))
                    {
                        _logger.LogWarning("Edit POST failed because RequestId={RequestId} was not found", viewModel.Id);
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestItem = await _context.RequestItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requestItem == null)
            {
                return NotFound();
            }

            return View(requestItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST started for RequestId={RequestId} by {Actor}", id, User?.Identity?.Name ?? "Unknown");

            var requestItem = await _context.RequestItems.FindAsync(id);
            if (requestItem == null)
            {
                _logger.LogWarning("Delete POST failed because RequestId={RequestId} was not found", id);
                TempData["ErrorMessage"] = "Request not found.";
                return NotFound();
            }

            requestItem.UsageStatus = 9;
            requestItem.UpdatedAt = DateTime.UtcNow;
            requestItem.UpdatedBy = User?.Identity?.Name ?? "Unknown";
            await _context.SaveChangesAsync();

            await AddAuditLogAsync(
                entityName: nameof(RequestItem),
                entityId: requestItem.Id.ToString(),
                action: "SoftDelete",
                details: $"RequestType={requestItem.RequestType}; Requester={requestItem.Requester}; Status={requestItem.Status}; UsageStatus=9");

            _logger.LogInformation(
                "Delete POST succeeded for RequestId={RequestId}. RequestType={RequestType}, Requester={Requester}",
                requestItem.Id,
                requestItem.RequestType,
                requestItem.Requester);

            TempData["SuccessMessage"] = "Request removed from active list successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool RequestItemExists(int id)
        {
            return _context.RequestItems.Any(e => e.Id == id);
        }

        private async Task AddAuditLogAsync(string entityName, string? entityId, string action, string? details)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    PerformedBy = User?.Identity?.Name ?? "Unknown",
                    PerformedAt = DateTime.UtcNow,
                    Details = details
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist audit log. Entity={EntityName}, EntityId={EntityId}, Action={Action}", entityName, entityId, action);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImported(List<RequestItem> requests, string? serializedRequests)
        {
            if ((requests == null || !requests.Any()) && !string.IsNullOrWhiteSpace(serializedRequests))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    requests = JsonSerializer.Deserialize<List<RequestItem>>(serializedRequests, options) ?? new List<RequestItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize import preview payload");
                    requests = new List<RequestItem>();
                }
            }

            if (requests == null || !requests.Any())
            {
                TempData["ErrorMessage"] = "No import data to save.";
                return RedirectToAction(nameof(Create));
            }

            List<RequestItem> serializedRequestSnapshot = new();
            if (!string.IsNullOrWhiteSpace(serializedRequests))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    serializedRequestSnapshot = JsonSerializer.Deserialize<List<RequestItem>>(serializedRequests, options) ?? new List<RequestItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize snapshot for SaveImported fallback");
                }
            }

            for (int i = 0; i < requests.Count && i < serializedRequestSnapshot.Count; i++)
            {
                var posted = requests[i];
                var source = serializedRequestSnapshot[i];

                if (string.IsNullOrWhiteSpace(posted.NextApproverId))
                {
                    posted.NextApproverId = source.NextApproverId;
                }

                if (string.IsNullOrWhiteSpace(posted.RequestType))
                {
                    posted.RequestType = source.RequestType;
                }

                if (string.IsNullOrWhiteSpace(posted.Requester))
                {
                    posted.Requester = source.Requester;
                }
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Ensure required metadata
            foreach (var r in requests)
            {
                r.Requester = r.Requester ?? $"{user.FirstName} {user.LastName}";
                r.Status = r.Status ?? "Pending";
                r.UsageStatus = r.UsageStatus == 0 ? 1 : r.UsageStatus;
                r.RequestDate = r.RequestDate == default ? DateTime.UtcNow : r.RequestDate;
            }

            // If form posted plant values didn't bind to model, read them explicitly from Request.Form
            try
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    var key = $"requests[{i}].Plant";
                    if (Request.Form.ContainsKey(key))
                    {
                        var postedPlant = Request.Form[key].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(postedPlant))
                        {
                            requests[i].Plant = postedPlant;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read plant values from Request.Form during import save");
            }

            // Log incoming parsed requests for debugging (plant values, component counts)
            try
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    var rr = requests[i];
                    _logger.LogInformation("Import save incoming request index={Index} plant={Plant} components={Count}", i, rr.Plant, rr.BomComponents?.Count ?? 0);
                }
            }
            catch { }

            // If a request has a Plant value, propagate it to all BOM components for that request
            foreach (var r in requests)
            {
                // If request-level Plant is empty, try to take from first component
                if (string.IsNullOrWhiteSpace(r.Plant) && r.BomComponents != null && r.BomComponents.Any())
                {
                    r.Plant = r.BomComponents.First().Plant;
                }

                if (!string.IsNullOrWhiteSpace(r.Plant) && r.BomComponents != null)
                {
                    foreach (var c in r.BomComponents)
                    {
                        // Overwrite component Plant with request-level Plant
                        c.Plant = r.Plant;
                    }
                }
            }

            _context.RequestItems.AddRange(requests);
            await _context.SaveChangesAsync();

            // Log saved items for debug
            try
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    var r = requests[i];
                    _logger.LogInformation("Saved imported request index {Index} id {Id} plant={Plant} components={Count}", i, r.Id, r.Plant, r.BomComponents?.Count ?? 0);
                }
            }
            catch { }

            TempData["SuccessMessage"] = $"{requests.Count} requests have been saved.";

            // Redirect to Edit of the first saved request so user can verify and edit fields
            var firstId = requests.FirstOrDefault()?.Id;
            if (firstId != null && firstId > 0)
            {
                TempData["FromImport"] = "true";
                return RedirectToAction(nameof(Edit), new { id = firstId, fromImport = true });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, string NextResponsibleUserId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Create));
            }

            if (string.IsNullOrEmpty(NextResponsibleUserId))
            {
                TempData["ErrorMessage"] = "Please select the next responsible user before importing.";
                return RedirectToAction(nameof(Create));
            }

            string? nextApproverId = null;
            var parts = NextResponsibleUserId.Split('|');
            if (parts.Length > 0)
            {
                nextApproverId = parts[0];
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var fileName = file.FileName;
            var requestTypeString = fileName.Split('_').FirstOrDefault();
            if (string.IsNullOrEmpty(requestTypeString) || !Enum.TryParse<RequestType>(requestTypeString, true, out var requestType))
            {
                TempData["ErrorMessage"] = "Invalid file name format. Expected format: [RequestType]_Template.xlsx";
                return RedirectToAction(nameof(Create));
            }

         //   if (requestType == RequestType.BOM || requestType == RequestType.Routing)
         //   {
         //       TempData["ErrorMessage"] = "Import for BOM and Routing request types is not supported yet.";
         //       return RedirectToAction(nameof(Create));
         //   }

            var newRequests = new List<RequestItem>();
            var parsedHeaders = new List<string>();
            int parsedDataRows = 0;
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "The Excel file is empty or invalid.";
                            return RedirectToAction(nameof(Create));
                        }

                        var lastRowUsed = worksheet.LastRowUsed();
                        if (lastRowUsed == null) {
                             TempData["ErrorMessage"] = "The Excel file does not contain any data rows.";
                            return RedirectToAction(nameof(Create));
                        }

                        var lastRowNumber = lastRowUsed.RowNumber();

                        var headerRow = worksheet.Row(1);
                        var headerMap = new Dictionary<string, int>();
                        foreach (var cell in headerRow.CellsUsed())
                        {
                            if(cell == null) continue;

                            var headerText = cell.GetString()?.Trim();
                            if (!string.IsNullOrEmpty(headerText))
                            {
                                if (!headerMap.ContainsKey(headerText))
                                {
                                    headerMap[headerText] = cell.Address.ColumnNumber;
                                }
                            }
                        }

                        parsedHeaders = headerMap.Keys.ToList();
                        _logger.LogInformation("Import file {FileName}: RequestType={RequestType}, ParsedHeaders={Headers}, LastRow={LastRow}", fileName, requestType, string.Join(",", parsedHeaders), lastRowNumber);

                        // Log first few data rows for debugging
                        try
                        {
                            var previewRows = new List<string>();
                            for (int r = 2; r <= Math.Min(lastRowNumber, 6); r++)
                            {
                                var row = worksheet.Row(r);
                                if (row == null) continue;
                                var values = headerMap.Values.Select(c => row.Cell(c).GetFormattedString()).ToArray();
                                if (values.All(v => string.IsNullOrWhiteSpace(v))) continue;
                                previewRows.Add($"Row {r}: " + string.Join(" | ", values));
                            }
                            if (previewRows.Any())
                                _logger.LogInformation("Import preview rows:\n{Preview}", string.Join("\n", previewRows));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to build import preview rows");
                        }

                        if (requestType == RequestType.BOM)
                        {
                            var components = new List<BomComponent>();
                            var headerColumns = headerMap.Values.ToList();

                            for (int rowNum = 2; rowNum <= lastRowNumber; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);
                                if (row == null)
                                    continue;

                                // skip completely empty rows
                                bool hasData = headerColumns.Any(col => !string.IsNullOrWhiteSpace(row.Cell(col).GetFormattedString()));
                                if (!hasData)
                                    continue;

                                var component = new BomComponent();
                                foreach (var header in headerMap)
                                {
                                    var cellValue = row.Cell(header.Value).GetFormattedString();
                                    SetBomComponentProperty(component, header.Key, cellValue);
                                }

                                components.Add(component);
                            }

                            parsedDataRows = components.Count;

                            if (components.Any())
                            {
                                var requestItem = new RequestItem
                                {
                                    RequestType = requestType.ToString(),
                                    Requester = $"{user.FirstName} {user.LastName}",
                                    Status = "Pending",
                                    RequestDate = DateTime.UtcNow,
                                    NextApproverId = nextApproverId,
                                    Description = $"Imported {requestTypeString} BOM data on {DateTime.UtcNow.ToShortDateString()}",
                                    BomComponents = components
                                };

                                newRequests.Add(requestItem);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "The file was processed but no BOM component rows were found. Please check the template and data.";
                            }
                        }
                        else if (requestType == RequestType.EditBOM)
                        {
                            var components = new List<BomEditComponent>();
                            var headerColumns = headerMap.Values.ToList();

                            for (int rowNum = 2; rowNum <= lastRowNumber; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);
                                if (row == null)
                                    continue;

                                bool hasData = headerColumns.Any(col => !string.IsNullOrWhiteSpace(row.Cell(col).GetFormattedString()));
                                if (!hasData)
                                    continue;

                                var component = new BomEditComponent();
                                foreach (var header in headerMap)
                                {
                                    var cellValue = row.Cell(header.Value).GetFormattedString();
                                    SetBomEditComponentProperty(component, header.Key, cellValue);
                                }

                                components.Add(component);
                            }

                            parsedDataRows = components.Count;

                            if (components.Any())
                            {
                                var requestItem = new RequestItem
                                {
                                    RequestType = requestType.ToString(),
                                    Requester = $"{user.FirstName} {user.LastName}",
                                    Status = "Pending",
                                    RequestDate = DateTime.UtcNow,
                                    NextApproverId = nextApproverId,
                                    Description = $"Imported {requestTypeString} data on {DateTime.UtcNow.ToShortDateString()}",
                                    bomEditComponents = components
                                };

                                newRequests.Add(requestItem);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "The file was processed but no Edit BOM rows were found. Please check the template and data.";
                            }
                        }
                        else if (requestType == RequestType.Routing)
                        {
                            var routings = new List<Routing>();
                            var headerColumns = headerMap.Values.ToList();

                            for (int rowNum = 2; rowNum <= lastRowNumber; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);
                                if (row == null)
                                    continue;

                                bool hasData = headerColumns.Any(col => !string.IsNullOrWhiteSpace(row.Cell(col).GetFormattedString()));
                                if (!hasData)
                                    continue;

                                var routing = new Routing();
                                foreach (var header in headerMap)
                                {
                                    var cellValue = row.Cell(header.Value).GetFormattedString();
                                    SetRoutingProperty(routing, header.Key, cellValue);
                                }

                                // Import rule: Allocation Expense follows Direct Expenses.
                                routing.AllocationExpense = routing.DirectExpenses;

                                if (!routing.ValidTo.HasValue)
                                {
                                    routing.ValidTo = new DateTime(9999, 12, 13);
                                }

                                if (!routing.MaximumLotSize.HasValue)
                                {
                                    routing.MaximumLotSize = 99999999m;
                                }

                                if (string.IsNullOrWhiteSpace(routing.BomUsage))
                                {
                                    routing.BomUsage = "1";
                                }

                                routings.Add(routing);
                            }

                            parsedDataRows = routings.Count;

                            if (routings.Any())
                            {
                                var requestItem = new RequestItem
                                {
                                    RequestType = requestType.ToString(),
                                    Requester = $"{user.FirstName} {user.LastName}",
                                    Status = "Pending",
                                    RequestDate = DateTime.UtcNow,
                                    NextApproverId = nextApproverId,
                                    Description = $"Imported {requestTypeString} routing data on {DateTime.UtcNow.ToShortDateString()}",
                                    Plant = routings.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.Plant))?.Plant,
                                    Routings = routings
                                };

                                newRequests.Add(requestItem);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "The file was processed but no Routing rows were found. Please check the template and data.";
                            }
                        }
                        else
                        {
                            for (int rowNum = 2; rowNum <= lastRowNumber; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);
                                if (row == null) continue;

                                // skip completely empty rows
                                bool hasData = headerMap.Values.Any(col => !string.IsNullOrWhiteSpace(row.Cell(col).GetFormattedString()));
                                if (!hasData) continue;

                                var requestItem = new RequestItem
                                {
                                    RequestType = requestType.ToString(),
                                    Requester = $"{user.FirstName} {user.LastName}",
                                    Status = "Pending",
                                    RequestDate = DateTime.UtcNow,
                                    NextApproverId = nextApproverId,
                                    Description = $"Imported {requestTypeString} data on {DateTime.UtcNow.ToShortDateString()}"
                                };

                                foreach (var header in headerMap)
                                {
                                    var cellValue = row.Cell(header.Value).GetFormattedString();
                                    SetProperty(requestItem, header.Key, cellValue);
                                }
                                newRequests.Add(requestItem);
                                parsedDataRows++;
                            }
                        }
                    }
                }

                if (newRequests.Any())
                {
                    // Before saving, ensure required fields have sensible defaults so edit can proceed
                    foreach (var nr in newRequests)
                    {
                        // Ensure Plant is set
                        if (string.IsNullOrWhiteSpace(nr.Plant))
                        {
                            nr.Plant = nr.BomComponents?.FirstOrDefault()?.Plant ?? "";
                        }

                        // Ensure MRP Controller and StorageLocation have defaults to avoid ValidateRequest failures
                        if (string.IsNullOrWhiteSpace(nr.MrpController)) nr.MrpController = "";
                        if (string.IsNullOrWhiteSpace(nr.StorageLocation)) nr.StorageLocation = "";

                        // CostingLotSize as string handled in ViewModel; set to null/empty
                        // ProductionSupervisor default empty
                        if (string.IsNullOrWhiteSpace(nr.ProductionSupervisor)) nr.ProductionSupervisor = "";
                    }

                    // Show preview page before saving
                    return View("PreviewImport", newRequests);
                }
                else
                {
                    // Provide more debugging info to help understand why nothing was imported
                    var headerInfo = parsedHeaders != null && parsedHeaders.Any() ? string.Join(", ", parsedHeaders) : "(no headers parsed)";
                    TempData["ErrorMessage"] = $"The file was processed, but no valid requests were found to import. Parsed headers: {headerInfo}. Data rows detected: {parsedDataRows}.";
                    return RedirectToAction(nameof(Create));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Excel file.");
                TempData["ErrorMessage"] = "An error occurred while processing the file. Please ensure the data format is correct and check the logs for details.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void SetProperty(RequestItem item, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return; // Do not set property if cell is empty
            }
            try
            {
                var propertyInfo = GetPropertyByHeader(typeof(RequestItem), propertyName);
                if (propertyInfo == null || !propertyInfo.CanWrite)
                    return;

                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                object convertedValue;
                if (targetType == typeof(decimal))
                    convertedValue = decimal.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(int))
                    convertedValue = int.Parse(value, CultureInfo.InvariantCulture);
                else if (targetType == typeof(bool))
                    convertedValue = bool.Parse(value);
                else if (targetType == typeof(DateTime))
                    convertedValue = DateTime.Parse(value, CultureInfo.InvariantCulture);
                else
                    convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                propertyInfo.SetValue(item, convertedValue, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set property {PropertyName} with value {Value}", propertyName, value);
            }
        }

        private System.Reflection.PropertyInfo? GetPropertyByHeader(Type targetType, string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return null;
            string Normalize(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

            var normalizedHeader = Normalize(header);
            var props = targetType.GetProperties();

            // First try exact match (case-insensitive)
            var exact = props.FirstOrDefault(p => string.Equals(p.Name, header, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            // Otherwise match by normalized name (remove spaces/punctuation)
            var byNormalized = props.FirstOrDefault(p => Normalize(p.Name) == normalizedHeader);
            if (byNormalized != null) return byNormalized;

            // As a fallback, try to find property where normalized header is substring of property name
            return props.FirstOrDefault(p => Normalize(p.Name).Contains(normalizedHeader) || normalizedHeader.Contains(Normalize(p.Name)));
        }
        
        [HttpGet]
        public async Task<IActionResult> GetNextApprovers(RequestType requestType, string? plant, string? requesterName, string? currentApproverId)
        {
            var requestTypeName = requestType.ToString();
            var normalizedPlant = (plant ?? string.Empty).Trim();
            var routings = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section) // Include Section data
                .Include(dr => dr.Plant)
                .Where(dr => dr.DocumentType.Name == requestTypeName)
                .Where(dr => string.IsNullOrWhiteSpace(normalizedPlant)
                    || dr.Plant.PlantName == normalizedPlant
                    || dr.Plant.PlantName.StartsWith(normalizedPlant + " ")
                    || dr.Plant.PlantName.StartsWith(normalizedPlant + "("))
                .OrderBy(dr => dr.Step)
                .ThenBy(dr => dr.Id)
                .ToListAsync();

            if (!routings.Any())
            {
                return Json(new List<object>());
            }

            var allUsers = await _userManager.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();
            var stepCandidates = new List<(int Step, int RoutingId, string Rule, List<ApplicationUser> Users)>();

            foreach (var stepRouting in routings)
            {
                var departmentName = stepRouting.Department?.DepartmentName;
                var sectionName = stepRouting.Section?.SectionName;

                var usersInRule = allUsers.AsQueryable();

                if (!string.IsNullOrEmpty(departmentName))
                {
                    usersInRule = usersInRule.Where(u => !string.IsNullOrEmpty(u.Department) && 
                                                       u.Department.Trim().Equals(departmentName.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(sectionName))
                {
                     usersInRule = usersInRule.Where(u => !string.IsNullOrEmpty(u.Section) && 
                                                        u.Section.Trim().Equals(sectionName.Trim(), StringComparison.OrdinalIgnoreCase));
                }
                
                var foundUsers = usersInRule.ToList();

                if (foundUsers.Any())
                {
                    var ruleForDisplay = $"Step {stepRouting.Step}: {departmentName}" + 
                                         (string.IsNullOrEmpty(sectionName) ? "" : $" / {sectionName}");

                    stepCandidates.Add((stepRouting.Step, stepRouting.Id, ruleForDisplay, foundUsers));
                }
            }

            if (!stepCandidates.Any())
            {
                return Json(new List<object>());
            }

            var availableSteps = stepCandidates
                .Select(c => c.Step)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            var currentUser = await _userManager.GetUserAsync(User);
            int currentUserStep = 0;

            // Prefer the routing id from current approver selection (format: userId|routingId)
            // to identify the exact current step in the workflow.
            if (!string.IsNullOrWhiteSpace(currentApproverId))
            {
                var idParts = currentApproverId.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (idParts.Length == 2 && int.TryParse(idParts[1], out var currentRoutingId))
                {
                    currentUserStep = stepCandidates
                        .Where(c => c.RoutingId == currentRoutingId)
                        .Select(c => c.Step)
                        .DefaultIfEmpty(0)
                        .First();
                }
            }

            if (currentUserStep == 0 && currentUser != null)
            {
                // If exact routing id is unavailable, use the earliest matched step
                // to avoid skipping intermediate steps (e.g., step 2).
                currentUserStep = stepCandidates
                    .Where(c => c.Users.Any(u => u.Id == currentUser.Id))
                    .Select(c => c.Step)
                    .DefaultIfEmpty(0)
                    .Min();
            }

            // Show only the immediate next step.
            // - Create flow (unknown current step): start at step 1.
            // - Edit flow by current approver: show the next step after current.
            int targetStep = currentUserStep == 0
                ? availableSteps.First()
                : availableSteps.FirstOrDefault(s => s > currentUserStep);

            if (targetStep == 0)
            {
                return Json(new List<object>());
            }

            var nextStepApprovers = stepCandidates
                .Where(c => c.Step == targetStep)
                .SelectMany(c => c.Users.Select(u => new
                {
                    Id = $"{u.Id}|{c.RoutingId}",
                    Step = c.Step,
                    Rule = c.Rule,
                    FullName = $"{u.FirstName} {u.LastName}"
                }))
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderBy(a => a.Step)
                .ThenBy(a => a.FullName)
                .ToList();

            return Json(nextStepApprovers);
        }

        [HttpGet]
        public IActionResult DownloadTemplate(RequestType requestType)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                // Add headers based on RequestType
                var headers = GetHeadersForRequestType(requestType);
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                // Style the header
                var headerRange = worksheet.Range(1, 1, 1, headers.Count);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                try
                {
                    worksheet.Columns().AdjustToContents();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-size Requests template columns. Returning default column widths.");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = $"{requestType}_Template.xlsx";
                    return File(content, contentType, fileName);
                }
            }
        }

        private List<string> GetHeadersForRequestType(RequestType requestType)
        {
            var headers = new List<string>();

            switch (requestType)
            {
                case RequestType.FG:
                    headers.AddRange(new[] 
                    {
                        "Plant", "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "MaterialGroup",
                        "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "DistributionChannel", "BoiCode",
                        "MrpController", "StorageLocation", "ProductionSupervisor", "CostingLotSize", "ValClass"
                    });
                    break;
                case RequestType.SM:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "EnglishMatDescription", "BaseUnit", "Plant", "MaterialGroup", "DivisionCode",
                        "ProfitCenter", "MrpController", "StorageLocation", "ProductionSupervisor", "CostingLotSize", "StandardPack"
                    });
                    break;
                case RequestType.RM:
                     headers.AddRange(new[] 
                    {
                        "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "BoiDescription", "Plant",
                        "MaterialGroup", "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "PurchasingGroup",
                        "MakerMfrPartNumber", "CommCodeTariffCode", "TraffCodePercentage", "MrpController",
                        "StorageLocation", "StorageLocationB1", "PriceControl", "ValClass", "Price", "Currency",
                        "CostingLotSize", "SupplierCode"
                    });
                    break;
                case RequestType.Passthrough:
                case RequestType.CrossPlantPurchase:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "BoiDescription", "Plant",
                        "MaterialGroup", "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "PurchasingGroup",
                        "MakerMfrPartNumber", "CommCodeTariffCode", "TraffCodePercentage", "MrpController",
                        "StorageLocation", "PriceControl", "ValClass", "Price", "SupplierCode"
                    });
                    break;
                case RequestType.ToolingB:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "MatType", "Check", "EnglishMatDescription", "MaterialGroup", "BaseUnit",
                        "ExternalMaterialGroup", "Plant", "DevicePlant", "AssemblyPlant", "IpoPlant", "AsiOfPlant",
                        "PurchasingGroup", "DivisionCode", "ProfitCenter", "Price", "PriceUnit", "StorageLocationEP",
                        "ToolingBModel", "ToolingBSection", "PoNumber", "StatusInA", "DateIn", "QuotationNumber"
                    });
                    break;
                case RequestType.ToolingB_FG:
                    headers.AddRange(new[] 
                    {
                        "CurrentICS", "ItemCode", "EnglishMatDescription", "Level", "Rohs", "MaterialGroup", "BaseUnit",
                        "CodenMid", "DevicePlant", "AssemblyPlant", "IpoPlant", "AsiOfPlant", "Plant", "SalesOrg",
                        "DistributionChannel", "DivisionCode", "TaxTh", "MaterialStatisticsGroup", "AccountAssignment",
                        "GeneralItemCategory", "Availability", "Transportation", "LoadingGroup", "BoiCode", "PurchasingGroup",
                        "ProfitCenter", "PlanDelTime", "SchedMargin", "ValClass", "Price", "PriceUnit", "CostingLotSize",
                        "MrpController", "MinLot", "MaxLot", "FixedLot", "Rounding", "Mtlsm", "Effective", "StorageLoc",
                        "ReceiveStorage", "ProductionSupervisor", "QuotationNumber", "PoNumber", "StatusInA", "ToolingBSection",
                        "DateIn", "ModelName"
                    });
                    break;
                case RequestType.ToolingB_PU:
                    headers.AddRange(new[] 
                    {
                        "CurrentICS", "ItemCode", "EnglishMatDescription", "Level", "Rohs", "MaterialGroup", "BaseUnit",
                        "ExternalMaterialGroup", "DivisionCode", "DevicePlant", "AssemblyPlant", "IpoPlant", "AsiOfPlant",
                        "Plant", "SalesOrg", "DistributionChannel"
                    });
                    break;
                case RequestType.BOM:
                     headers.AddRange(new[] { "Level", "Item", "ItemCat", "ComponentNumber", "Description", "ItemQuantity", "Unit", "BomUsage", "Sloc","Plant" });
                    break;
                case RequestType.EditBOM:
                    headers.AddRange(new[]
                    {
                        "ItemCodeFrom", "DescriptionFrom", "ItemQuantityFrom", "UnitFrom", "BomUsageFrom", "SlocFrom",
                        "ItemCodeTo", "DescriptionTo", "ItemQuantityTo", "UnitTo", "BomUsageTo", "SlocTo", "PlantTo"
                    });
                    break;
                case RequestType.Routing:
                    headers.AddRange(new[] 
                    {
                        "Counter", "Plant", "Material", "Description", "WorkCenter",
                        "", "", "", "", "",
                        "BaseQty", "Unit", "DirectLaborCosts", "", "", "DirectExpenses",
                        "", "", "", "", "",
                        "AllocationExpense", "", "ProductionVersionCode", "Version",
                        "", "", "", "", "",
                        "",
                        "ValidFrom", "ValidTo", "", "MaximumLotSize", "Unit", "Alternative", "BomUsage",
                        "",
                        "Group", "GroupCounter"
                    });
                    break;
                case RequestType.AddStorage:
                     headers.AddRange(new[] { "ItemCode", "Plant", "StorageLocation" });
                    break;
                case RequestType.DistributionChanel:
                     headers.AddRange(new[] { "ItemCode", "Plant", "StorageLocation", "DistributionChannel", "DivisionCode", "AccountAssignment", "ProfitCenter", "BoiCode" });
                    break;
                case RequestType.IPO:
                     headers.AddRange(new[] 
                     {
                        "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "Plant", "MaterialGroup", 
                        "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "DistributionChannel", "BoiCode", 
                        "PurchasingGroup", "TariffCode", "MrpController", "StorageLocation", "ValClass", "Price", "Planner"
                     });
                    break;
            }
            return headers;
        }
    }
}
