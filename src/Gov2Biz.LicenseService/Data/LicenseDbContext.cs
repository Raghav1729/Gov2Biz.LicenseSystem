using Gov2Biz.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Gov2Biz.LicenseService.Data
{
    public class LicenseDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _tenantId;

        public LicenseDbContext(DbContextOptions<LicenseDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
        }

        public LicenseDbContext(DbContextOptions<LicenseDbContext> options, string tenantId) : base(options)
        {
            _tenantId = tenantId;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<LicenseApplication> LicenseApplications { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure multi-tenancy
            ConfigureMultiTenancy(modelBuilder);

            // Configure entities
            ConfigureUser(modelBuilder);
            ConfigureAgency(modelBuilder);
            ConfigureLicense(modelBuilder);
            ConfigureLicenseApplication(modelBuilder);
            ConfigureTenant(modelBuilder);

            // Seed data
            SeedData(modelBuilder);
        }

        private void ConfigureMultiTenancy(ModelBuilder modelBuilder)
        {
            // Global query filter for tenant isolation
            if (!string.IsNullOrEmpty(_tenantId))
            {
                modelBuilder.Entity<User>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantId);
                modelBuilder.Entity<License>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantId);
                modelBuilder.Entity<LicenseApplication>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantId);
                modelBuilder.Entity<Agency>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantId);
            }
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).HasMaxLength(50);
                entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
            });
        }

        private void ConfigureAgency(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.Code, e.TenantId }).IsUnique();
            });
        }

        private void ConfigureLicense(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.LicenseNumber, e.TenantId }).IsUnique();
            });
        }

        private void ConfigureLicenseApplication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LicenseApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApplicationNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LicenseType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AgencyId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReviewerId).HasMaxLength(50);
                entity.HasIndex(e => new { e.ApplicationNumber, e.TenantId }).IsUnique();
            });
        }

        private void ConfigureTenant(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Domain).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConnectionString).HasMaxLength(500);
                entity.HasIndex(e => e.Domain).IsUnique();
            });
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { Id = "tenant-001", Name = "Government Agency 1", Domain = "agency1.gov", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tenant { Id = "tenant-002", Name = "Government Agency 2", Domain = "agency2.gov", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed agencies
            modelBuilder.Entity<Agency>().HasData(
                new Agency { Id = "HEALTH", Name = "Department of Health", Code = "HEALTH", TenantId = "tenant-001", Description = "Healthcare professional licensing", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agency { Id = "CONSTRUCTION", Name = "Construction Board", Code = "CONSTRUCTION", TenantId = "tenant-001", Description = "Construction contractor licensing", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agency { Id = "FINANCE", Name = "Financial Services Authority", Code = "FINANCE", TenantId = "tenant-002", Description = "Financial services licensing", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Email = "admin@gov.com", FirstName = "Admin", LastName = "User", Role = "Administrator", TenantId = "tenant-001", AgencyId = "HEALTH", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Id = 2, Email = "staff@agency.com", FirstName = "Staff", LastName = "User", Role = "AgencyStaff", TenantId = "tenant-001", AgencyId = "HEALTH", CreatedAt = DateTime.UtcNow, IsActive = true },
                new User { Id = 3, Email = "user@example.com", FirstName = "Regular", LastName = "User", Role = "Applicant", TenantId = "tenant-002", CreatedAt = DateTime.UtcNow, IsActive = true }
            );
        }

        public override int SaveChanges()
        {
            UpdateTenantProperties();
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTenantProperties();
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTenantProperties()
        {
            if (!string.IsNullOrEmpty(_tenantId))
            {
                var entries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity.GetType().GetProperty("TenantId") != null);

                foreach (var entry in entries)
                {
                    entry.Property("TenantId").CurrentValue = _tenantId;
                }
            }
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entry.Entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                    {
                        createdAtProperty.SetValue(entry.Entity, DateTime.UtcNow);
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                    if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
                    {
                        updatedAtProperty.SetValue(entry.Entity, DateTime.UtcNow);
                    }
                }
            }
        }
    }
}
