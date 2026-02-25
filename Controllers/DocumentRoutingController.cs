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
    public class DocumentRoutingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentRoutingController(ApplicationDbContext context)
        {
            _context = context;
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
                    section = new Section { SectionName = createModel.NewSectionName, DepartmentId = department.DepartmentId };
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
                    var newRouting = new DocumentRouting
                    {
                        DocumentTypeId = documentType.DocumentTypeId,
                        DepartmentId = department.DepartmentId,
                        SectionId = section.SectionId,
                        PlantId = plant.PlantId,
                        Step = createModel.Step
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
            ViewData["SectionId"] = new SelectList(_context.Sections.OrderBy(s => s.SectionName), "SectionId", "SectionName", documentRouting.SectionId);
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
            ViewData["SectionId"] = new SelectList(_context.Sections.OrderBy(s => s.SectionName), "SectionId", "SectionName", routingToUpdate.SectionId);
            ViewData["PlantId"] = new SelectList(_context.Plants.OrderBy(p => p.PlantName), "PlantId", "PlantName", routingToUpdate.PlantId);
            
            var documentType = await _context.DocumentTypes.FindAsync(routingToUpdate.DocumentTypeId);
            if (documentType != null)
            {
                routingToUpdate.DocumentType = documentType;
            }

            return View(routingToUpdate);
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
    }
}
