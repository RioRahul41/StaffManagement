# NuGet Package Upgrade Plan

## Overview

**Target**: Add and restore `Microsoft.AspNetCore.Identity.EntityFrameworkCore` for the `StaffManagement.Data` project targeting `net10.0`.
**Scope**: One data project and the solution restore/build.

## Tasks

### 01-add-identity-efcore: Add Identity Entity Framework Core package

Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to `StaffManagement.Data` using a stable version compatible with `net10.0`, then restore and validate the solution. The assessment found that the package is currently not referenced, so the failure is occurring during the attempted installation rather than an API upgrade; the task will capture the concrete NuGet restore error and correct the project/source/version configuration without changing unrelated projects.

**Done when**: `StaffManagement.Data.csproj` contains the package reference, restore completes without rollback/errors, the full solution builds with zero warnings, and available tests pass.
