using MakeupStudio_Api.Dtos;                 
using MakeupStudio_Api.Models;              
using Microsoft.AspNetCore.Identity;         
using Microsoft.AspNetCore.Mvc;              
using Microsoft.IdentityModel.Tokens;       
using System.IdentityModel.Tokens.Jwt;      
using System.Security.Claims;               
using System.Text;                          

namespace MakeupStudio_Api.Controllers;

[ApiController]

//base route: /api/Auth (because [controller] = Auth)
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    //identity service to create/find/update users in AspNetUsers table
    private readonly UserManager<ApplicationUser> _users;

    //identity service used to verify password sign-in
    private readonly SignInManager<ApplicationUser> _signIn;

    //identity service used to create or check roles in AspNetRoles table
    private readonly RoleManager<IdentityRole> _roles;

    //reads settings from appsettings.json (Jwt:Key, Issuer, Audience, ExpiresMinutes)
    private readonly IConfiguration _config;

    //Constructor
    public AuthController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        RoleManager<IdentityRole> roles,
        IConfiguration config
    )
    {
        _users = users;
        _signIn = signIn;
        _roles = roles;
        _config = config;
    }

    //=========================
    //REGISTER
    //=========================

    //POST: /api/Auth/register
    //creates a new user and assigns the default "Client" role
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        //checks if this email already exists
        var existing = await _users.FindByEmailAsync(dto.Email);

        //if user exists => 400 Bad Request
        if (existing is not null)
        {
            return BadRequest("Email already registered.");
        }

        //creates a new Identity user (stored in AspNetUsers)
        var user = new ApplicationUser
        {
            UserName = dto.Email, 
            Email = dto.Email
        };

        //creates user with hashed password
        var result = await _users.CreateAsync(user, dto.Password);

        //if password rules fail it returns identity errors
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        //making sure that the roles exist in the database
        await EnsureRoleExists("Client");
        await EnsureRoleExists("Admin");

        //assigning default role "Client" to the new user (AspNetUserRoles join table)
        await _users.AddToRoleAsync(user, "Client");

        return Ok("Registered successfully.");
    }

    //=========================
    //LOGIN
    //=========================
    //POST: /api/Auth/login
    //validates email and password and returns a JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        //find user by email
        var user = await _users.FindByEmailAsync(dto.Email);

        //if no user, it returns 401 Unauthorized
        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        //checks password 
        var passwordOk = await _signIn.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: false
        );

        if (!passwordOk.Succeeded)
        {
            return Unauthorized("Invalid credentials.");
        }

        //creates JWT token 
        var token = await CreateJwtToken(user);

        //returns token to the client 
        return Ok(new { token });
    }

    //=========================
    //JWT CREATION
    //=========================
    //builds and signs a JWT token using Jwt settings in appsettings.json
    private async Task<string> CreateJwtToken(ApplicationUser user)
    {
        //read Jwt section from configuration
        var jwt = _config.GetSection("Jwt");

        //basic claims stored inside the token
        var claims = new List<Claim>
        {
            //subject (commonly the user id)
            new(JwtRegisteredClaimNames.Sub, user.Id),

            //email claim
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),

            //identity standard claim (NameIdentifier = user id)
            new(ClaimTypes.NameIdentifier, user.Id),

            //identity standard claim (Name = username/email)
            new(ClaimTypes.Name, user.UserName ?? "")
        };

        //get user roles from AspNetUserRoles and add them into JWT as ClaimTypes.Role
        var roles = await _users.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        //create the signing key from Jwt:Key (must match the one used in Program.cs)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        //HS256 signing credentials
        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        //token expiration time
        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(jwt["ExpiresMinutes"] ?? "60")
        );

        //create JWT token object
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],      // must match TokenValidationParameters.ValidIssuer
            audience: jwt["Audience"],  // must match TokenValidationParameters.ValidAudience
            claims: claims,             // includes role claims for [Authorize(Roles="...")]
            expires: expires,
            signingCredentials: creds
        );

        //convert token object to a string (the one you paste into Swagger Authorize)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //=========================
    //ROLE HELPERS
    //=========================
    //creates a role in AspNetRoles if it doesn't exist yet
    private async Task EnsureRoleExists(string roleName)
    {
        if (!await _roles.RoleExistsAsync(roleName))
        {
            await _roles.CreateAsync(new IdentityRole(roleName));
        }
    }
}
