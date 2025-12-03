using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userFullName = User.FindFirst("FullName")?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.UserFullName = userFullName;
            ViewBag.TenantId = tenantId;

            // Get dashboard statistics based on role
            var model = GetDashboardData(userRole, tenantId);

            return View(model);
        }

        private dynamic GetDashboardData(string role, string tenantId)
        {
            switch (role)
            {
                case "Administrator":
                    return new
                    {
                        Title = "Administrator Dashboard",
                        TotalLicenses = 1250,
                        ActiveLicenses = 890,
                        PendingApplications = 45,
                        RenewalsDue = 67,
                        TotalAgencies = 12,
                        SystemHealth = "Good",
                        RecentActivity = new[]
                        {
                            new { Action = "New license application", User = "John Doe", Time = "2 hours ago" },
                            new { Action = "License renewal approved", User = "Jane Smith", Time = "4 hours ago" },
                            new { Action = "New agency registered", User = "System", Time = "6 hours ago" }
                        }
                    };

                case "AgencyStaff":
                    return new
                    {
                        Title = "Agency Dashboard",
                        AgencyName = GetAgencyName(tenantId),
                        TotalLicenses = 156,
                        ActiveLicenses = 124,
                        PendingApplications = 8,
                        RenewalsDue = 12,
                        ProcessingTime = "2.3 days",
                        RecentActivity = new[]
                        {
                            new { Action = "License application submitted", User = "Current User", Time = "1 hour ago" },
                            new { Action = "Document uploaded", User = "Current User", Time = "3 hours ago" },
                            new { Action = "Payment processed", User = "Finance Team", Time = "5 hours ago" }
                        }
                    };

                case "Applicant":
                    return new
                    {
                        Title = "Applicant Dashboard",
                        MyApplications = 3,
                        ActiveLicenses = 1,
                        PendingApplications = 2,
                        RenewalsDue = 0,
                        NextAction = "Upload documents for application #APP-2024-002",
                        RecentActivity = new[]
                        {
                            new { Action = "Application submitted", Reference = "APP-2024-003", Time = "1 day ago", Status = "Under Review" },
                            new { Action = "Document requested", Reference = "APP-2024-002", Time = "2 days ago", Status = "Pending" },
                            new { Action = "License approved", Reference = "APP-2024-001", Time = "1 week ago", Status = "Active" }
                        }
                    };

                default:
                    return new
                    {
                        Title = "Dashboard",
                        Message = "No dashboard data available for your role."
                    };
            }
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
