using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentity>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Department> Departments { get; set; }

        public DbSet<Designation> Designations { get; set; }

        public DbSet<Employee> Employees { get; set; } = null!;

        public DbSet<Timesheet> Timesheets { get; set; } = null!;

        public DbSet<EmployeeExpense> EmployeeExpenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>()
                .HasOne<Department>()
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timesheet>()
                .HasOne(timesheet => timesheet.Employee)
                .WithMany()
                .HasForeignKey(timesheet => timesheet.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeExpense>()
                .HasOne(expense => expense.Employee)
                .WithMany()
                .HasForeignKey(expense => expense.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeExpense>()
                .Property(expense => expense.Amount)
                .HasPrecision(18, 2);

        }
    }
}
