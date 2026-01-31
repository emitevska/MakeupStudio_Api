using MakeupStudio_Api.Data;
using MakeupStudio_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeupStudio_Api.Repositories;

//this repository contains all database logic for appointments
//controllers call these methods instead of talking to ef core directly
public class AppointmentRepository : IAppointmentRepository
{
    //db context gives us access to tables (appointments, services, join table, identity tables)
    private readonly ApplicationDbContext _db;

    //constructor injection so ef core context is available here
    public AppointmentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    //returns all appointments in the system (used by admin)
    //includes services so swagger can show full appointment details
    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _db.Appointments
            .Include(a => a.AppointmentServices)     //load join table rows
            .ThenInclude(x => x.Service)             //then load each related service
            .ToListAsync();
    }

    //returns one appointment by id (used by admin and also internally after create)
    //includes services for a complete result
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _db.Appointments
            .Include(a => a.AppointmentServices)
            .ThenInclude(x => x.Service)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    //checks if the requested time conflicts with an existing booking
    //it looks at the total duration of the selected services and checks for overlap
    public async Task<bool> IsSlotTakenAsync(
        DateTime appointmentDate,
        List<int> serviceIds
    )
    {
        //step 1:calculate how long the new appointment will last
        //we sum the duration of all selected services (bridal 90 + soft 60, etc.)
        var totalDurationMinutes = await _db.Services
            .Where(s => serviceIds.Contains(s.Id))
            .SumAsync(s => s.DurationMinutes);

        //new appointment start/end
        var newStart = appointmentDate;
        var newEnd = appointmentDate.AddMinutes(totalDurationMinutes);

        //step 2:check if it overlaps with any existing appointment
        //overlap rule:existingStart < newEnd AND existingEnd > newStart
        return await _db.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled) //cancelled ones don't block time
            .AnyAsync(a =>
                a.AppointmentDate < newEnd &&
                a.AppointmentDate.AddMinutes(
                    a.AppointmentServices
                        .Sum(x => x.Service!.DurationMinutes)   //existing appointment duration
                ) > newStart
            );
    }

    //creates an appointment and connects it to services through the join table
    public async Task<Appointment> CreateAsync(
        Appointment appointment,
        List<int> serviceIds
    )
    {
        //step 1:save the appointment first so the database generates an Id
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        //step 2:create the join rows that connect this appointment to each selected service
        var links = serviceIds.Select(serviceId => new AppointmentService
        {
            AppointmentId = appointment.Id,
            ServiceId = serviceId
        }).ToList();

        _db.AppointmentServices.AddRange(links);
        await _db.SaveChangesAsync();

        //step 3:return the appointment again but with services loaded
        //this makes the controller response include the service list
        return await GetByIdAsync(appointment.Id)
            ?? appointment;
    }

    //admin uses this to change status (pending -> confirmed -> completed/cancelled)
    public async Task<bool> UpdateStatusAsync(
        int id,
        AppointmentStatus status
    )
    {
        //FindAsync looks up by primary key (fast)
        var appt = await _db.Appointments.FindAsync(id);

        if (appt is null)
        {
            return false; //nothing to update
        }

        appt.Status = status;
        await _db.SaveChangesAsync();

        return true;
    }

    //returns appointments for one specific user (used by /api/appointments/me)
    //we filter by email because that's what we store in the appointment record
    public async Task<List<Appointment>> GetByEmailAsync(string email)
    {
        return await _db.Appointments
            .Where(a => a.Email == email)
            .Include(a => a.AppointmentServices)
            .ThenInclude(x => x.Service)
            .ToListAsync();
    }

    //marks an appointment as cancelled
    //we don't delete rows, we keep history (better for admin tracking)
    public async Task<bool> CancelAsync(int id)
    {
        var appt = await _db.Appointments.FindAsync(id);

        if (appt is null)
        {
            return false;
        }

        appt.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();

        return true;
    }
}
