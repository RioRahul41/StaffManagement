using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Core.DTOs.ReliveingModel
{
    public class RelievingTypeDto
    {
        public int Id { get; set; }

        public string RelievingTypeName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
