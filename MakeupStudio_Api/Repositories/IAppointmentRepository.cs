using MakeupStudio_Api.Models;

namespace MakeupStudio_Api.Repositories;

//this interface defines what operations are allowed for appointments
//it is used to separate controller logic from database logic
public interface IAppointmentRepository
{
    //returns all appointments in the system
    //used mainly by admin endpoints
    Task<List<Appointment>> GetAllAsync();

    //returns a single appointment by its id
    //used by admin and internally after creating an appointment
    Task<Appointment?> GetByIdAsync(int id);

    //checks if a requested appointment time overlaps with existing ones
    //uses the selected service ids to calculate total duration
    Task<bool> IsSlotTakenAsync(
        DateTime appointmentDate,
        List<int> serviceIds
    );

    //creates a new appointment and links it with selected services
    //returns the created appointment with services loaded
    Task<Appointment> CreateAsync(
        Appointment appointment,
        List<int> serviceIds
    );

    //updates the status of an appointment (pending, confirmed, cancelled, completed)
    //used by admin actions
    Task<bool> UpdateStatusAsync(int id, AppointmentStatus status);

    //returns all appointments that belong to a specific user (by email)
    //used for the "my appointments" endpoint
    Task<List<Appointment>> GetByEmailAsync(string email);

    //cancels an appointment by setting its status to cancelled
    //used instead of deleting to keep history
    Task<bool> CancelAsync(int id);
}
