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
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.Id} not found");

            application.Status = "Approved";
            application.ApprovedDate = DateTime.UtcNow;
            application.ReviewerId = request.ReviewerId;
            application.Notes = request.Notes;

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
                AgencyName = agency?.Name ?? "",
                IssuedDate = DateTime.UtcNow,
                ExpiryDate = null,
                CreatedAt = application.ApprovedDate ?? DateTime.UtcNow,
                Notes = application.Notes
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
            var application = await _context.LicenseApplications.FindAsync(request.Id);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.Id} not found");

            application.Status = "Rejected";
            application.ReviewerId = request.ReviewerId;
            application.Notes = request.Reason;

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
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                AgencyName = agency?.Name ?? "",
                SubmittedDate = application.SubmittedDate,
                ReviewedDate = application.ReviewedDate,
                ApprovedDate = application.ApprovedDate,
                ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : null,
                Notes = application.Notes,
                Fee = application.Fee,
                PaymentCompleted = application.PaymentCompleted
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
                ApplicantId = application.ApplicantId,
                AgencyId = application.AgencyId,
                IssuedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
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

            return new LicenseDto
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                Type = license.Type,
                Status = license.Status,
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                AgencyName = agency?.Name ?? "",
                IssuedDate = license.IssuedDate,
                ExpiryDate = license.ExpiryDate,
                CreatedAt = license.CreatedAt,
                Notes = license.Notes
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

            license.ExpiryDate = request.NewExpiryDate;
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

            return new LicenseDto
            {
                Id = license.Id,
                LicenseNumber = license.LicenseNumber,
                Type = license.Type,
                Status = license.Status,
                ApplicantName = $"{applicant?.FirstName} {applicant?.LastName}",
                AgencyName = agency?.Name ?? "",
                IssuedDate = license.IssuedDate,
                ExpiryDate = license.ExpiryDate,
                CreatedAt = license.CreatedAt,
                Notes = license.Notes
            };
        }
    }
}
