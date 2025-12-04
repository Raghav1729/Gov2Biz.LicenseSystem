using Gov2Biz.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.GatewayResponse).HasMaxLength(1000);
                entity.HasIndex(e => e.TransactionId).IsUnique();
            });
        }
    }
}
