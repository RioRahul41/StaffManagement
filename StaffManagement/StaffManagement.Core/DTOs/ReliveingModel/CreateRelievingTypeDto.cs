using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StaffManagement.Core.DTOs.ReliveingModel
{
   
        public class CreateRelievingTypeDto
        {
            [Required(ErrorMessage = "Relieving type name is required.")]
            [StringLength(100, ErrorMessage = "Maximum 100 characters allowed.")]
            public string RelievingTypeName { get; set; } = string.Empty;
        }
    
}
