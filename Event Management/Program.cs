using Eventpro.DataAccess;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.ResponseFormat;
using Eventpro.Service;
using Eventpro.Service.Constants;
using Eventpro.Service.Helpers;
using Eventpro.Service.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddSession();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind JwtSettings from appsettings.json
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Register JwtHelper (depends on IOptions<JwtSettings>)
builder.Services.AddScoped<JwtHelper>();

// Register Repositories (DataAccess Layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register ServiceResponseFactory
builder.Services.AddScoped<IServiceResponseFactory, ServiceResponseFactory>();

// Register Services (Service Layer)
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add session middleware before authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
