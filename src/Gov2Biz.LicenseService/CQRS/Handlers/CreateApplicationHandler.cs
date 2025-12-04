using Gov2Biz.LicenseService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.LicenseService.CQRS.Handlers
{
    public class CreateLicenseApplicationHandler : IRequestHandler<CreateLicenseApplicationCommand, LicenseApplicationDto>
    {
        private readonly LicenseDbContext _context;

        public CreateLicenseApplicationHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseApplicationDto> Handle(CreateLicenseApplicationCommand request, CancellationToken cancellationToken)
        {
            var applicationNumber = GenerateApplicationNumber();
            
            var application = new LicenseApplication
            {
                ApplicationNumber = applicationNumber,
                LicenseType = request.LicenseType,
                AgencyId = request.AgencyId,
                ApplicantId = request.ApplicantId,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow,
                ApplicationFee = request.Fee,
                ReviewerNotes = request.Notes,
                IsPaid = false,
                TenantId = "default" // Should be extracted from user context
            };

            _context.LicenseApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(application);
        }

        private string GenerateApplicationNumber()
        {
            var year = DateTime.UtcNow.Year;
            var random = new Random().Next(1000, 9999);
            return $"APP-{year}-{random}";
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
}
