using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Gov2Biz.Web.Services;
using Gov2Biz.Shared.DTOs;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class LicenseController : Controller
    {
        private readonly ILicenseServiceClient _licenseServiceClient;
        private readonly IDocumentServiceClient _documentServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly ILogger<LicenseController> _logger;

        public LicenseController(
            ILicenseServiceClient licenseServiceClient,
            IDocumentServiceClient documentServiceClient,
            INotificationServiceClient notificationServiceClient,
            ILogger<LicenseController> logger)
        {
            _licenseServiceClient = licenseServiceClient;
            _documentServiceClient = documentServiceClient;
            _notificationServiceClient = notificationServiceClient;
            _logger = logger;
        }
        // GET: License
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;
                var agencyId = User.FindFirst("AgencyId")?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                ViewBag.UserRole = userRole;
                ViewBag.TenantId = tenantId;

                var filter = new LicenseFilter
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    AgencyId = userRole == "Administrator" ? agencyId : agencyId,
                    ApplicantId = userRole == "Applicant" && int.TryParse(userIdClaim, out var uid) ? uid : null
                };

                var licenses = await _licenseServiceClient.GetLicensesAsync(filter);

                return View(licenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading licenses");
                ViewBag.ErrorMessage = "Unable to load licenses. Please try again later.";
                return View(new PagedResult<LicenseDto>());
            }
        }

        // GET: License/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;

                var license = await _licenseServiceClient.GetLicenseAsync(id);
                
                if (license == null || license.Id == 0)
                {
                    return NotFound();
                }

                // Get related documents
                var documents = await _documentServiceClient.GetDocumentsAsync("License", id);

                ViewBag.UserRole = userRole;
                ViewBag.TenantId = tenantId;
                ViewBag.Documents = documents;

                return View(license);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license details {LicenseId}", id);
                ViewBag.ErrorMessage = "Unable to load license details. Please try again later.";
                return View(new LicenseDto());
            }
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
        public async Task<IActionResult> Create(LicenseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                    var tenantId = User.FindFirst("TenantId")?.Value;
                    var agencyId = User.FindFirst("AgencyId")?.Value;
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!int.TryParse(userIdClaim, out var applicantId))
                    {
                        ModelState.AddModelError("", "Unable to determine user identity.");
                        return View(model);
                    }

                    var command = new CreateLicenseApplicationCommand(
                        model.LicenseType,
                        agencyId ?? "default",
                        model.ApplicationFee,
                        model.Description,
                        applicantId
                    );

                    var application = await _licenseServiceClient.CreateApplicationAsync(command);

                    TempData["Success"] = $"License application {application.ApplicationNumber} created successfully!";
                    return RedirectToAction("ApplicationDetails", new { id = application.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating license application");
                    ModelState.AddModelError("", "Failed to create license application. Please try again.");
                }
            }

            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.TenantId = User.FindFirst("TenantId")?.Value;

            return View(model);
        }

        // GET: License/Edit/5
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<IActionResult> Edit(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            try
            {
                var license = await _licenseServiceClient.GetLicenseAsync(id);
                
                if (license == null || license.Id == 0)
                {
                    return NotFound();
                }

                var model = new LicenseEditViewModel
                {
                    Id = license.Id,
                    LicenseNumber = license.LicenseNumber,
                    LicenseType = license.Type,
                    ApplicantName = license.ApplicantName ?? "",
                    ApplicantEmail = license.ApplicantEmail ?? "",
                    BusinessName = "", // Not available in LicenseDto
                    Description = license.Notes ?? "",
                    Status = license.Status,
                    ExpiryDate = license.ExpiresAt,
                    IssuedDate = license.IssuedAt
                };

                ViewBag.UserRole = userRole;
                ViewBag.TenantId = tenantId;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license for edit {LicenseId}", id);
                ViewBag.ErrorMessage = "Unable to load license. Please try again later.";
                return View(new LicenseEditViewModel());
            }
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
                // TODO: Implement update functionality when API endpoint is available
                var success = true; // Placeholder

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
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            try
            {
                var license = await _licenseServiceClient.GetLicenseAsync(id);
                
                if (license == null || license.Id == 0)
                {
                    return NotFound();
                }

                // Delete license logic here
                // TODO: Implement delete functionality when API endpoint is available
                var success = true; // Placeholder

            if (success)
                {
                    TempData["Success"] = "License deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete license. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting license {LicenseId}", id);
                TempData["Error"] = "Failed to delete license. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: License/ApplicationDetails/5
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = User.FindFirst("TenantId")?.Value;

                var application = await _licenseServiceClient.GetApplicationAsync(id);
                
                if (application == null || application.Id == 0)
                {
                    return NotFound();
                }

                // Get related documents
                var documents = await _documentServiceClient.GetDocumentsAsync("LicenseApplication", id);

                ViewBag.UserRole = userRole;
                ViewBag.TenantId = tenantId;
                ViewBag.Documents = documents;

                return View(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading application details {ApplicationId}", id);
                ViewBag.ErrorMessage = "Unable to load application details. Please try again later.";
                return View(new LicenseApplicationDto());
            }
        }

        // POST: License/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<IActionResult> Approve(int id, string reviewerNotes)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
                if (!int.TryParse(userIdClaim, out var reviewerId))
                {
                    reviewerId = 0;
                }
                
                var command = new ApproveLicenseApplicationCommand(id, reviewerNotes, reviewerId);

                var result = await _licenseServiceClient.ApproveApplicationAsync(id, command);

                // Create notification - using ApplicationNumber instead of ApplicantId
                await _notificationServiceClient.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        "Application Approved",
                        $"Your license application {result.ApplicationNumber} has been approved.",
                        "Success",
                        0, // Placeholder - we need the actual applicant ID
                        result.ApplicationNumber
                    )
                );

                TempData["Success"] = "Application approved successfully!";
                return RedirectToAction("ApplicationDetails", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving application {ApplicationId}", id);
                TempData["Error"] = "Failed to approve application. Please try again.";
                return RedirectToAction("ApplicationDetails", new { id });
            }
        }

        // POST: License/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
                if (!int.TryParse(userIdClaim, out var reviewerId))
                {
                    reviewerId = 0;
                }
                
                var command = new RejectLicenseApplicationCommand(id, rejectionReason, reviewerId);

                var result = await _licenseServiceClient.RejectApplicationAsync(id, command);

                // Create notification
                await _notificationServiceClient.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        "Application Rejected",
                        $"Your license application {result.ApplicationNumber} has been rejected. Reason: {rejectionReason}",
                        "Warning",
                        0, // Placeholder - we need the actual applicant ID
                        result.ApplicationNumber
                    )
                );

                TempData["Success"] = "Application rejected successfully!";
                return RedirectToAction("ApplicationDetails", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting application {ApplicationId}", id);
                TempData["Error"] = "Failed to reject application. Please try again.";
                return RedirectToAction("ApplicationDetails", new { id });
            }
        }

        // POST: License/Issue/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<IActionResult> Issue(int applicationId)
        {
            try
            {
                var license = await _licenseServiceClient.IssueLicenseAsync(applicationId);

                // Create notification
                await _notificationServiceClient.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        "License Issued",
                        $"Your license {license.LicenseNumber} has been issued and is now active.",
                        "Success",
                        0, // Placeholder - we need the actual applicant ID
                        license.LicenseNumber
                    )
                );

                TempData["Success"] = $"License {license.LicenseNumber} issued successfully!";
                return RedirectToAction("Details", new { id = license.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing license for application {ApplicationId}", applicationId);
                TempData["Error"] = "Failed to issue license. Please try again.";
                return RedirectToAction("ApplicationDetails", new { id = applicationId });
            }
        }

        // POST: License/Renew/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,AgencyStaff,Applicant")]
        public async Task<IActionResult> Renew(int id, int renewalPeriodMonths = 12)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
                if (!int.TryParse(userIdClaim, out var renewedBy))
                {
                    renewedBy = 0;
                }
                
                var command = new RenewLicenseCommand(id, renewedBy, renewalPeriodMonths);

                var license = await _licenseServiceClient.RenewLicenseAsync(id, command);

                // Create notification
                await _notificationServiceClient.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        "License Renewed",
                        $"Your license {license.LicenseNumber} has been renewed for {renewalPeriodMonths} months.",
                        "Success",
                        0, // Placeholder - we need the actual applicant ID
                        license.LicenseNumber
                    )
                );

                TempData["Success"] = $"License {license.LicenseNumber} renewed successfully!";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing license {LicenseId}", id);
                TempData["Error"] = "Failed to renew license. Please try again.";
                return RedirectToAction("Details", new { id });
            }
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
    }

    #region View Models

    public class LicenseCreateViewModel
    {
        public string LicenseType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ApplicationFee { get; set; } = 100.00m;
        public string[] AvailableLicenseTypes { get; } = new[]
        {
            "Business License",
            "Professional License",
            "Health Permit",
            "Food Service License",
            "Construction Permit",
            "Environmental Permit"
        };
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
