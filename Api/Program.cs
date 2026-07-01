using AcademicGateway.Application;
using AcademicGateway.Application.Common.Behaviors;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure; // <-- Natively resolves your new Infrastructure extension
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Centralized Layer Registrations
// ==========================================
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration); // Loads persistence, identity framework, and decoupled interceptors safely

// ==========================================
// 2. Configure Presentation JWT Authentication Entry Guards
// ==========================================
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
    };
});

// ==========================================
// 3. Register Presentation Pipeline Elements (MediatR & FluentValidation)
// ==========================================
builder.Services.AddValidatorsFromAssemblyContaining<RegisterStudentCommandValidator>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(RegisterStudentCommand).Assembly);

    // Injects our validation cross-cutting pipeline behavior before commands hit concrete handlers
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// ==========================================
// 4. Register Web API & Explorer Utilities
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academic Gateway API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// ==========================================
// 5. Cross-Cutting UI Utilities (Exceptions, CORS)
// ==========================================
builder.Services.AddExceptionHandler<Api.Infrastructure.CustomExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ==========================================
// 6. Configure HTTP Middleware Pipeline
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseExceptionHandler(); // Activates CustomExceptionHandler natively
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 7. Relational Database Migration & Seeding Execution
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Automatically executes outstanding schema definitions down onto the active cluster
        await context.Database.MigrateAsync();

        // Resolve dependencies explicitly matching the Guid tracking keys assigned onto your identity core structures
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>(); // Realigned type parameter to use your clean Guid contract keys

        // Executes seeder parameters cleanly
        await ApplicationDbContextSeed.SeedDefaultUserAndDataAsync(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database workspace cluster.");
    }
}

app.Run();

public partial class Program { }