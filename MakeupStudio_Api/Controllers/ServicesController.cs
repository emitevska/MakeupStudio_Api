using MakeupStudio_Api.Models;
using MakeupStudio_Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakeupStudio_Api.Controllers;

[ApiController]

//base route: /api/Services (because controller=Services)
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    //repository that talks to the database for Services table
    private readonly IServiceRepository _services;

    //dependency Injection: ASP.NET creates this controller and gives it to the repository
    public ServicesController(IServiceRepository services)
    {
        _services = services;
    }

    //=========================
    //PUBLIC, no login required
    //=========================

    //GET: api/services
    //Returns all services
    [HttpGet]
    public async Task<ActionResult<List<Service>>> GetAll()
    {
        //Calls repository to read all services from db
        var result = await _services.GetAllAsync();
        return Ok(result);
    }

    //GET: api/services/id
    //Returns one service by id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Service>> GetById(int id)
    {
        //finds service in db by primary key
        var service = await _services.GetByIdAsync(id);

        //if not found, return Not Found
        if (service is null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    //=========================
    //ADMIN ONLY
    //=========================

    //POST: api/services
    //creates a new service that is admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Service>> Create(Service service)
    {
        //save the new service to the database
        var created = await _services.CreateAsync(service);

        //location header points yo GET /api/Services/{id}
        //response body contains the created service
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    //DELETE: api/services/5
    //only the admin can delete service by id 
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        //tries to delete in db and returns true if deleted and false if not found
        var deleted = await _services.DeleteAsync(id);

        //if service doesnt exist, 404, Not Found
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
