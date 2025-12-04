using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Gov2Biz.Web.Services;
using Gov2Biz.Shared.DTOs;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILicenseServiceClient _licenseServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ILicenseServiceClient licenseServiceClient,
            INotificationServiceClient notificationServiceClient,
            ILogger<DashboardController> logger)
        {
            _licenseServiceClient = licenseServiceClient;
            _notificationServiceClient = notificationServiceClient;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userFullName = User.FindFirst("FullName")?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;
                var agencyId = User.FindFirst("AgencyId")?.Value;

                ViewBag.UserRole = userRole;
                ViewBag.UserFullName = userFullName;
                ViewBag.TenantId = tenantId;

                // Get real dashboard statistics from services
                var dashboardStats = await _licenseServiceClient.GetDashboardStatsAsync(agencyId);
                var notifications = new List<NotificationDto>();
                
                // Get notifications if user ID is available
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    notifications = await _notificationServiceClient.GetNotificationsAsync(userId);
                }

                // Get dashboard data based on role
                var model = GetDashboardData(userRole, tenantId, agencyId, dashboardStats, notifications);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                ViewBag.ErrorMessage = "Unable to load dashboard data. Please try again later.";
                return View(GetErrorDashboardData());
            }
        }

        private dynamic GetDashboardData(string role, string tenantId, string? agencyId, DashboardStatsDto stats, List<NotificationDto> notifications)
        {
            switch (role)
            {
                case "Administrator":
                    return new
                    {
                        Title = "Administrator Dashboard",
                        TotalLicenses = stats.TotalLicenses,
                        ActiveLicenses = stats.ActiveLicenses,
                        PendingApplications = stats.PendingApplications,
                        ApprovedApplications = stats.ApprovedApplications,
                        RejectedApplications = stats.RejectedApplications,
                        ExpiredLicenses = stats.ExpiredLicenses,
                        ExpiringSoonLicenses = stats.ExpiringSoonLicenses,
                        TotalRevenue = stats.TotalRevenue,
                        PendingPayments = stats.PendingPayments,
                        UnreadNotifications = stats.UnreadNotifications,
                        RecentActivity = stats.RecentActivities,
                        Notifications = notifications.Take(5).ToList()
                    };

                case "AgencyStaff":
                    return new
                    {
                        Title = "Agency Dashboard",
                        AgencyName = GetAgencyName(tenantId),
                        TotalLicenses = stats.TotalLicenses,
                        ActiveLicenses = stats.ActiveLicenses,
                        PendingApplications = stats.PendingApplications,
                        ApprovedApplications = stats.ApprovedApplications,
                        RejectedApplications = stats.RejectedApplications,
                        ExpiredLicenses = stats.ExpiredLicenses,
                        ExpiringSoonLicenses = stats.ExpiringSoonLicenses,
                        TotalRevenue = stats.TotalRevenue,
                        PendingPayments = stats.PendingPayments,
                        UnreadNotifications = stats.UnreadNotifications,
                        RecentActivity = stats.RecentActivities,
                        Notifications = notifications.Take(5).ToList()
                    };

                case "Applicant":
                    return new
                    {
                        Title = "Applicant Dashboard",
                        MyApplications = stats.TotalApplications,
                        ActiveLicenses = stats.ActiveLicenses,
                        PendingApplications = stats.PendingApplications,
                        ApprovedApplications = stats.ApprovedApplications,
                        RejectedApplications = stats.RejectedApplications,
                        ExpiredLicenses = stats.ExpiredLicenses,
                        ExpiringSoonLicenses = stats.ExpiringSoonLicenses,
                        UnreadNotifications = stats.UnreadNotifications,
                        RecentActivity = stats.RecentActivities,
                        Notifications = notifications.Take(5).ToList()
                    };

                default:
                    return new
                    {
                        Title = "Dashboard",
                        Message = "No dashboard data available for your role."
                    };
            }
        }

        private dynamic GetErrorDashboardData()
        {
            return new
            {
                Title = "Dashboard",
                Message = "Dashboard data is currently unavailable.",
                TotalLicenses = 0,
                ActiveLicenses = 0,
                PendingApplications = 0,
                ApprovedApplications = 0,
                RejectedApplications = 0,
                ExpiredLicenses = 0,
                ExpiringSoonLicenses = 0,
                TotalRevenue = 0,
                PendingPayments = 0,
                UnreadNotifications = 0,
                RecentActivity = new List<object>(),
                Notifications = new List<object>()
            };
        }

        private string GetAgencyName(string tenantId)
        {
            // In a real application, this would lookup from database
            return tenantId switch
            {
                "AGENCY001" => "Department of Transportation",
                "AGENCY002" => "Health Services Agency",
                "AGENCY003" => "Business Licensing Board",
                _ => "Your Agency"
            };
        }
    }
}
