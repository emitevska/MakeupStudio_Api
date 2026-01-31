using MakeupStudio_Api.Models;

namespace MakeupStudio_Api.Dtos;

//this dto is used when sending appointment data back to the client
public class AppointmentDto
{
    //database id of the appointment
    public int Id { get; set; }

    //date and time when the appointment is scheduled
    public DateTime AppointmentDate { get; set; }

    //name of the client who booked the appointment
    public string ClientName { get; set; } = string.Empty;

    //phone number provided by the client
    public string PhoneNumber { get; set; } = string.Empty;

    //email of the client (used to link appointments to users)
    public string Email { get; set; } = string.Empty;

    //current status of the appointment (pending, confirmed, cancelled, completed)
    public AppointmentStatus Status { get; set; }

    //list of services included in this appointment
    //mapped from the join table into a simple list for easier frontend use
    public List<ServiceDto> Services { get; set; } = new();
}
