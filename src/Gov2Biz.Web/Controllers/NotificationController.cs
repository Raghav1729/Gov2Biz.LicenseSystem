using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        // GET: Notification
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            ViewBag.UserId = userId;

            var notifications = GetNotificationsForUser(userRole, tenantId, userId);
            return View(notifications);
        }

        // GET: Notification/Details/5
        public IActionResult Details(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var notification = GetNotificationById(id, userRole, tenantId);
            if (notification == null)
            {
                return NotFound();
            }

            // Mark as read
            MarkNotificationAsRead(id);

            return View(notification);
        }

        // GET: Notification/Create
        public IActionResult Create()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            return View();
        }

        // POST: Notification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string message, string notificationType, string recipientType, string recipientEmail, int? licenseId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(message))
            {
                var notification = CreateNotification(message, notificationType, recipientType, recipientEmail, licenseId, userRole, tenantId, userId);
                if (notification != null)
                {
                    TempData["Success"] = "Notification sent successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Error"] = "Failed to send notification. Please try again.";
            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            return View();
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var success = MarkNotificationAsRead(id);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllAsRead()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var success = MarkAllNotificationsAsRead(userRole, tenantId, userId);
            if (success)
            {
                TempData["Success"] = "All notifications marked as read!";
            }
            else
            {
                TempData["Error"] = "Failed to mark notifications as read.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Notification/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var success = DeleteNotification(id, userRole, tenantId);
            if (success)
            {
                TempData["Success"] = "Notification deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete notification. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private dynamic GetNotificationsForUser(string role, string tenantId, string userId)
        {
            // Mock data - in real app, this would come from database/API
            switch (role)
            {
                case "Administrator":
                    return new object[]
                    {
                        new { Id = 1, Message = "New license application submitted", Type = "License Application", Recipient = "admin@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-01"), IsRead = false, LicenseId = 2, LicenseNumber = "LIC-2024-002", Priority = "High" },
                        new { Id = 2, Message = "Document upload pending approval", Type = "Document Review", Recipient = "admin@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-02"), IsRead = true, LicenseId = 1, LicenseNumber = "LIC-2024-001", Priority = "Medium" },
                        new { Id = 3, Message = "System maintenance scheduled", Type = "System", Recipient = "admin@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-03"), IsRead = false, LicenseId = (int?)null, LicenseNumber = (string)null, Priority = "Low" }
                    };

                case "AgencyStaff":
                    return new object[]
                    {
                        new { Id = 4, Message = "License renewal request received", Type = "License Renewal", Recipient = "agency@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-01"), IsRead = false, LicenseId = 1, LicenseNumber = "LIC-2024-001", Priority = "High" },
                        new { Id = 5, Message = "New document uploaded for review", Type = "Document Review", Recipient = "agency@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-02"), IsRead = true, LicenseId = 4, LicenseNumber = "LIC-2024-004", Priority = "Medium" }
                    };

                case "Applicant":
                    return new object[]
                    {
                        new { Id = 6, Message = "Your license application has been approved", Type = "License Status", Recipient = "applicant@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-01"), IsRead = false, LicenseId = 1, LicenseNumber = "LIC-2024-001", Priority = "High" },
                        new { Id = 7, Message = "Document upload successful", Type = "Document Upload", Recipient = "applicant@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-02"), IsRead = true, LicenseId = 5, LicenseNumber = "APP-2024-002", Priority = "Medium" },
                        new { Id = 8, Message = "License expiry reminder", Type = "License Renewal", Recipient = "applicant@gov2biz.com", CreatedDate = DateTime.Parse("2024-02-03"), IsRead = false, LicenseId = 1, LicenseNumber = "LIC-2024-001", Priority = "High" }
                    };

                default:
                    return new object[0];
            }
        }

        private dynamic GetNotificationById(int id, string role, string tenantId)
        {
            var notifications = GetNotificationsForUser(role, tenantId, "") as object[];
            return notifications?.FirstOrDefault(n => n.GetType().GetProperty("Id")?.GetValue(n)?.ToString() == id.ToString());
        }

        private dynamic CreateNotification(string message, string notificationType, string recipientType, string recipientEmail, int? licenseId, string role, string tenantId, string userId)
        {
            // Mock creation - in real app, this would send email/SMS and create database record
            return new
            {
                Id = new Random().Next(100, 999),
                Message = message,
                Type = notificationType,
                Recipient = recipientEmail,
                CreatedDate = DateTime.Now,
                IsRead = false,
                LicenseId = licenseId,
                SentBy = userId
            };
        }

        private bool MarkNotificationAsRead(int id)
        {
            // Mock update - in real app, this would update database
            return true;
        }

        private bool MarkAllNotificationsAsRead(string role, string tenantId, string userId)
        {
            // Mock update - in real app, this would update database
            return true;
        }

        private bool DeleteNotification(int id, string role, string tenantId)
        {
            // Mock deletion - in real app, this would delete from database
            return true;
        }

        #endregion
    }
}
