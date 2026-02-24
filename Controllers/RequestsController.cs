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

namespace myapp.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
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

        public IActionResult Create()
        {
            var viewModel = new CreateRequestViewModel
            {
                RequesterName = "Sample User", 
                Department = "IT",
                Section = "Development",
                Plant = "HQ-01"
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                 var requestItem = new RequestItem
                {
                    RequestType = viewModel.RequestType.ToString(),
                    Description = viewModel.Description,
                    Requester = viewModel.RequesterName,
                    Status = "Pending",
                    RequestDate = DateTime.UtcNow,

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
                    StatusInA = viewModel.StatusInA,
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
            // The validation summary will now display all errors.
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
                StatusInA = requestItem.StatusInA,
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
                try
                {
                    var requestItemToUpdate = await _context.RequestItems
                        .Include(r => r.BomComponents)
                        .Include(r => r.Routings)
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (requestItemToUpdate == null)
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                        return NotFound();
                    }

                    // Update scalar properties from the view model
                    requestItemToUpdate.RequestType = viewModel.RequestType.ToString();
                    requestItemToUpdate.Description = viewModel.Description;
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
                    requestItemToUpdate.StatusInA = viewModel.StatusInA;
                    requestItemToUpdate.DateIn = DateTime.TryParse(viewModel.DateIn, out var dateIn) ? dateIn : null;
                    requestItemToUpdate.QuotationNumber = viewModel.QuotationNumber;
                    requestItemToUpdate.ToolingBModel = viewModel.ToolingBModel;
                    requestItemToUpdate.TariffCode = viewModel.TariffCode;
                    requestItemToUpdate.Planner = viewModel.Planner;
                    requestItemToUpdate.PurchasingGroup = viewModel.PurchasingGroup;
                    requestItemToUpdate.Price = decimal.TryParse(viewModel.Price, out var price) ? price : null;

                    // Remove existing related entities
                    _context.BomComponents.RemoveRange(requestItemToUpdate.BomComponents);
                    _context.Routings.RemoveRange(requestItemToUpdate.Routings);

                    // Add updated related entities from the view model
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

                    _context.Update(requestItemToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Request updated successfully!";
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
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Create));
            }

            var newRequests = new List<RequestItem>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "The Excel file is empty or corrupted.";
                            return RedirectToAction(nameof(Create));
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++) 
                        {
                            var requestItem = new RequestItem
                            {
                                Description = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? string.Empty,
                                Requester = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty,
                                Status = "Pending",
                                RequestDate = DateTime.UtcNow
                            };
                            if (!string.IsNullOrWhiteSpace(requestItem.Description) && !string.IsNullOrWhiteSpace(requestItem.Requester))
                            {
                                newRequests.Add(requestItem);
                            }
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
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the file.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
