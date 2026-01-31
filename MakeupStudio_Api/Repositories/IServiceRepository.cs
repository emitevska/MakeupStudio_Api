using MakeupStudio_Api.Models;

namespace MakeupStudio_Api.Repositories;

//this interface defines all allowed operations for services
//controllers depend on this interface instead of a concrete ef core implementation
public interface IServiceRepository
{
    //returns all available services (bridal, evening, soft, etc.)
    //used by both clients and admin
    Task<List<Service>> GetAllAsync();

    //returns a single service by its id
    //used for validation and admin actions
    Task<Service?> GetByIdAsync(int id);

    //creates a new service
    //admin only operation
    Task<Service> CreateAsync(Service service);

    //deletes a service by id
    //returns false if the service does not exist
    //admin only operation
    Task<bool> DeleteAsync(int id);
}
