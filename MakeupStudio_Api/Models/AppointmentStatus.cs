namespace MakeupStudio_Api.Models;

//this enum represents the current state of an appointment
//it helps keep status values consistent and easy to understand
public enum AppointmentStatus
{
    //appointment is created but not yet approved by admin
    Pending = 0,

    //appointment is confirmed and scheduled
    Confirmed = 1,

    //appointment was cancelled by client or admin
    Cancelled = 2,

    //appointment has already taken place
    Completed = 3
}
