using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        // GET: Payment
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            ViewBag.UserId = userId;

            var payments = GetPaymentsForUser(userRole ?? "User", tenantId ?? "default", userId ?? "");
            return View(payments);
        }

        // GET: Payment/Details/5
        public IActionResult Details(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var payment = GetPaymentById(id, userRole ?? "User", tenantId ?? "default");
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payment/Create
        public IActionResult Create(int licenseId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            ViewBag.LicenseId = licenseId;

            var license = GetLicenseById(licenseId, userRole ?? "User", tenantId ?? "default");
            if (license == null)
            {
                return NotFound();
            }

            ViewBag.License = license;
            return View();
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int licenseId, string paymentMethod, decimal amount, string cardNumber, string expiryDate, string cvv, string cardholderName)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (ModelState.IsValid)
            {
                var payment = ProcessPayment(licenseId, paymentMethod, amount, cardNumber, expiryDate, cvv, cardholderName, userRole, tenantId, userId);
                if (payment != null)
                {
                    TempData["Success"] = "Payment processed successfully!";
                    return RedirectToAction(nameof(Details), new { id = payment.Id });
                }
            }

            TempData["Error"] = "Payment processing failed. Please try again.";
            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            ViewBag.LicenseId = licenseId;
            ViewBag.License = GetLicenseById(licenseId, userRole ?? "User", tenantId ?? "default");
            return View();
        }

        // GET: Payment/Refund/5
        public IActionResult Refund(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var payment = GetPaymentById(id, userRole ?? "User", tenantId ?? "default");
            if (payment == null)
            {
                return NotFound();
            }

            if (payment.Status != "Completed")
            {
                TempData["Error"] = "Only completed payments can be refunded.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(payment);
        }

        // POST: Payment/Refund/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refund(int id, string reason)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var success = ProcessRefund(id, reason ?? "", userRole ?? "User", tenantId ?? "default");
            if (success)
            {
                TempData["Success"] = "Refund processed successfully!";
            }
            else
            {
                TempData["Error"] = "Refund processing failed. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Payment/Receipt/5
        public IActionResult Receipt(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var payment = GetPaymentById(id, userRole ?? "User", tenantId ?? "default");
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        #region Private Methods

        private dynamic GetPaymentsForUser(string role, string tenantId, string userId)
        {
            // Mock data - in real app, this would come from database/API
            switch (role)
            {
                case "Administrator":
                    return new object[]
                    {
                        new { Id = 1, LicenseId = 1, LicenseNumber = "LIC-2024-001", Amount = 500.00m, PaymentMethod = "Credit Card", Status = "Completed", TransactionId = "TXN-001", CreatedDate = DateTime.Parse("2024-02-01"), ProcessedDate = DateTime.Parse("2024-02-01"), CardholderName = "John Doe" },
                        new { Id = 2, LicenseId = 2, LicenseNumber = "LIC-2024-002", Amount = 750.00m, PaymentMethod = "Bank Transfer", Status = "Pending", TransactionId = "TXN-002", CreatedDate = DateTime.Parse("2024-02-02"), ProcessedDate = (DateTime?)null, CardholderName = "Jane Smith" },
                        new { Id = 3, LicenseId = 3, LicenseNumber = "LIC-2024-003", Amount = 300.00m, PaymentMethod = "Credit Card", Status = "Refunded", TransactionId = "TXN-003", CreatedDate = DateTime.Parse("2024-02-03"), ProcessedDate = DateTime.Parse("2024-02-03"), CardholderName = "Bob Johnson" }
                    };

                case "AgencyStaff":
                    return new object[]
                    {
                        new { Id = 4, LicenseId = 1, LicenseNumber = "LIC-2024-001", Amount = 500.00m, PaymentMethod = "Credit Card", Status = "Completed", TransactionId = "TXN-004", CreatedDate = DateTime.Parse("2024-02-01"), ProcessedDate = DateTime.Parse("2024-02-01"), CardholderName = "Alice Brown" },
                        new { Id = 5, LicenseId = 4, LicenseNumber = "LIC-2024-004", Amount = 600.00m, PaymentMethod = "PayPal", Status = "Completed", TransactionId = "TXN-005", CreatedDate = DateTime.Parse("2024-02-02"), ProcessedDate = DateTime.Parse("2024-02-02"), CardholderName = "Charlie Wilson" }
                    };

                case "Applicant":
                    return new object[]
                    {
                        new { Id = 6, LicenseId = 1, LicenseNumber = "LIC-2024-001", Amount = 500.00m, PaymentMethod = "Credit Card", Status = "Completed", TransactionId = "TXN-006", CreatedDate = DateTime.Parse("2024-02-01"), ProcessedDate = DateTime.Parse("2024-02-01"), CardholderName = "David Lee" },
                        new { Id = 7, LicenseId = 5, LicenseNumber = "APP-2024-002", Amount = 400.00m, PaymentMethod = "Credit Card", Status = "Pending", TransactionId = "TXN-007", CreatedDate = DateTime.Parse("2024-02-02"), ProcessedDate = (DateTime?)null, CardholderName = "Eve Martinez" }
                    };

                default:
                    return new object[0];
            }
        }

        private dynamic GetPaymentById(int id, string role = "User", string tenantId = "default")
        {
            var payments = GetPaymentsForUser(role, tenantId, "") as object[];
            return payments?.FirstOrDefault(p => p.GetType().GetProperty("Id")?.GetValue(p)?.ToString() == id.ToString());
        }

        private dynamic GetLicenseById(int id, string role = "User", string tenantId = "default")
        {
            // Mock license data
            return new { Id = id, LicenseNumber = $"LIC-2024-{id:D3}", Fee = 500.00m, Type = "Business License", Status = "Approved" };
        }

        private dynamic ProcessPayment(int licenseId, string paymentMethod, decimal amount, string cardNumber, string expiryDate, string cvv, string cardholderName, string role, string tenantId, string userId)
        {
            // Mock payment processing - in real app, this would integrate with payment gateway
            if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(expiryDate) || string.IsNullOrEmpty(cvv))
            {
                return null;
            }

            return new
            {
                Id = new Random().Next(100, 999),
                LicenseId = licenseId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = "Completed",
                TransactionId = $"TXN-{new Random().Next(1000, 9999)}",
                CreatedDate = DateTime.Now,
                ProcessedDate = DateTime.Now,
                CardholderName = cardholderName
            };
        }

        private bool ProcessRefund(int paymentId, string reason, string role = "User", string tenantId = "default")
        {
            // Mock refund processing - in real app, this would integrate with payment gateway
            return true;
        }

        #endregion
    }
}
