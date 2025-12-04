using Gov2Biz.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.LicenseService.Data
{
    public class LicenseDbContext : DbContext
    {
        public LicenseDbContext(DbContextOptions<LicenseDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<LicenseApplication> LicenseApplications { get; set; }
        public DbSet<Agency> Agencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Agency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<LicenseApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApplicationNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LicenseType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReviewerId).HasMaxLength(50);
                entity.HasIndex(e => e.ApplicationNumber).IsUnique();
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agency>().HasData(
                new Agency { Id = "HEALTH", Name = "Department of Health", Code = "HEALTH", Description = "Healthcare professional licensing", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agency { Id = "CONSTRUCTION", Name = "Construction Board", Code = "CONSTRUCTION", Description = "Construction contractor licensing", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agency { Id = "FINANCE", Name = "Financial Services Authority", Code = "FINANCE", Description = "Financial services licensing", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Email = "admin@gov.com", FirstName = "Admin", LastName = "User", Role = "Administrator", AgencyId = "HEALTH", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Id = 2, Email = "staff@agency.com", FirstName = "Staff", LastName = "User", Role = "AgencyStaff", AgencyId = "HEALTH", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Id = 3, Email = "user@example.com", FirstName = "Regular", LastName = "User", Role = "Applicant", AgencyId = "", CreatedAt = DateTime.UtcNow, IsActive = true }
            );
        }
    }
}
