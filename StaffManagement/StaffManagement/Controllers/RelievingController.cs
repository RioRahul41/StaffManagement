using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.ReliveingModel;
using StaffManagement.Core.Iservices;
using System.Security.Claims;

namespace StaffManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,HR")]

    public class RelievingController : Controller
    {
        private readonly IRelievingTypeService _relievingTypeService;

        public RelievingController(IRelievingTypeService relievingTypeService)
        {
            _relievingTypeService = relievingTypeService;
        }

        [Authorize(Roles = "Admin,Manager,HR")]
        public async Task<IActionResult> Index()
        {
            var relievingTypes = await _relievingTypeService.GetAllAsync();

            return View(relievingTypes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            CreateRelievingTypeDto model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            
            var createdby = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Unknown";
            var result = await _relievingTypeService.CreateAsync(model , createdby);
            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.RelievingTypeName),
                    "This relieving type already exists.");

                return View(model);
            }
            TempData["SuccessMessage"] = "Relieving type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _relievingTypeService.GetByIdAsync(id);

            if (type == null)
                return NotFound();

            var model = new UpdateRelievingTypeDto
            {
                Id = type.Id,
                RelievingTypeName = type.RelievingTypeName,
                IsActive = type.IsActive
            };

            return View(model);
        }

        // POST: RelievingType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UpdateRelievingTypeDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var updatedBy = User.FindFirstValue(
                ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Unknown";

            var result = await _relievingTypeService.UpdateAsync(model, updatedBy);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.RelievingTypeName),
                    "Relieving type not found or name already exists.");

                return View(model);
            }

            TempData["Success"] = "Relieving type updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: RelievingType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var updatedBy = User.Identity?.Name ?? "Unknown";

            var result = await _relievingTypeService.SoftDeleteAsync(
                id, updatedBy);

            if (!result)
            {
                TempData["Error"] = "Relieving type not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Relieving type deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: RelievingType/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            bool isActive)
        {
            var updatedBy = User.FindFirstValue(
                ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Unknown";

            var result = await _relievingTypeService.UpdateStatusAsync(
                id, isActive, updatedBy);

            TempData[result ? "Success" : "Error"] =
                result
                    ? "Relieving type status updated successfully."
                    : "Relieving type not found.";

            return RedirectToAction(nameof(Index));
        }
    }
}

