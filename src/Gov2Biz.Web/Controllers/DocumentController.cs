using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        // GET: Document
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            ViewBag.UserId = userId;

            var documents = GetDocumentsForUser(userRole, tenantId);
            return View(documents);
        }

        // GET: Document/Details/5
        public IActionResult Details(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            var document = GetDocumentById(id, userRole, tenantId);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Document/Upload
        public IActionResult Upload()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;

            return View();
        }

        // POST: Document/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile file, string documentType, string description, int? licenseId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (file != null && file.Length > 0)
            {
                var document = UploadDocument(file, documentType, description, licenseId, userRole, tenantId, userId);
                if (document != null)
                {
                    TempData["Success"] = "Document uploaded successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Error"] = "Failed to upload document. Please try again.";
            ViewBag.UserRole = userRole;
            ViewBag.TenantId = tenantId;
            return View();
        }

        // GET: Document/Download/5
        public IActionResult Download(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var document = GetDocumentById(id, userRole, tenantId);
            if (document == null)
            {
                return NotFound();
            }

            // Mock file download - in real app, this would return actual file
            TempData["Info"] = "Document download started (mock implementation)";
            return RedirectToAction(nameof(Index));
        }

        // POST: Document/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;

            var success = DeleteDocument(id, userRole, tenantId);
            if (success)
            {
                TempData["Success"] = "Document deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete document. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private dynamic GetDocumentsForUser(string role, string tenantId)
        {
            // Mock data - in real app, this would come from database/API
            switch (role)
            {
                case "Administrator":
                    return new object[]
                    {
                        new { Id = 1, FileName = "Business_Registration.pdf", DocumentType = "Business Registration", FileSize = "2.3 MB", UploadDate = DateTime.Parse("2024-01-15"), Status = "Approved", LicenseId = 1, LicenseNumber = "LIC-2024-001", UploadedBy = "John Doe" },
                        new { Id = 2, FileName = "Health_Certificate.pdf", DocumentType = "Health Certificate", FileSize = "1.5 MB", UploadDate = DateTime.Parse("2024-01-20"), Status = "Pending", LicenseId = 2, LicenseNumber = "LIC-2024-002", UploadedBy = "Jane Smith" },
                        new { Id = 3, FileName = "Tax_Clearance.pdf", DocumentType = "Tax Clearance", FileSize = "800 KB", UploadDate = DateTime.Parse("2024-01-25"), Status = "Approved", LicenseId = 1, LicenseNumber = "LIC-2024-001", UploadedBy = "John Doe" }
                    };

                case "AgencyStaff":
                    return new object[]
                    {
                        new { Id = 1, FileName = "Business_Registration.pdf", DocumentType = "Business Registration", FileSize = "2.3 MB", UploadDate = DateTime.Parse("2024-01-15"), Status = "Approved", LicenseId = 1, LicenseNumber = "LIC-2024-001", UploadedBy = "John Doe" },
                        new { Id = 4, FileName = "Professional_Cert.pdf", DocumentType = "Professional Certificate", FileSize = "1.2 MB", UploadDate = DateTime.Parse("2024-02-01"), Status = "Under Review", LicenseId = 4, LicenseNumber = "LIC-2024-004", UploadedBy = "Alice Brown" }
                    };

                case "Applicant":
                    return new object[]
                    {
                        new { Id = 1, FileName = "Business_Registration.pdf", DocumentType = "Business Registration", FileSize = "2.3 MB", UploadDate = DateTime.Parse("2024-01-15"), Status = "Approved", LicenseId = 1, LicenseNumber = "LIC-2024-001", UploadedBy = "John Doe" },
                        new { Id = 5, FileName = "Food_Safety_Course.pdf", DocumentType = "Food Safety Certificate", FileSize = "3.1 MB", UploadDate = DateTime.Parse("2024-02-05"), Status = "Pending", LicenseId = 5, LicenseNumber = "APP-2024-002", UploadedBy = "John Doe" }
                    };

                default:
                    return new object[0];
            }
        }

        private dynamic GetDocumentById(int id, string role, string tenantId)
        {
            var documents = GetDocumentsForUser(role, tenantId) as object[];
            return documents?.FirstOrDefault(d => d.GetType().GetProperty("Id")?.GetValue(d)?.ToString() == id.ToString());
        }

        private dynamic UploadDocument(IFormFile file, string documentType, string description, int? licenseId, string role, string tenantId, string userId)
        {
            // Mock upload - in real app, this would save file to storage and create database record
            return new
            {
                Id = new Random().Next(100, 999),
                FileName = file.FileName,
                DocumentType = documentType,
                FileSize = $"{file.Length / 1024.0:F1} KB",
                UploadDate = DateTime.Now,
                Status = "Pending",
                LicenseId = licenseId,
                UploadedBy = userId,
                Description = description
            };
        }

        private bool DeleteDocument(int id, string role, string tenantId)
        {
            // Mock deletion - in real app, this would delete from database and storage
            return true;
        }

        #endregion
    }
}
