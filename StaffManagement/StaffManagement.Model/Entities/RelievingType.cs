using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Model.Entities
{
    public class RelievingType
    {
        public int Id { get; set; }

        public string RelievingTypeName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
        public string? UpdatedBy { get; set; }
    }
}
