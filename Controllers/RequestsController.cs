using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Threading.Tasks;
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

namespace myapp.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.RequestItems.OrderByDescending(r => r.RequestDate).ToListAsync();
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

            return View(requestItem);
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
                Plant = user.Plant ?? string.Empty,
                Status = "Pending"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string? nextApproverId = null;
                if (!string.IsNullOrEmpty(viewModel.NextResponsibleUserId))
                {
                    var parts = viewModel.NextResponsibleUserId.Split('|');
                    if (parts.Length > 0)
                    {
                        nextApproverId = parts[0];
                    }
                }

                 var requestItem = new RequestItem
                {
                    RequestType = viewModel.RequestType.ToString(),
                    Description = viewModel.Description,
                    Requester = viewModel.RequesterName,
                    Status = User.IsInRole("IT") ? viewModel.Status : "Pending",
                    RequestDate = DateTime.UtcNow,
                    NextApproverId = nextApproverId, // Set the next approver

                    // Correctly parse and assign values
                    PlantFG = viewModel.PlantFG,
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
                        Group = r.Group,
                        GroupCounter = r.GroupCounter
                    }).ToList()
                };

                _context.Add(requestItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Request created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestItem = await _context.RequestItems
                .Include(r => r.BomComponents)
                .Include(r => r.Routings)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestItem == null)
            {
                return NotFound();
            }
            
            var viewModel = new CreateRequestViewModel
            {
                Id = requestItem.Id,
                RequestType = Enum.Parse<RequestType>(requestItem.RequestType, true),
                Description = requestItem.Description,
                RequesterName = requestItem.Requester,
                Status = requestItem.Status,

                // Convert model properties back to strings for display
                PlantFG = requestItem.PlantFG,
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
                    Group = r.Group,
                    GroupCounter = r.GroupCounter
                }).ToList()
            };

            return View("Edit", viewModel); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateRequestViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                 var requestItemToUpdate = await _context.RequestItems
                    .Include(r => r.BomComponents)
                    .Include(r => r.Routings)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (requestItemToUpdate == null)
                {
                    return NotFound();
                }

                try
                {
                    // Update scalar properties from the view model
                    requestItemToUpdate.RequestType = viewModel.RequestType.ToString();
                    requestItemToUpdate.Description = viewModel.Description;
                    requestItemToUpdate.Status = User.IsInRole("IT") ? viewModel.Status : requestItemToUpdate.Status;
                    requestItemToUpdate.PlantFG = viewModel.PlantFG;
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
                    requestItemToUpdate.Price = decimal.TryParse(viewModel.Price, out var price) ? price : null;

                    // Remove existing related entities and add updated ones
                    _context.BomComponents.RemoveRange(requestItemToUpdate.BomComponents);
                    _context.Routings.RemoveRange(requestItemToUpdate.Routings);
                    
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
                        Group = r.Group,
                        GroupCounter = r.GroupCounter
                    }).ToList();

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Request updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestItemExists(viewModel.Id))
                    {
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
            var requestItem = await _context.RequestItems.FindAsync(id);
            if (requestItem == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return NotFound();
            }

            _context.RequestItems.Remove(requestItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Request deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool RequestItemExists(int id)
        {
            return _context.RequestItems.Any(e => e.Id == id);
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

            if (requestType == RequestType.BOM || requestType == RequestType.Routing)
            {
                TempData["ErrorMessage"] = "Import for BOM and Routing request types is not supported yet.";
                return RedirectToAction(nameof(Create));
            }

            var newRequests = new List<RequestItem>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null || worksheet.LastRowUsed() == null || worksheet.LastRowUsed().RowNumber() < 2)
                        {
                            TempData["ErrorMessage"] = "The Excel file is empty or does not contain any data.";
                            return RedirectToAction(nameof(Create));
                        }

                        var headerRow = worksheet.Row(1);
                        var headerMap = new Dictionary<string, int>();
                        foreach (var cell in headerRow.CellsUsed())
                        {
                            headerMap[cell.Value.ToString().Trim()] = cell.Address.ColumnNumber;
                        }

                        for (int rowNum = 2; rowNum <= worksheet.LastRowUsed().RowNumber(); rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            var requestItem = new RequestItem
                            {
                                RequestType = requestType.ToString(),
                                Requester = $"{user.FirstName} {user.LastName}",
                                Status = "Pending",
                                RequestDate = DateTime.UtcNow,
                                NextApproverId = nextApproverId, // Assign the selected approver
                                Description = $"Imported {requestTypeString} data on {DateTime.UtcNow.ToShortDateString()}"
                            };

                            foreach (var header in headerMap)
                            {
                                var cellValue = row.Cell(header.Value).GetFormattedString();
                                SetProperty(requestItem, header.Key, cellValue);
                            }

                            newRequests.Add(requestItem);
                        }
                    }
                }

                if (newRequests.Any())
                {
                    _context.RequestItems.AddRange(newRequests);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{newRequests.Count} requests have been successfully imported.";
                }
                else
                {
                    TempData["ErrorMessage"] = "The file was processed, but no valid requests were found to import.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception ex.ToString() to your logging framework
                TempData["ErrorMessage"] = "An error occurred while processing the file. Please ensure the data format is correct.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void SetProperty(RequestItem item, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return; // Do not set property if cell is empty
            }

            var propertyInfo = typeof(RequestItem).GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                try
                {
                    var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    var convertedValue = Convert.ChangeType(value, targetType);
                    propertyInfo.SetValue(item, convertedValue, null);
                }
                catch (Exception)
                {
                    // Could log a warning: $"Could not set property {propertyName} with value {value}"
                }
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetNextApprovers(RequestType requestType)
        {
            var requestTypeName = requestType.ToString();
            var routings = await _context.DocumentRoutings
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.Department)
                .Include(dr => dr.Section) // Include Section data
                .Where(dr => dr.DocumentType.Name == requestTypeName)
                .OrderBy(dr => dr.Step)
                .ToListAsync();

            if (!routings.Any())
            {
                return Json(new List<object>());
            }

            var allUsers = await _userManager.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();
            var approvers = new List<object>();

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

                    foreach (var user in foundUsers)
                    {
                        approvers.Add(new
                        {
                            Id = $"{user.Id}|{stepRouting.Id}",
                            FullName = $"{user.FirstName} {user.LastName} (Assign to {ruleForDisplay})"
                        });
                    }
                }
            }
            
            var distinctApprovers = approvers
                .GroupBy(a => ((dynamic)a).Id)
                .Select(g => g.First())
                .ToList();

            return Json(distinctApprovers);
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

                worksheet.Columns().AdjustToContents();

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
                        "PlantFG", "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "MaterialGroup",
                        "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "DistributionChannel", "BoiCode",
                        "MrpController", "StorageLocation", "ProductionSupervisor", "CostingLotSize", "ValClass"
                    });
                    break;
                case RequestType.SM:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "EnglishMatDescription", "BaseUnit", "PlantFG", "MaterialGroup", "DivisionCode",
                        "ProfitCenter", "MrpController", "StorageLocation", "ProductionSupervisor", "CostingLotSize", "StandardPack"
                    });
                    break;
                case RequestType.RM:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "BoiDescription", "PlantFG",
                        "MaterialGroup", "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "PurchasingGroup",
                        "MakerMfrPartNumber", "CommCodeTariffCode", "TraffCodePercentage", "MrpController",
                        "StorageLocation", "StorageLocationB1", "PriceControl", "ValClass", "Price", "Currency",
                        "CostingLotSize", "SupplierCode"
                    });
                    break;
                case RequestType.ToolingB:
                    headers.AddRange(new[] 
                    {
                        "ItemCode", "MatType", "Check", "EnglishMatDescription", "MaterialGroup", "BaseUnit",
                        "ExternalMaterialGroup", "PlantFG", "DevicePlant", "AssemblyPlant", "IpoPlant", "AsiOfPlant",
                        "PurchasingGroup", "DivisionCode", "ProfitCenter", "Price", "PriceUnit", "StorageLocationEP",
                        "ToolingBModel", "ToolingBSection", "PoNumber", "StatusInA", "DateIn", "QuotationNumber"
                    });
                    break;
                case RequestType.ToolingB_FG:
                    headers.AddRange(new[] 
                    {
                        "CurrentICS", "ItemCode", "EnglishMatDescription", "Level", "Rohs", "MaterialGroup", "BaseUnit",
                        "CodenMid", "DevicePlant", "AssemblyPlant", "IpoPlant", "AsiOfPlant", "PlantFG", "SalesOrg",
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
                        "PlantFG", "SalesOrg", "DistributionChannel"
                    });
                    break;
                case RequestType.BOM:
                     headers.AddRange(new[] { "Level", "Item", "ItemCat", "ComponentNumber", "Description", "ItemQuantity", "Unit", "BomUsage", "Plant", "Sloc" });
                    break;
                case RequestType.Routing:
                    headers.AddRange(new[] 
                    {
                        "Material", "Description", "WorkCenter", "Operation", "BaseQty", "Unit", "DirectLaborCosts", 
                        "DirectExpenses", "AllocationExpense", "ProductionVersionCode", "Version", "ValidFrom", "ValidTo", 
                        "MaximumLotSize", "Group", "GroupCounter"
                    });
                    break;
                case RequestType.AddStorage:
                     headers.AddRange(new[] { "ItemCode", "PlantFG", "StorageLocation" });
                    break;
                case RequestType.DistributionChanel:
                     headers.AddRange(new[] { "ItemCode", "PlantFG", "StorageLocation", "DistributionChannel", "DivisionCode", "AccountAssignment", "ProfitCenter", "BoiCode" });
                    break;
                case RequestType.IPO:
                     headers.AddRange(new[] 
                     {
                        "ItemCode", "EnglishMatDescription", "ModelName", "BaseUnit", "PlantFG", "MaterialGroup", 
                        "ExternalMaterialGroup", "DivisionCode", "ProfitCenter", "DistributionChannel", "BoiCode", 
                        "PurchasingGroup", "TariffCode", "MrpController", "StorageLocation", "ValClass", "Price", "Planner"
                     });
                    break;
            }
            return headers;
        }
    }
}
