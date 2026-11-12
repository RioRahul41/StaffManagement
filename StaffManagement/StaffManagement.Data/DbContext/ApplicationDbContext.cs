using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Model.Entities;
using System;
using System.Collections.Generic;

namespace StaffManagement.Data.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentity>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
        public DbSet<Employee> Employees { get; set; }
    }
}
