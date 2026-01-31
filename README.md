# Makeup Studio Appointment System (ASP.NET Core Web API)

## Project Overview
This project is a .NET 9.0 Web API developed as the final project for the Programming in .NET course.
The application provides a backend system for managing makeup services, client appointments, and user authentication.

No frontend is implemented. The API is tested using Swagger.

---

## Technologies Used
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL (Npgsql)
- ASP.NET Core Identity
- JWT Authentication
- Swagger / OpenAPI

---

## System Features

### Authentication & Authorization
- User registration and login
- JWT token generation
- Role-based authorization (Admin, Client)
- Secured endpoints using `[Authorize]`

### Services Management
- View all services (public)
- View service by ID
- Admin can create and delete services

### Appointment Management
- Clients can create appointments
- Clients can view their own appointments
- Clients can cancel their own appointments
- Admin can view all appointments
- Admin can update appointment status

---

## Business Logic
- Prevents double booking
- Calculates appointment duration based on selected services
- Handles many-to-many relationship between appointments and services

---

## Database
The database is managed using Entity Framework Core migrations and PostgreSQL.

Tables include:
- AspNetUsers
- AspNetRoles
- Appointments
- Services
- AppointmentServices

---

## Default Admin Account
A default admin account is created using a data seeder.


## How to Run the Project

### 1. Configure the database
Update the connection string in `appsettings.json`.

### 2. Apply migrations
Run:
Update-Database


### 3. Run the application
Swagger will open at:
https://localhost:7006/swagger


---

## API Testing
1. Register a user
2. Login to receive a JWT token
3. Click **Authorize** in Swagger
4. Paste: `token`
5. Test secured endpoints

---

## Project Structure
MakeupStudio_Api/
├── Controllers
├── Models
├── Dtos
├── Repositories
├── Data
├── Seed
├── Program.cs
├── appsettings.json
└── README.md
