using MakeupStudio_Api.Dtos;               //DTOs used for requests/responses in the API
using MakeupStudio_Api.Models;             //database models/entities (Appointment, Status, etc.)
using MakeupStudio_Api.Repositories;       //repositories 
using Microsoft.AspNetCore.Authorization;  //[Authorize] and role-based access control
using Microsoft.AspNetCore.Mvc;            //ControllerBase, ActionResult, routing attributes
using System.Security.Claims;              //Reading claims (email/role) from the JWT token

namespace MakeupStudio_Api.Controllers;


[ApiController]

//base route for this controller.
//[controller] becomes "Appointments" -> /api/Appointments
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    //repository that handles Appointment database operations
    private readonly IAppointmentRepository _appointments;

    //repository that handles Service database operations (used to validate serviceIds)
    private readonly IServiceRepository _services;

    //constructor 
    public AppointmentsController(
        IAppointmentRepository appointments,
        IServiceRepository services
    )
    {
        _appointments = appointments;
        _services = services;
    }


    //ADMIN ONLY

    //GET: api/appointments
    //only users with role "Admin" can list all appointments in the system
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll()
    {
        //load all appointments (usually includes services via AppointmentServices)
        var items = await _appointments.GetAllAsync();

        //map entity -> DTO so we return a clean response (not EF entities)
        var result = items.Select(a => new AppointmentDto
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            ClientName = a.ClientName,
            PhoneNumber = a.PhoneNumber,
            Email = a.Email,
            Status = a.Status,

            //convert join table (AppointmentServices) into a simple list of services for the client
            Services = a.AppointmentServices
                .Where(x => x.Service != null)
                .Select(x => new ServiceDto
                {
                    Id = x.Service!.Id,
                    Name = x.Service.Name,
                    DurationMinutes = x.Service.DurationMinutes,
                    Price = x.Service.Price
                })
                .ToList()
        }).ToList();

        return Ok(result);
    }

    //GET: api/appointments/5
    //admin can fetch a single appointment by database Id
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        //find appointment by Id (including services)
        var a = await _appointments.GetByIdAsync(id);

        //if not found
        if (a is null)
        {
            return NotFound();
        }

        //map entity -> DTO
        var result = new AppointmentDto
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            ClientName = a.ClientName,
            PhoneNumber = a.PhoneNumber,
            Email = a.Email,
            Status = a.Status,
            Services = a.AppointmentServices
                .Where(x => x.Service != null)
                .Select(x => new ServiceDto
                {
                    Id = x.Service!.Id,
                    Name = x.Service.Name,
                    DurationMinutes = x.Service.DurationMinutes,
                    Price = x.Service.Price
                })
                .ToList()
        };

        return Ok(result);
    }

    //=========================
    //CLIENT: MY APPOINTMENTS
    //=========================

    //GET: api/appointments/me
    //any logged-in user can view only their own appointments (by email claim)
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<AppointmentDto>>> GetMyAppointments()
    {
        //read email from JWT claims:
        //ClaimTypes.Email (standard)
        //or "email" (some JWT libraries use this name)
        var email =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");

        //if token doesn't contain email then treat as not authenticated properly
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized("Email claim not found in token.");
        }

        //gets appointments that belong to this email
        var items = await _appointments.GetByEmailAsync(email);

        //map entity -> DTO
        var result = items.Select(a => new AppointmentDto
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            ClientName = a.ClientName,
            PhoneNumber = a.PhoneNumber,
            Email = a.Email,
            Status = a.Status,
            Services = a.AppointmentServices
                .Where(x => x.Service != null)
                .Select(x => new ServiceDto
                {
                    Id = x.Service!.Id,
                    Name = x.Service.Name,
                    DurationMinutes = x.Service.DurationMinutes,
                    Price = x.Service.Price
                })
                .ToList()
        }).ToList();

        return Ok(result);
    }

    //=========================
    //CLIENT OR ADMIN
    //=========================

    //POST: api/appointments
    //any logged-in user can create an appointment (client) and admin can too
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        CreateAppointmentDto dto
    )
    {
        //preventing double booking or overlapping slots
        //by passing appointmentDate plus serviceIds so repository can calculate duration
        //so it can detect overlap based on each service duration (90 min, 60 min, etc.)
        var slotTaken = await _appointments.IsSlotTakenAsync(
            dto.AppointmentDate,
            dto.ServiceIds
        );

        //if time overlaps another appointment 
        if (slotTaken)
        {
            return Conflict("That time slot is already taken.");
        }

        //validate every serviceId exists
        //if someone sends serviceIds: [999], its rejected with 400 BadRequest
        foreach (var serviceId in dto.ServiceIds)
        {
            var service = await _services.GetByIdAsync(serviceId);

            if (service is null)
            {
                return BadRequest($"ServiceId {serviceId} does not exist.");
            }
        }

        //creating appointment entity (DB model)
        //status starts as Pending until admin confirms/changes it
        var appointment = new Appointment
        {
            AppointmentDate = dto.AppointmentDate,
            ClientName = dto.ClientName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Status = AppointmentStatus.Pending
        };

        //saves appointment plus create join records in AppointmentServices
        var created = await _appointments.CreateAsync(
            appointment,
            dto.ServiceIds
        );

        //maps created entity with DTO for response
        var result = new AppointmentDto
        {
            Id = created.Id,
            AppointmentDate = created.AppointmentDate,
            ClientName = created.ClientName,
            PhoneNumber = created.PhoneNumber,
            Email = created.Email,
            Status = created.Status,
            Services = created.AppointmentServices
                .Where(x => x.Service != null)
                .Select(x => new ServiceDto
                {
                    Id = x.Service!.Id,
                    Name = x.Service.Name,
                    DurationMinutes = x.Service.DurationMinutes,
                    Price = x.Service.Price
                })
                .ToList()
        };

        return Ok(result);
    }

    //=========================
    //ADMIN: UPDATE STATUS
    //=========================

    //PUT: api/appointments/5/status
    //Admin confirms / cancels / completes.. by changing appointment status
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateAppointmentStatusDto dto
    )
    {
        //updates appointment status in DB
        var ok = await _appointments.UpdateStatusAsync(
            id,
            dto.Status
        );

        //appointment doesn't exist => 404
        if (!ok)
        {
            return NotFound();
        }

        return NoContent();
    }

    //PUT: api/appointments/5/cancel
    //Admin can cancel any appointment.
    //Client can cancel only their own appointment 
    [Authorize]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        //gets current user's email from token
        var email =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized("Email claim not found in token.");
        }

        //Admin can cancel anything
        var isAdmin = User.IsInRole("Admin");

        //Load the appointment from DB
        var appt = await _appointments.GetByIdAsync(id);

        if (appt is null)
        {
            return NotFound();
        }

        //if NOT admin, allow cancel only if appointment belongs to the logged-in user
        if (!isAdmin && !string.Equals(appt.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); 
        }

        //cancel the appointment
        var ok = await _appointments.CancelAsync(id);

        if (!ok)
        {
            return NotFound();
        }

        return NoContent(); //success
    }

}
