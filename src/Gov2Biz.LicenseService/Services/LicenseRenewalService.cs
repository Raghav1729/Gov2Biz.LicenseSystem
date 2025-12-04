using Gov2Biz.LicenseService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace Gov2Biz.LicenseService.Services
{
    public class LicenseRenewalService
    {
        private readonly LicenseDbContext _context;
        private readonly ILogger<LicenseRenewalService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public LicenseRenewalService(
            LicenseDbContext context,
            ILogger<LicenseRenewalService> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task CheckExpiringLicenses()
        {
            _logger.LogInformation("Checking for expiring licenses...");

            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);

            // Get licenses expiring in 30 days (for first notification)
            var licensesExpiringIn30Days = await _context.Licenses
                .Include(l => l.Applicant)
                .Include(l => l.Agency)
                .Where(l => l.Status == "Active" && 
                           l.ExpiresAt.HasValue &&
                           l.ExpiresAt.Value <= thirtyDaysFromNow &&
                           l.ExpiresAt.Value > DateTime.UtcNow)
                .ToListAsync();

            // Get licenses expiring in 7 days (for final reminder)
            var licensesExpiringIn7Days = licensesExpiringIn30Days
                .Where(l => l.ExpiresAt.HasValue && l.ExpiresAt.Value <= sevenDaysFromNow)
                .ToList();

            // Schedule notifications for 30-day expirations
            foreach (var license in licensesExpiringIn30Days)
            {
                await ScheduleRenewalNotification(license, 30);
            }

            // Schedule notifications for 7-day expirations
            foreach (var license in licensesExpiringIn7Days)
            {
                await ScheduleRenewalNotification(license, 7);
            }

            _logger.LogInformation($"Processed {licensesExpiringIn30Days.Count} licenses expiring soon");
        }

        private async Task ScheduleRenewalNotification(License license, int daysUntilExpiry)
        {
            try
            {
                var notification = new Notification
                {
                    Title = $"License Expiring in {daysUntilExpiry} Days",
                    Message = $"Your license {license.LicenseNumber} for {license.Type} will expire in {daysUntilExpiry} days on {license.ExpiresAt:yyyy-MM-dd}. Please renew your license to avoid interruption.",
                    Type = daysUntilExpiry <= 7 ? "Urgent" : "Reminder",
                    RecipientId = license.ApplicantId,
                    EntityReference = license.Id.ToString(),
                    TenantId = license.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Schedule background job to send email/notification
                _backgroundJobClient.Schedule<LicenseRenewalService>(
                    service => service.SendRenewalNotification(notification.Id),
                    TimeSpan.FromMinutes(1));

                _logger.LogInformation($"Scheduled renewal notification for license {license.LicenseNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scheduling renewal notification for license {license.LicenseNumber}");
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendRenewalNotification(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.Recipient)
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    _logger.LogWarning($"Notification {notificationId} not found");
                    return;
                }

                // In a real implementation, this would send an email, SMS, or push notification
                // For now, we'll just log it and mark as sent
                _logger.LogInformation($"Sending renewal notification to {notification.Recipient.Email}: {notification.Title}");

                // Mark notification as processed (in a real system, you'd have a separate status)
                notification.IsRead = false; // Keep as unread so user sees it
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Renewal notification sent successfully for notification {notificationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending renewal notification {notificationId}");
                throw;
            }
        }

        public async Task AutoRenewLicenses()
        {
            _logger.LogInformation("Checking for licenses eligible for auto-renewal...");

            var expiredLicenses = await _context.Licenses
                .Include(l => l.Applicant)
                .Include(l => l.Agency)
                .Where(l => l.Status == "Active" && 
                           l.ExpiresAt.HasValue &&
                           l.ExpiresAt.Value <= DateTime.UtcNow.AddDays(-1)) // Expired yesterday
                .ToListAsync();

            foreach (var license in expiredLicenses)
            {
                // Check if auto-renewal is enabled (this could be a property on the license)
                // For now, we'll just create a renewal notification
                await CreateExpiredLicenseNotification(license);
            }

            _logger.LogInformation($"Processed {expiredLicenses.Count} expired licenses");
        }

        private async Task CreateExpiredLicenseNotification(License license)
        {
            try
            {
                var notification = new Notification
                {
                    Title = "License Expired",
                    Message = $"Your license {license.LicenseNumber} for {license.Type} expired on {license.ExpiresAt:yyyy-MM-dd}. Please contact your agency immediately to renew your license.",
                    Type = "Critical",
                    RecipientId = license.ApplicantId,
                    EntityReference = license.Id.ToString(),
                    TenantId = license.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send immediate notification
                _backgroundJobClient.Enqueue<LicenseRenewalService>(
                    service => service.SendRenewalNotification(notification.Id));

                _logger.LogInformation($"Created expired license notification for license {license.LicenseNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating expired license notification for license {license.LicenseNumber}");
            }
        }

        public async Task<LicenseRenewalJobDto> GetRenewalJobInfo(int licenseId)
        {
            var license = await _context.Licenses
                .Include(l => l.Applicant)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                throw new KeyNotFoundException($"License with ID {licenseId} not found");

            var daysUntilExpiry = license.ExpiresAt.HasValue 
                ? (license.ExpiresAt.Value - DateTime.UtcNow).Days 
                : 0;

            return new LicenseRenewalJobDto
            {
                LicenseId = license.Id,
                LicenseNumber = license.LicenseNumber,
                ExpiresAt = license.ExpiresAt ?? DateTime.MinValue,
                ApplicantEmail = license.Applicant?.Email ?? "",
                ApplicantName = $"{license.Applicant?.FirstName} {license.Applicant?.LastName}",
                AgencyId = license.AgencyId,
                TenantId = license.TenantId,
                DaysUntilExpiry = daysUntilExpiry,
                IsNotified = daysUntilExpiry <= 30, // Simplified logic
                LastNotifiedAt = daysUntilExpiry <= 30 ? DateTime.UtcNow.AddDays(-1) : null
            };
        }
    }
}
