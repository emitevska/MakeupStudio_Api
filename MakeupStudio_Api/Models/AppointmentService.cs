namespace MakeupStudio_Api.Models;

//this is the join table model for the many-to-many relationship
//it connects appointments with the services selected for them
public class AppointmentService
{
    //foreign key pointing to the appointment
    public int AppointmentId { get; set; }

    //navigation property to access the related appointment
    //can be null when ef core is not loading it
    public Appointment? Appointment { get; set; }

    //foreign key pointing to the service
    public int ServiceId { get; set; }

    //navigation property to access the related service
    //used when loading appointment details with services
    public Service? Service { get; set; }
}
