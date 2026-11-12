# 01-add-identity-efcore: Add Identity Entity Framework Core package

Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to `StaffManagement.Data` using a stable version compatible with `net10.0`, then restore and validate the solution. The assessment found that the package is currently not referenced, so the failure is occurring during the attempted installation rather than an API upgrade; the task will capture the concrete NuGet restore error and correct the project/source/version configuration without changing unrelated projects.

## Research findings

- `StaffManagement.Data/StaffManagement.Data.csproj` is SDK-style, targets `net10.0`, enables implicit usings and nullable reference types, and currently has no `PackageReference` items.
- The project references `StaffManagement.Core` and `StaffManagement.Model`.
- `get_project_dependencies` found no Central Package Management or imported package-version file; the package reference should therefore be added directly to `StaffManagement.Data.csproj`.
- NuGet.org is enabled and the installed SDK is .NET SDK `10.0.401`.

**Done when**: `StaffManagement.Data.csproj` contains the package reference, restore completes without rollback/errors, the full solution builds with zero warnings, and available tests pass.
