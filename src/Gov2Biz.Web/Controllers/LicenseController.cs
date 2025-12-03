using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class LicenseController : Controller
    {
        // GET: License
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            // Get licenses based on user role
            var model = GetLicensesForUser(userRole, tenantId);

            return View(model);
        }

        // GET: License/Details/5
        public IActionResult Details(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var license = GetLicenseById(id, userRole, tenantId);
            
            if (license == null)
            {
                return NotFound();
            }

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            return View(license);
        }

        // GET: License/Create
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public IActionResult Create()
        {
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.TenantId = User.FindFirst("TenantId")?.Value;

            return View();
        }

        // POST: License/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public IActionResult Create(LicenseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Create license logic here
                var license = CreateLicense(model, userRole, tenantId, userId);

                TempData["Success"] = "License created successfully!";
                return RedirectToAction(nameof(Details), new { id = license.Id });
            }

            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.TenantId = User.FindFirst("TenantId")?.Value;

            return View(model);
        }

        // GET: License/Edit/5
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public IActionResult Edit(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var license = GetLicenseById(id, userRole, tenantId);
            
            if (license == null)
            {
                return NotFound();
            }

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var editModel = new LicenseEditViewModel
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                LicenseType = license.LicenseType,
                ApplicantName = license.ApplicantName,
                ApplicantEmail = license.ApplicantEmail,
                BusinessName = license.BusinessName,
                Description = license.Description,
                Status = license.Status,
                ExpiryDate = license.ExpiryDate,
                IssuedDate = license.IssuedDate
            };

            return View(editModel);
        }

        // POST: License/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public IActionResult Edit(int id, LicenseEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;

                // Update license logic here
                var success = UpdateLicense(id, model, userRole, tenantId);

                if (success)
                {
                    TempData["Success"] = "License updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update license. Please try again.");
                }
            }

            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.TenantId = User.FindFirst("TenantId")?.Value;

            return View(model);
        }

        // POST: License/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public IActionResult Delete(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var license = GetLicenseById(id, userRole, tenantId);
            
            if (license == null)
            {
                return NotFound();
            }

            // Delete license logic here
            var success = DeleteLicense(id, userRole);

            if (success)
            {
                TempData["Success"] = "License deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete license. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private dynamic GetLicensesForUser(string role, string tenantId)
        {
            // Mock data - in real app, this would come from database/API
            switch (role)
            {
                case "Administrator":
                    return new object[]
                    {
                        new { Id = 1, LicenseNumber = "LIC-2024-001", LicenseType = "Business License", ApplicantName = "John Doe", ApplicantEmail = "john.doe@email.com", BusinessName = "Doe Enterprises", Status = "Active", ExpiryDate = DateTime.Parse("2025-12-31"), IssuedDate = DateTime.Parse("2024-01-15"), AgencyName = "Department of Transportation", Description = "General business license for commercial operations" },
                        new { Id = 2, LicenseNumber = "LIC-2024-002", LicenseType = "Health Permit", ApplicantName = "Jane Smith", ApplicantEmail = "jane.smith@email.com", BusinessName = "Smith Healthcare", Status = "Pending", ExpiryDate = (DateTime?)null, IssuedDate = (DateTime?)null, AgencyName = "Health Services Agency", Description = "Health department permit for medical facility operations" },
                        new { Id = 3, LicenseNumber = "LIC-2024-003", LicenseType = "Food Service License", ApplicantName = "Bob Johnson", ApplicantEmail = "bob.johnson@email.com", BusinessName = "Johnson's Restaurant", Status = "Expired", ExpiryDate = DateTime.Parse("2024-06-30"), IssuedDate = DateTime.Parse("2022-07-01"), AgencyName = "Business Licensing Board", Description = "Food service license for restaurant operations" }
                    };

                case "AgencyStaff":
                    return new object[]
                    {
                        new { Id = 1, LicenseNumber = "LIC-2024-001", LicenseType = "Business License", ApplicantName = "John Doe", ApplicantEmail = "john.doe@email.com", BusinessName = "Doe Enterprises", Status = "Active", ExpiryDate = DateTime.Parse("2025-12-31"), IssuedDate = DateTime.Parse("2024-01-15"), AgencyName = GetAgencyName(tenantId), Description = "General business license for commercial operations" },
                        new { Id = 4, LicenseNumber = "LIC-2024-004", LicenseType = "Professional License", ApplicantName = "Alice Brown", ApplicantEmail = "alice.brown@email.com", BusinessName = "Brown Consulting", Status = "Under Review", ExpiryDate = (DateTime?)null, IssuedDate = (DateTime?)null, AgencyName = GetAgencyName(tenantId), Description = "Professional consulting license for business services" }
                    };

                case "Applicant":
                    return new object[]
                    {
                        new { Id = 1, LicenseNumber = "LIC-2024-001", LicenseType = "Business License", ApplicantName = "John Doe", ApplicantEmail = "john.doe@email.com", BusinessName = "Doe Enterprises", Status = "Active", ExpiryDate = DateTime.Parse("2025-12-31"), IssuedDate = DateTime.Parse("2024-01-15"), AgencyName = "Department of Transportation", Description = "General business license for commercial operations" },
                        new { Id = 5, LicenseNumber = "APP-2024-002", LicenseType = "Food Service License", ApplicantName = "John Doe", ApplicantEmail = "john.doe@email.com", BusinessName = "Doe's Cafe", Status = "In Progress", ExpiryDate = (DateTime?)null, IssuedDate = (DateTime?)null, AgencyName = "Business Licensing Board", Description = "Food service license for cafe operations" }
                    };

                default:
                    return new object[0];
            }
        }

        private dynamic GetLicenseById(int id, string role, string tenantId)
        {
            var licenses = GetLicensesForUser(role, tenantId) as object[];
            return licenses?.FirstOrDefault(l => l.GetType().GetProperty("Id")?.GetValue(l)?.ToString() == id.ToString());
        }

        private dynamic CreateLicense(LicenseCreateViewModel model, string role, string tenantId, string userId)
        {
            // Mock creation - in real app, this would call API/database
            return new
            {
                Id = new Random().Next(100, 999),
                LicenseNumber = $"LIC-{DateTime.Now.Year}-{new Random().Next(1000, 9999):D4}",
                LicenseType = model.LicenseType,
                ApplicantName = model.ApplicantName,
                ApplicantEmail = model.ApplicantEmail,
                BusinessName = model.BusinessName,
                Description = model.Description,
                Status = "Pending",
                ExpiryDate = model.ExpiryDate,
                IssuedDate = DateTime.Now,
                AgencyName = GetAgencyName(tenantId)
            };
        }

        private bool UpdateLicense(int id, LicenseEditViewModel model, string role, string tenantId)
        {
            // Mock update - in real app, this would call API/database
            return true;
        }

        private bool DeleteLicense(int id, string role)
        {
            // Mock delete - in real app, this would call API/database
            return role == "Administrator";
        }

        private string GetAgencyName(string tenantId)
        {
            return tenantId switch
            {
                "AGENCY001" => "Department of Transportation",
                "AGENCY002" => "Health Services Agency",
                "AGENCY003" => "Business Licensing Board",
                _ => "Your Agency"
            };
        }

        #endregion
    }

    #region View Models

    public class LicenseCreateViewModel
    {
        public string LicenseType { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
    }

    public class LicenseEditViewModel
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public DateTime? IssuedDate { get; set; }
    }

    #endregion
}
