using GestionAirPort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionAirPort.data
{
    public class AirportContext : DbContext
    {
        public AirportContext(DbContextOptions<AirportContext> options)
            : base(options)
        {
        }

        public DbSet<Plane> Planes { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Traveller> Travellers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration pour Plane
            modelBuilder.Entity<Plane>(entity =>
            {
                // Configuration de base
                entity.HasKey(p => p.PlaneId);
                entity.ToTable("MyPlanes");
                entity.Property(p => p.Capacity).HasColumnName("PlaneCapacity");

                // Configuration du type d'avion
                entity.Property(p => p.PlaneType)
                      .HasConversion<string>()
                      .IsRequired();

                // Configuration de la relation avec Flights
                entity.HasMany(p => p.Flights)
                      .WithOne(f => f.Plane)
                      .HasForeignKey(f => f.PlaneFk)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configuration de la date de fabrication
                entity.Property(p => p.ManufactureDate)
                      .IsRequired()
                      .HasColumnType("datetime");

                // Configuration de la capacité
                entity.Property(p => p.Capacity)
                      .IsRequired()
                      .HasAnnotation("Range", new[] { 10, 1000 });
            });

            // Configuration pour Flight
            modelBuilder.ApplyConfiguration(new FlightConfiguration());

            // Configuration pour Passenger (FullName comme type possédé)
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.OwnsOne(p => p.FullName, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.Property(f => f.FirstName).IsRequired().HasMaxLength(50);
                    ownedNavigationBuilder.Property(f => f.LastName).IsRequired().HasMaxLength(50);
                });
            });

            // Configuration de l'héritage TPT (Table Per Type)
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staffs");
            });

            modelBuilder.Entity<Traveller>(entity =>
            {
                entity.ToTable("Travellers");
            });

            // Configuration de Ticket avec clé composite
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => new { t.FlightFk, t.PassengerFk });

                entity.HasOne(t => t.Flight)
                      .WithMany(f => f.Tickets)
                      .HasForeignKey(t => t.FlightFk)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Passenger)
                      .WithMany(p => p.Tickets)
                      .HasForeignKey(t => t.PassengerFk)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Configuration globale pour les propriétés DateTime
            configurationBuilder.Properties<DateTime>()
                              .HaveColumnType("datetime");

            base.ConfigureConventions(configurationBuilder);
        }
    }

    public class FlightConfiguration : IEntityTypeConfiguration<Flight>
    {
        public void Configure(EntityTypeBuilder<Flight> builder)
        {
            // Configuration principale pour Flight
            builder.HasKey(f => f.FlightId);

            builder.Property(f => f.FlightDate)
                  .IsRequired()
                  .HasColumnType("datetime");

            builder.Property(f => f.EstimatedDuration)
                  .IsRequired();

            // Relation avec Plane
            builder.HasOne(f => f.Plane)
                  .WithMany(p => p.Flights)
                  .HasForeignKey(f => f.PlaneFk)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            // Configuration des relations avec Ticket
            builder.HasMany(f => f.Tickets)
                  .WithOne(t => t.Flight)
                  .HasForeignKey(t => t.FlightFk)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}