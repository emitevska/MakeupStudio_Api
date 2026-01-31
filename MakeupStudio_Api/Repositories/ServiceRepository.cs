using MakeupStudio_Api.Data;
using MakeupStudio_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeupStudio_Api.Repositories;

//this repository handles all database operations related to services
//controllers use this instead of working with ef core directly
public class ServiceRepository : IServiceRepository
{
    //db context gives access to the Services table
    private readonly ApplicationDbContext _db;

    //constructor injection of the db context
    public ServiceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    //returns all services from the database
    //used to show available services to clients and admin
    public async Task<List<Service>> GetAllAsync()
    {
        return await _db.Services.ToListAsync();
    }

    //returns a single service by id
    //used when validating service ids during appointment creation
    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _db.Services.FirstOrDefaultAsync(s => s.Id == id);
    }

    //creates a new service in the database
    //used by admin only endpoints
    public async Task<Service> CreateAsync(Service service)
    {
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return service;
    }

    //deletes a service by id
    //returns false if the service does not exist
    public async Task<bool> DeleteAsync(int id)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id);

        if (service is null)
        {
            return false;
        }

        _db.Services.Remove(service);
        await _db.SaveChangesAsync();
        return true;
    }
}
