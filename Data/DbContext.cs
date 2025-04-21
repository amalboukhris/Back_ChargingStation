using ChargingStation.Models;
using Microsoft.EntityFrameworkCore;

namespace ChargingStation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information); // Log SQL queries
        }

        // Tables principales
        public DbSet<Station> Stations { get; set; }
        public DbSet<ChargePoint> ChargePoints { get; set; }
        public DbSet<Connector> Connectors { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // Utilisateurs
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<MeterValue> MeterValues { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'héritage User
            modelBuilder.Entity<Admin>().HasBaseType<User>();
            modelBuilder.Entity<Client>().HasBaseType<User>();

            // Configuration des Clients
            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                //entity.HasMany(c => c.Reservations)
                //    .WithOne(r => r.User)
                //    .HasForeignKey(r => r.UserId)
                //    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Reservation>()
                 .ToTable("Reservations")
    .HasOne(r => r.User)
    .WithMany(u => u.Reservations)
    .HasForeignKey(r => r.UserId);


            // Configuration des Stations
            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasMany(s => s.ChargePoints)
                    .WithOne(cp => cp.Station)
                    .HasForeignKey(cp => cp.StationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration des ChargePoints
            modelBuilder.Entity<ChargePoint>(entity =>
            {
                entity.HasIndex(cp => cp.ChargePointId)
                    .IsUnique();

                entity.HasMany(cp => cp.Connectors)
                    .WithOne(c => c.ChargePoint)
                    .HasForeignKey(c => c.ChargePointId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(cp => cp.Transactions)
                    .WithOne(t => t.ChargePoint)
                    .HasForeignKey(t => t.ChargePointId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // In AppDbContext.cs
            // In AppDbContext.cs
            modelBuilder.Entity<Connector>(entity =>
            {
                entity.ToTable("Connectors");
                entity.Property(c => c.ConnectorId).HasColumnName("ConnectorId");
                entity.Property(c => c.ChargePointId).HasColumnName("ChargePointId");
                entity.HasKey(c => c.Id);

                // Explicit PostgreSQL identity configuration
                entity.Property(c => c.Id)
                      .UseIdentityAlwaysColumn()
                      .IsRequired();

                // Business key constraint
                entity.HasIndex(c => new { c.ChargePointId, c.ConnectorId })
                      .IsUnique();

                // Relationships
                entity.HasMany(c => c.Reservations)
                    .WithOne(r => r.Connector)
                    .HasForeignKey(r => r.ConnectorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Transactions)
                    .WithOne(t => t.Connector)
                    .HasForeignKey(t => t.ConnectorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration des Réservations
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasIndex(r => r.ReservationCode)
                    .IsUnique();

                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            // Configuration des Transactions
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(t => t.StartTime)
                    .HasColumnType("timestamp with time zone");

                entity.Property(t => t.EndTime)
                    .HasColumnType("timestamp with time zone");
            });

            // Configuration pour PostgreSQL des dates
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ChargePoint>()
                .Property(cp => cp.LastHeartbeat)
                .HasColumnType("timestamp with time zone");
        }
    }
}