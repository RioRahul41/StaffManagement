using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Models;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class HolidayController(IHolidayService holidayService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var holidays = await holidayService.GetAllAsync(cancellationToken);
        return View(new HolidayListViewModel { Holidays = holidays });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        HolidayListViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !model.HolidayDate.HasValue)
        {
            model = new HolidayListViewModel
            {
                Holidays = await holidayService.GetAllAsync(cancellationToken),
                HolidayName = model.HolidayName,
                HolidayDate = model.HolidayDate,
                Description = model.Description
            };
            return View("Index", model);
        }

        await holidayService.AddAsync(
            model.HolidayName,
            model.HolidayDate.Value,
            model.Description,
            cancellationToken);

        TempData["Success"] = "Holiday added to the calendar.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDate(int id, DateTime? holidayDate, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !holidayDate.HasValue || holidayDate.Value == default)
        {
            TempData["Error"] = "Select a valid holiday date.";
            return RedirectToAction(nameof(Index));
        }

        if (!await holidayService.UpdateDateAsync(id, holidayDate.Value, cancellationToken))
            return NotFound();

        TempData["Success"] = "Holiday date updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await holidayService.DeleteAsync(id, cancellationToken))
            return NotFound();

        TempData["Success"] = "Holiday removed from the calendar.";
        return RedirectToAction(nameof(Index));
    }
}
