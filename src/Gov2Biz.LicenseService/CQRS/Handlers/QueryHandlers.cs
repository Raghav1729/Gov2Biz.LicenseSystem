using Gov2Biz.LicenseService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.LicenseService.CQRS.Handlers
{
    public class GetLicenseApplicationHandler : IRequestHandler<GetLicenseApplicationQuery, LicenseApplicationDto>
    {
        private readonly LicenseDbContext _context;

        public GetLicenseApplicationHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseApplicationDto> Handle(GetLicenseApplicationQuery request, CancellationToken cancellationToken)
        {
            var application = await _context.LicenseApplications
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.ApplicationId} not found");

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
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : "",
                SubmittedAt = application.SubmittedAt,
                ReviewedAt = application.ReviewedAt,
                ApprovedAt = application.ApprovedAt,
                RejectedAt = application.RejectedAt,
                IssuedAt = application.IssuedAt,
                ReviewerNotes = application.ReviewerNotes,
                RejectionReason = application.RejectionReason,
                ApplicationFee = application.ApplicationFee,
                IsPaid = application.IsPaid,
                DocumentCount = 0, // Would need to join with documents table
                PaymentCount = 0  // Would need to join with payments table
            };
        }
    }

    public class GetLicenseApplicationsHandler : IRequestHandler<GetLicenseApplicationsQuery, PagedResult<LicenseApplicationDto>>
    {
        private readonly LicenseDbContext _context;

        public GetLicenseApplicationsHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<LicenseApplicationDto>> Handle(GetLicenseApplicationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LicenseApplications.AsQueryable();

            if (!string.IsNullOrEmpty(request.Filter.AgencyId))
                query = query.Where(a => a.AgencyId == request.Filter.AgencyId);

            if (!string.IsNullOrEmpty(request.Filter.Status))
                query = query.Where(a => a.Status == request.Filter.Status);

            if (request.Filter.ApplicantId.HasValue)
                query = query.Where(a => a.ApplicantId == request.Filter.ApplicantId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var applications = await query
                .OrderByDescending(a => a.SubmittedAt)
                .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                .Take(request.Filter.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseApplicationDto>();
            foreach (var application in applications)
            {
                dtos.Add(await MapToDto(application));
            }

            return new PagedResult<LicenseApplicationDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.Filter.PageNumber,
                PageSize = request.Filter.PageSize
            };
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
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : "",
                SubmittedAt = application.SubmittedAt,
                ReviewedAt = application.ReviewedAt,
                ApprovedAt = application.ApprovedAt,
                RejectedAt = application.RejectedAt,
                IssuedAt = application.IssuedAt,
                ReviewerNotes = application.ReviewerNotes,
                RejectionReason = application.RejectionReason,
                ApplicationFee = application.ApplicationFee,
                IsPaid = application.IsPaid,
                DocumentCount = 0, // Would need to join with documents table
                PaymentCount = 0  // Would need to join with payments table
            };
        }
    }
}
