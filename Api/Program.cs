using AcademicGateway.Application;
using AcademicGateway.Application.Common.Behaviors;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure;
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.ProjectInstances.Services; // Added to resolve the Domain Service factory tracking dependency
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AcademicGateway.Infrastructure.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Layer Registrations (Clean Architecture)
// ==========================================
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Domain Service Factory Registration: Unlocks the design-time container setup 
// required by the StartProjectCommandHandler during CLI migrations discovery loops.
builder.Services.AddTransient<LocalMilestoneFactory>();

// ==========================================
// 2. Authentication (JWT)
// ==========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
        };
    });

// ==========================================
// 3. MediatR & Pipeline Behaviors
// ==========================================
builder.Services.AddValidatorsFromAssemblyContaining<RegisterStudentCommandValidator>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(RegisterStudentCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// ==========================================
// 4. API & Swagger Configuration
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academic Gateway API", Version = "v1" });

    // Swagger UI Grouping Logic: 
    // Automatically tags endpoints based on their controller or folder name to prevent UI mess.
    c.TagActionsBy(api => new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Default" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// 5. Middleware Pipeline
// ==========================================
builder.Services.AddExceptionHandler<AcademicGateway.Api.Infrastructure.CustomExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 6. Database Seeding & Migration
// ==========================================
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await ApplicationDbContextSeed.SeedDefaultUserAndDataAsync(userManager, roleManager, context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

app.Run();

public partial class Program { }