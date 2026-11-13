# Agent guidance

- Read `.github/copilot-instructions.md` and the relevant project files before making changes; preserve its role and account-creation rules.
- The solution is `StaffManagement/StaffManagement.slnx`. Keep the existing three-tier separation between the web app, Core services, Data repositories, and Model projects.
- Follow the established ASP.NET Core Identity, dependency injection, and Entity Framework patterns already used in the solution.
- Keep changes focused, avoid unrelated refactors, and never expose secrets from configuration files.
- When a change affects behavior, explain the impact and mention relevant build or test checks.

