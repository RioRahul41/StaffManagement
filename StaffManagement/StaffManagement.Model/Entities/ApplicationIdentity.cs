using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Model.Entities
{
    public class ApplicationIdentity : IdentityUser 
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool UserIsDeleted { get; set; } = false;
        public bool UserIsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
    }
}
