using MakeupStudio_Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MakeupStudio_Api.Data;

//this is the main ef core database context for the app
//it also includes identity tables because we inherit from IdentityDbContext<ApplicationUser>
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    //options contains the connection string + provider settings (postgres/sql server etc.)
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
    ) : base(options)
    {
    }

    //these DbSet<> properties become tables in the database
    //Services -> services table (the makeup services you offer)
    public DbSet<Service> Services { get; set; }

    //Appointments -> appointments table (the bookings)
    public DbSet<Appointment> Appointments { get; set; }

    //AppointmentServices -> join table for many-to-many (appointments <-> services)
    public DbSet<AppointmentService> AppointmentServices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //keeps the default identity mappings (AspNetUsers, AspNetRoles, etc.)
        base.OnModelCreating(modelBuilder);

        //this join table uses two ids together as one unique key
        //so the same service can't be added twice to the same appointment
        modelBuilder.Entity<AppointmentService>()
            .HasKey(x => new { x.AppointmentId, x.ServiceId });

        //one appointment can have many rows in the join table
        //each join row points back to exactly one appointment
        modelBuilder.Entity<AppointmentService>()
            .HasOne(x => x.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(x => x.AppointmentId);

        //one service can also appear in many join rows (across many appointments)
        //each join row points back to exactly one service
        modelBuilder.Entity<AppointmentService>()
            .HasOne(x => x.Service)
            .WithMany(s => s.AppointmentServices)
            .HasForeignKey(x => x.ServiceId);
    }
}
