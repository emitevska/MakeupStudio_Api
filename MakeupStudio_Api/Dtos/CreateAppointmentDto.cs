using System.ComponentModel.DataAnnotations;

namespace MakeupStudio_Api.Dtos;

public class CreateAppointmentDto
{
    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<int> ServiceIds { get; set; } = new();
}
