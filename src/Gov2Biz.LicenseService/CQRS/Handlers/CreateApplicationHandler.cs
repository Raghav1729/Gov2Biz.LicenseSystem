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
                SubmittedDate = DateTime.UtcNow,
                Fee = request.Fee,
                Notes = request.Notes,
                PaymentCompleted = false
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
}
