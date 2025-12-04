using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Gov2Biz.LicenseService.Data;

namespace Gov2Biz.LicenseService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LicenseDbContext>
    {
        public LicenseDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LicenseDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=Gov2BizLicenseSystem;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");

            return new LicenseDbContext(optionsBuilder.Options, "design-time");
        }
    }
}
