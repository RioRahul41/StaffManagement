using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.HelpDeskModel;

public sealed class CreateHelpDeskTicketDto
{
    [Required(ErrorMessage = "Select a ticket type.")]
    [EnumDataType(typeof(HelpDeskTicketType))]
    public HelpDeskTicketType? TicketType { get; set; }

    [Required(ErrorMessage = "Description is required."), StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}
