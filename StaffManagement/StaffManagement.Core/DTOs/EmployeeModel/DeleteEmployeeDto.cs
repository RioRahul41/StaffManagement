using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StaffManagement.Core.DTOs.EmployeeModel
{
    public class DeleteEmployeeDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please select relieving type.")]
        public int? RelievingTypeId { get; set; }

        [StringLength(500, ErrorMessage = "Maximum 500 characters allowed.")]
        public string? DeletedReason { get; set; }
    }
}
