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
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (application == null)
                throw new KeyNotFoundException($"Application with ID {request.Id} not found");

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

    public class GetLicenseApplicationsHandler : IRequestHandler<GetLicenseApplicationsQuery, Gov2Biz.Shared.Responses.PagedResult<LicenseApplicationDto>>
    {
        private readonly LicenseDbContext _context;

        public GetLicenseApplicationsHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.Responses.PagedResult<LicenseApplicationDto>> Handle(GetLicenseApplicationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LicenseApplications.AsQueryable();

            if (!string.IsNullOrEmpty(request.AgencyId))
                query = query.Where(a => a.AgencyId == request.AgencyId);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(a => a.Status == request.Status);

            if (request.ApplicantId.HasValue)
                query = query.Where(a => a.ApplicantId == request.ApplicantId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var applications = await query
                .OrderByDescending(a => a.SubmittedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseApplicationDto>();
            foreach (var application in applications)
            {
                dtos.Add(await MapToDto(application));
            }

            return new Gov2Biz.Shared.Responses.PagedResult<LicenseApplicationDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
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
