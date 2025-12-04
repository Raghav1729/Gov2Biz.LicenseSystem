using Gov2Biz.LicenseService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.LicenseService.CQRS.Handlers
{
    public class ApproveLicenseApplicationHandler : IRequestHandler<ApproveLicenseApplicationCommand, LicenseDto>
    {
        private readonly LicenseDbContext _context;

        public ApproveLicenseApplicationHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseDto> Handle(ApproveLicenseApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.LicenseApplications
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.ApplicationId} not found");

            application.Status = "Approved";
            application.ApprovedAt = DateTime.UtcNow;
            application.ReviewerId = request.ReviewerId;
            application.ReviewerNotes = request.ReviewerNotes;

            _context.LicenseApplications.Update(application);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(application);
        }

        private async Task<LicenseDto> MapToDto(LicenseApplication application)
        {
            var applicant = await _context.Users.FindAsync(application.ApplicantId);
            var agency = await _context.Agencies.FindAsync(application.AgencyId);

            return new LicenseDto
            {
                Id = 0, // License not yet issued
                LicenseNumber = "PENDING",
                Type = application.LicenseType,
                Status = "Approved",
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = null,
                Notes = application.ReviewerNotes,
                DaysUntilExpiry = 0
            };
        }
    }

    public class RejectLicenseApplicationHandler : IRequestHandler<RejectLicenseApplicationCommand, LicenseApplicationDto>
    {
        private readonly LicenseDbContext _context;

        public RejectLicenseApplicationHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseApplicationDto> Handle(RejectLicenseApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.LicenseApplications.FindAsync(request.ApplicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.ApplicationId} not found");

            application.Status = "Rejected";
            application.ReviewerId = request.ReviewerId;
            application.RejectionReason = request.RejectionReason;
            application.RejectedAt = DateTime.UtcNow;

            _context.LicenseApplications.Update(application);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(application);
        }

        private async Task<LicenseApplicationDto> MapToDto(LicenseApplication application)
        {
            var applicant = await _context.Users.FindAsync(application.ApplicantId);
            var agency = await _context.Agencies.FindAsync(application.AgencyId);
            var reviewer = application.ReviewerId != null ? await _context.Users.FindAsync(application.ReviewerId) : null;

            return new LicenseApplicationDto
            {
                Id = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                LicenseType = application.LicenseType,
                Status = application.Status,
                ApplicationFee = application.ApplicationFee,
                IsPaid = application.IsPaid,
                SubmittedAt = application.SubmittedAt,
                ReviewedAt = application.ReviewedAt,
                ApprovedAt = application.ApprovedAt,
                RejectedAt = application.RejectedAt,
                IssuedAt = application.IssuedAt,
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : "",
                ReviewerNotes = application.ReviewerNotes,
                RejectionReason = application.RejectionReason,
                DocumentCount = 0, // Would need to join with Documents table
                PaymentCount = 0  // Would need to join with Payments table
            };
        }
    }

    public class IssueLicenseCommandHandler : IRequestHandler<IssueLicenseCommand, LicenseDto>
    {
        private readonly LicenseDbContext _context;

        public IssueLicenseCommandHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseDto> Handle(IssueLicenseCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.LicenseApplications
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.ApplicationId} not found");

            if (application.Status != "Approved")
                throw new InvalidOperationException("Application must be approved before issuing license");

            var licenseNumber = GenerateLicenseNumber(application.AgencyId);
            var license = new License
            {
                LicenseNumber = licenseNumber,
                Type = application.LicenseType,
                Status = "Active",
                ApplicationId = application.Id,
                ApplicantId = application.ApplicantId,
                AgencyId = application.AgencyId,
                TenantId = application.TenantId,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow,
                Notes = $"Issued from application {application.ApplicationNumber}"
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(license);
        }

        private string GenerateLicenseNumber(string agencyId)
        {
            var year = DateTime.UtcNow.Year;
            var random = new Random().Next(10000, 99999);
            return $"{agencyId}-{year}-{random}";
        }

        private async Task<LicenseDto> MapToDto(License license)
        {
            var applicant = await _context.Users.FindAsync(license.ApplicantId);
            var agency = await _context.Agencies.FindAsync(license.AgencyId);
            var application = await _context.LicenseApplications.FindAsync(license.ApplicationId);

            return new LicenseDto
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                Type = license.Type,
                Status = license.Status,
                IssuedAt = license.IssuedAt,
                ExpiresAt = license.ExpiresAt,
                RenewedAt = license.RenewedAt,
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                ApplicationNumber = application?.ApplicationNumber ?? "",
                Notes = license.Notes,
                DaysUntilExpiry = license.ExpiresAt.HasValue ? 
                    (license.ExpiresAt.Value - DateTime.UtcNow).Days : 0
            };
        }
    }

    public class RenewLicenseCommandHandler : IRequestHandler<RenewLicenseCommand, LicenseDto>
    {
        private readonly LicenseDbContext _context;

        public RenewLicenseCommandHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseDto> Handle(RenewLicenseCommand request, CancellationToken cancellationToken)
        {
            var license = await _context.Licenses.FindAsync(request.LicenseId);
            if (license == null)
                throw new KeyNotFoundException($"License with ID {request.LicenseId} not found");

            if (license.Status != "Active")
                throw new InvalidOperationException("Only active licenses can be renewed");

            license.ExpiresAt = DateTime.UtcNow.AddMonths(request.RenewalPeriodMonths);
            license.RenewedAt = DateTime.UtcNow;
            license.UpdatedAt = DateTime.UtcNow;
            license.Notes = $"Renewed on {DateTime.UtcNow:yyyy-MM-dd}. {license.Notes}";

            _context.Licenses.Update(license);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(license);
        }

        private async Task<LicenseDto> MapToDto(License license)
        {
            var applicant = await _context.Users.FindAsync(license.ApplicantId);
            var agency = await _context.Agencies.FindAsync(license.AgencyId);
            var application = await _context.LicenseApplications.FindAsync(license.ApplicationId);

            return new LicenseDto
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                Type = license.Type,
                Status = license.Status,
                IssuedAt = license.IssuedAt,
                ExpiresAt = license.ExpiresAt,
                RenewedAt = license.RenewedAt,
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                ApplicationNumber = application?.ApplicationNumber ?? "",
                Notes = license.Notes,
                DaysUntilExpiry = license.ExpiresAt.HasValue ? 
                    (license.ExpiresAt.Value - DateTime.UtcNow).Days : 0
            };
        }
    }
}
