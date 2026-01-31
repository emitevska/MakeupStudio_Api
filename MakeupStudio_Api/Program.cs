using MakeupStudio_Api.Data;
using MakeupStudio_Api.Models;
using MakeupStudio_Api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//--------------------
//services
//--------------------

//adds support for controllers (your api endpoints)
builder.Services.AddControllers();

//database connection (ef core + postgresql)
//reads the connection string from appsettings.json ("DefaultConnection")
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

//register repositories for dependency injection
//so controllers can ask for interfaces and get real implementations
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

//identity setup (users + roles) backed by ef core tables
//this creates/uses tables like AspNetUsers, AspNetRoles, etc.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//jwt authentication setup
//this forces jwt as the default (so swagger/api won't try cookie redirects)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //reads jwt settings from appsettings.json -> "Jwt" section
    var jwt = builder.Configuration.GetSection("Jwt");

    //rules for validating incoming tokens
    //if any rule fails, request becomes 401 unauthorized
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],

        //this is the secret key used to sign and validate tokens
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        )
    };
});

//enables [Authorize] and role checks like [Authorize(Roles="Admin")]
builder.Services.AddAuthorization();

//swagger setup (api documentation)
//also adds the "Authorize" button so we can paste bearer tokens in swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //tells swagger what "Bearer token" means and where it goes (Authorization header)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    //makes swagger apply bearer auth globally so endpoints show the lock icon
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

//--------------------
//middleware
//--------------------

//swagger is enabled only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//forces https redirection for safer requests
app.UseHttpsRedirection();

//enables routing so endpoints like /api/services can be matched
app.UseRouting();

//auth middleware reads the jwt token and builds the User (claims + roles)
//must run before authorization
app.UseAuthentication();

//authorization enforces [Authorize] attributes and role rules
app.UseAuthorization();

//maps controller routes (so your controllers actually become endpoints)
app.MapControllers();

//runs the seeder at startup
//creates roles, creates admin user, inserts default services if missing
using (var scope = app.Services.CreateScope())
{
    await MakeupStudio_Api.Seed.DataSeeder
        .SeedAsync(scope.ServiceProvider);
}

app.Run();
