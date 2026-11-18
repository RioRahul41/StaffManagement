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

        public DbSet<Project> Projects { get; set; } = null!;

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; } = null!;
        public DbSet<EmployeeOtherDetail> EmployeeOtherDetails { get; set; } = null!;

        public DbSet<EmployeeProject> EmployeeProjects { get; set; } = null!;

        public DbSet<Timesheet> Timesheets { get; set; } = null!;

        public DbSet<EmployeeExpense> EmployeeExpenses { get; set; } = null!;

        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;

        public DbSet<LeaveTypeMaster> LeaveTypes { get; set; } = null!;

        public DbSet<PolicyDocument> PolicyDocuments { get; set; } = null!;

        public DbSet<Holiday> Holidays { get; set; } = null!;

        public DbSet<HelpDeskTicket> HelpDeskTickets { get; set; } = null!;

        public DbSet<EmployeeCelebrationNotification> EmployeeCelebrationNotifications { get; set; } = null!;

        public DbSet<SkillSet> SkillSets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>().Property(employee => employee.SickLeaveAllowance).HasDefaultValue(10);
            modelBuilder.Entity<Employee>().Property(employee => employee.SickLeaveRemaining).HasDefaultValue(10);
            modelBuilder.Entity<Employee>().Property(employee => employee.CasualLeaveAllowance).HasDefaultValue(10);
            modelBuilder.Entity<Employee>().Property(employee => employee.CasualLeaveRemaining).HasDefaultValue(10);

            modelBuilder.Entity<Holiday>()
                .Property(holiday => holiday.HolidayDate)
                .HasColumnType("date");

            modelBuilder.Entity<Holiday>()
                .HasIndex(holiday => holiday.HolidayDate);

            modelBuilder.Entity<SkillSet>()
                .HasIndex(skill => skill.SkillName)
                .IsUnique();

            modelBuilder.Entity<EmployeeOtherDetail>()
                .HasOne(detail => detail.Employee)
                .WithMany()
                .HasForeignKey(detail => detail.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(profile => profile.Employee)
                .WithOne()
                .HasForeignKey<EmployeeProfile>(profile => profile.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(profile => profile.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasOne<Department>()
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeProject>()
                .HasKey(assignment => new { assignment.EmployeeId, assignment.ProjectId });

            modelBuilder.Entity<EmployeeProject>()
                .HasOne(assignment => assignment.Employee)
                .WithMany(employee => employee.ProjectAssignments)
                .HasForeignKey(assignment => assignment.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeProject>()
                .HasOne(assignment => assignment.Project)
                .WithMany()
                .HasForeignKey(assignment => assignment.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(employee => employee.ReportingPerson)
                .WithMany()
                .HasForeignKey(employee => employee.ReportingPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timesheet>()
                .HasOne(timesheet => timesheet.Employee)
                .WithMany()
                .HasForeignKey(timesheet => timesheet.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Timesheet>()
                .HasOne(timesheet => timesheet.Project)
                .WithMany()
                .HasForeignKey(timesheet => timesheet.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeExpense>()
                .HasOne(expense => expense.Employee)
                .WithMany()
                .HasForeignKey(expense => expense.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeExpense>()
                .Property(expense => expense.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeExpense>()
                .Property(expense => expense.BillAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeExpense>()
                .HasIndex(expense => new { expense.EmployeeId, expense.BatchId });

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(request => request.Employee)
                .WithMany()
                .HasForeignKey(request => request.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HelpDeskTicket>()
                .HasOne(ticket => ticket.Employee)
                .WithMany()
                .HasForeignKey(ticket => ticket.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeCelebrationNotification>()
                .Property(notification => notification.CelebrationDate)
                .HasColumnType("date");

            modelBuilder.Entity<EmployeeCelebrationNotification>()
                .HasIndex(notification => new
                {
                    notification.EmployeeId,
                    notification.CelebrationType,
                    notification.CelebrationDate
                })
                .IsUnique();

            modelBuilder.Entity<EmployeeCelebrationNotification>()
                .HasOne(notification => notification.Employee)
                .WithMany()
                .HasForeignKey(notification => notification.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}


