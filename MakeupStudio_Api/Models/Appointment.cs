using System.ComponentModel.DataAnnotations;

namespace MakeupStudio_Api.Models;

//this is the main database model for appointments
//each instance represents one booking made by a client
public class Appointment
{
    //primary key for the appointment table
    public int Id { get; set; }

    //date and time when the appointment is scheduled
    //required because an appointment must have a time
    [Required]
    public DateTime AppointmentDate { get; set; }

    //name of the client who booked the appointment
    //limited to 100 characters to keep data clean
    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    //phone number provided by the client
    //validated using the built-in phone attribute
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    //email address of the client
    //used to link appointments to user accounts
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    //current status of the appointment
    //defaults to Pending when a new appointment is created
    public AppointmentStatus Status { get; set; }
        = AppointmentStatus.Pending;

    //navigation property for the many-to-many relationship with services
    //one appointment can include multiple services
    public ICollection<AppointmentService> AppointmentServices { get; set; }
        = new List<AppointmentService>();
}
