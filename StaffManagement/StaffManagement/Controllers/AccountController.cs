using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

public sealed class AccountController(
    SignInManager<ApplicationIdentity> signInManager,
    UserManager<ApplicationIdentity> userManager,
    IAccountService accountService) : Controller
{
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.UserIsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return RedirectToLocal(returnUrl ?? model.ReturnUrl);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    public IActionResult Create()
    {
        var roles = GetCreatorRoles();
        ViewBag.Roles = accountService.GetCreatableRoles(roles);
        return View(new CreateAccountDto());
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto model, CancellationToken cancellationToken)
    {
        var creatorRoles = GetCreatorRoles();
        var allowedRoles = accountService.GetCreatableRoles(creatorRoles);
        if (!allowedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Role), "You cannot create an account with this role.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = allowedRoles;
            return View(model);
        }

        var result = await accountService.CreateAsync(model, creatorRoles, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Roles = allowedRoles;
            return View(model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was created with the {model.Role} role.";
        return RedirectToAction("Index", "Employee");
    }

    private IReadOnlyCollection<string> GetCreatorRoles() =>
        RoleNames.All.Where(role => User.IsInRole(role)).ToArray();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home")!;
}
