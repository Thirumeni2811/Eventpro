using Eventpro.DataAccess;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Interfaces.IProvide;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.ResponseFormat;
using Eventpro.Service;
using Eventpro.Service.Constants;
using Eventpro.Service.Helpers;
using Eventpro.Service.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// This creates a builder object used to configure the app (services, middleware, etc.)
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// MVC pattern support
builder.Services.AddControllersWithViews();

// Adds session middleware to keep user data across requests
builder.Services.AddSession();

// Configure EF Core with SQL Server
// Lifetime is Scoped by default.
// A new AppDbContext is created per HTTP request.
// Scoped helps maintain a consistent unit-of-work across a request
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind JwtSettings from appsettings.json
// Dependency Injection is a design pattern used to achieve loose coupling between classes.
// loose coupling
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// load jwtSettings values from appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Register JwtHelper (depends on IOptions<JwtSettings>)
builder.Services.AddScoped<JwtHelper>();

// Scoped  -> A new instance is provided for each HTTP request
// Transient -> a new instance is created every time it's injected
// Singleton -> only one instance is created and shared across all requests

// Register Repositories (DataAccess Layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
builder.Services.AddScoped<IServRepository, ServRepository>();
builder.Services.AddScoped<IProvideRepository, ProvideRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

// Register Services (Service Layer)
builder.Services.AddScoped<IServiceResponseFactory, ServiceResponseFactory>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<IServService, ServService>();
builder.Services.AddScoped<IProvideService, ProvideService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.HttpContext.Request.Cookies["Token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Compiles all configuration and middleware into a runnable app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirects HTTP requests to HTTPS
app.UseHttpsRedirection();

// Serves static files (CSS, JS, images, etc.)
app.UseStaticFiles();

// Adds routing middleware to support URL pattern matching
app.UseRouting();

// Enables session handling. Must be added before authorization
app.UseSession();

// Middlewares
app.UseAuthentication();
app.UseAuthorization(); // Verifies access policies (roles, claims, etc.)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Runs the app and starts listening for incoming HTTP requests
app.Run();


// Lifetime                     Description	                            Example Use
// Singleton	  One instance throughout app's lifetime	    Logging, caching, settings
// Scoped	      One instance per HTTP request	                    DbContext, Services
// Transient	  New instance every time it's injected	        Lightweight stateless logic
