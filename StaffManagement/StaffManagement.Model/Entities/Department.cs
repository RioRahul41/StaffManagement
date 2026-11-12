using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StaffManagement.Model.Entities
{
    public class Department


    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Department name must be 2 to 50 characters")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
