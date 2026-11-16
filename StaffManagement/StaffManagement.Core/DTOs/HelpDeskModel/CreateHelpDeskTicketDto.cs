using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.HelpDeskModel;

public sealed class CreateHelpDeskTicketDto
{
    [EnumDataType(typeof(HelpDeskTicketType))]
    public HelpDeskTicketType TicketType { get; set; }

    [EnumDataType(typeof(HelpDeskStatus))]
    public HelpDeskStatus Status { get; set; } = HelpDeskStatus.Open;

    [Required(ErrorMessage = "Description is required."), StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}
