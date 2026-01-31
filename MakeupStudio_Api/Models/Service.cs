using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeupStudio_Api.Models;

//this model represents a makeup service that can be booked
//examples: bridal makeup, evening makeup, soft makeup
public class Service
{
    //primary key for the service table
    public int Id { get; set; }

    //name of the service shown to clients
    //limited to 100 characters to keep names reasonable
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    //how long this service takes in minutes
    //used when calculating time slots and overlaps
    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    //price of the service
    //numeric type is used for better precision with money values
    [Range(0, 100000)]
    [Column(TypeName = "numeric(18,2)")]
    public decimal Price { get; set; }

    //navigation property for the many-to-many relationship with appointments
    //a service can be part of many different appointments
    public ICollection<AppointmentService> AppointmentServices { get; set; }
        = new List<AppointmentService>();
}
