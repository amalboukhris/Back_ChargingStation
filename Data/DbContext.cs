using ChargingStation.Models;
using Microsoft.EntityFrameworkCore;

namespace ChargingStation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Client> Clients { get; set; }

        public DbSet<ChargingStationM> ChargingStations { get; set; }
        public DbSet<Borne> Bornes { get; set; }

        // Utilisez NotificationData pour les notifications en base de données
        public DbSet<NotificationData> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>().HasBaseType<User>();
            modelBuilder.Entity<Client>().HasBaseType<User>();

            // Configuration des relations entre Client et NotificationData
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Notifications)
                .WithOne(n => n.Client)
                .HasForeignKey(n => n.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Autres configurations...
            modelBuilder.Entity<ChargingStationM>()
                .HasMany(cs => cs.Bornes)
                .WithOne(b => b.ChargingStation)
                .HasForeignKey(b => b.ChargingStationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        internal async Task GetByIdAsync(string? userId)
        {
            throw new NotImplementedException();
        }
    }
}
