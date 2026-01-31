using MakeupStudio_Api.Models;
using System.ComponentModel.DataAnnotations;

namespace MakeupStudio_Api.Dtos;

public class UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus Status { get; set; }
}
