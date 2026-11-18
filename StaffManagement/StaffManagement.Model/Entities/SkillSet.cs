using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StaffManagement.Model.Entities
{
    public class SkillSet
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string SkillName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
