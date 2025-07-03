using Event_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Provide> Provides { get; set; }
        public DbSet<Eventsss> Events { get; set; }

        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Gallery> Gallery { get; set; }

    }
}
