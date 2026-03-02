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

namespace myapp.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<RequestsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Should not happen for authorized users
            }

            IQueryable<RequestItem> requestsQuery = _context.RequestItems.AsQueryable();

            // If the user is not in the "IT" role, filter the requests.
            if (!User.IsInRole("IT"))
            {
                var currentUserName = $"{user.FirstName} {user.LastName}";
                var userId = user.Id;

                // A user sees their own requests OR requests where they are the next approver.
                requestsQuery = requestsQuery.Where(r => r.Requester == currentUserName || r.NextApproverId == userId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                var isDateSearch = DateTime.TryParse(searchTerm, out var parsedDate);
                var searchDate = parsedDate.Date;

                requestsQuery = requestsQuery.Where(r =>
                    (r.RequestType != null && r.RequestType.Contains(searchTerm)) ||
                    (r.Status != null && r.Status.Contains(searchTerm)) ||
                    (r.ItemCode != null && r.ItemCode.Contains(searchTerm)) ||
                    (isDateSearch && r.RequestDate.Date == searchDate)
                );
            }

            ViewBag.SearchTerm = searchTerm;

            // Order the results and execute the query.
            var requests = await requestsQuery.OrderByDescending(r => r.RequestDate).ToListAsync();
            
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
                RequesterPlant = user.Plant ?? string.Empty,
                Status = "Pending"
            };

            return View(viewModel);
        }

        private void ValidateRequest(CreateRequestViewModel viewModel)
        {
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
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            ValidateRequest(viewModel);

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

        public async Task<IActionResult> Edit(int? id, bool fromImport = false)
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

            // Mark viewModel when redirected from import flow
            viewModel.FromImport = fromImport || (TempData["FromImport"] as string) == "true";
            if (viewModel.FromImport)
            {
                ViewBag.ImportNotice = "This request was created from imported data. Some validations may be relaxed for initial save. Please verify fields before finalizing.";
            }

            ViewBag.CurrentNextApproverName = currentApproverUser != null
                ? $"{currentApproverUser.FirstName} {currentApproverUser.LastName}"
                : "Current Responsible User";

            return View("Edit", viewModel); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateRequestViewModel viewModel)
        {
            _logger.LogInformation("Edit POST called for id={Id}", id);
            if (viewModel != null)
            {
                try { _logger.LogDebug("Posted viewModel: {@ViewModel}", viewModel); } catch { }
            }
            if (id != viewModel.Id)
            {
                return NotFound();
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

                    if (!string.IsNullOrWhiteSpace(viewModel.NextResponsibleUserId))
                    {
                        var nextApproverParts = viewModel.NextResponsibleUserId.Split('|');
                        requestItemToUpdate.NextApproverId = nextApproverParts.Length > 0 ? nextApproverParts[0] : viewModel.NextResponsibleUserId;
                    }
                    requestItemToUpdate.Plant = viewModel.Plant;
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
                                headerMap[headerText] = cell.Address.ColumnNumber;
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
                        "Material", "Description", "WorkCenter", "Operation", "BaseQty", "Unit", "DirectLaborCosts", 
                        "DirectExpenses", "AllocationExpense", "ProductionVersionCode", "Version", "ValidFrom", "ValidTo", 
                        "MaximumLotSize", "Group", "GroupCounter"
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
