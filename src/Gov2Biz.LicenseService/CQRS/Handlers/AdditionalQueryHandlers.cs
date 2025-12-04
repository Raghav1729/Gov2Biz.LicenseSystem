using Gov2Biz.LicenseService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.LicenseService.CQRS.Handlers
{
    public class GetLicenseHandler : IRequestHandler<GetLicenseQuery, LicenseDto>
    {
        private readonly LicenseDbContext _context;

        public GetLicenseHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseDto> Handle(GetLicenseQuery request, CancellationToken cancellationToken)
        {
            var license = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (license == null)
                throw new KeyNotFoundException($"License with ID {request.Id} not found");

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

    public class GetLicensesHandler : IRequestHandler<GetLicensesQuery, Gov2Biz.Shared.Responses.PagedResult<LicenseDto>>
    {
        private readonly LicenseDbContext _context;

        public GetLicensesHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.Responses.PagedResult<LicenseDto>> Handle(GetLicensesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Licenses.AsQueryable();

            if (!string.IsNullOrEmpty(request.AgencyId))
                query = query.Where(l => l.AgencyId == request.AgencyId);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(l => l.Status == request.Status);

            if (request.ApplicantId.HasValue)
                query = query.Where(l => l.ApplicantId == request.ApplicantId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var licenses = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseDto>();
            foreach (var license in licenses)
            {
                dtos.Add(await MapToDto(license));
            }

            return new Gov2Biz.Shared.Responses.PagedResult<LicenseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
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

    public class GetUserLicensesHandler : IRequestHandler<GetUserLicensesQuery, List<LicenseDto>>
    {
        private readonly LicenseDbContext _context;

        public GetUserLicensesHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<List<LicenseDto>> Handle(GetUserLicensesQuery request, CancellationToken cancellationToken)
        {
            var licenses = await _context.Licenses
                .Where(l => l.ApplicantId == request.UserId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseDto>();
            foreach (var license in licenses)
            {
                dtos.Add(await MapToDto(license));
            }

            return dtos;
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

    public class GetUserApplicationsHandler : IRequestHandler<GetUserApplicationsQuery, List<LicenseApplicationDto>>
    {
        private readonly LicenseDbContext _context;

        public GetUserApplicationsHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<List<LicenseApplicationDto>> Handle(GetUserApplicationsQuery request, CancellationToken cancellationToken)
        {
            var applications = await _context.LicenseApplications
                .Where(a => a.ApplicantId == request.UserId)
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseApplicationDto>();
            foreach (var application in applications)
            {
                dtos.Add(await MapToDto(application));
            }

            return dtos;
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

    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Gov2Biz.Shared.Responses.DashboardStatsDto>
    {
        private readonly LicenseDbContext _context;

        public GetDashboardStatsHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.Responses.DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var applicationQuery = _context.LicenseApplications.AsQueryable();
            var licenseQuery = _context.Licenses.AsQueryable();

            if (!string.IsNullOrEmpty(request.AgencyId))
            {
                applicationQuery = applicationQuery.Where(a => a.AgencyId == request.AgencyId);
                licenseQuery = licenseQuery.Where(l => l.AgencyId == request.AgencyId);
            }

            var totalApplications = await applicationQuery.CountAsync(cancellationToken);
            var pendingApplications = await applicationQuery.CountAsync(a => a.Status == "Submitted", cancellationToken);
            var approvedApplications = await applicationQuery.CountAsync(a => a.Status == "Approved", cancellationToken);
            var rejectedApplications = await applicationQuery.CountAsync(a => a.Status == "Rejected", cancellationToken);

            var activeLicenses = await licenseQuery.CountAsync(l => l.Status == "Active", cancellationToken);
            var expiredLicenses = await licenseQuery.CountAsync(l => l.Status == "Expired" || (l.ExpiryDate.HasValue && l.ExpiryDate < DateTime.UtcNow), cancellationToken);
            
            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            var licensesExpiringNext30Days = await licenseQuery
                .CountAsync(l => l.Status == "Active" && l.ExpiryDate.HasValue && l.ExpiryDate <= thirtyDaysFromNow, cancellationToken);

            var totalRevenue = await applicationQuery
                .Where(a => a.PaymentCompleted)
                .SumAsync(a => a.Fee, cancellationToken);

            return new Gov2Biz.Shared.Responses.DashboardStatsDto
            {
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                ApprovedApplications = approvedApplications,
                RejectedApplications = rejectedApplications,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = expiredLicenses,
                LicensesExpiringNext30Days = licensesExpiringNext30Days,
                TotalRevenue = totalRevenue
            };
        }
    }
}
