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
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

            if (license == null)
                throw new KeyNotFoundException($"License with ID {request.LicenseId} not found");

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
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                IssuedAt = license.IssuedAt,
                ExpiresAt = license.ExpiresAt,
                Notes = license.Notes,
                DaysUntilExpiry = license.ExpiresAt.HasValue ? (license.ExpiresAt.Value - DateTime.UtcNow).Days : 0
            };
        }
    }

    public class GetLicensesHandler : IRequestHandler<GetLicensesQuery, PagedResult<LicenseDto>>
    {
        private readonly LicenseDbContext _context;

        public GetLicensesHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<LicenseDto>> Handle(GetLicensesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Licenses.AsQueryable();

            if (!string.IsNullOrEmpty(request.Filter.AgencyId))
                query = query.Where(l => l.AgencyId == request.Filter.AgencyId);

            if (!string.IsNullOrEmpty(request.Filter.Status))
                query = query.Where(l => l.Status == request.Filter.Status);

            if (request.Filter.ApplicantId.HasValue)
                query = query.Where(l => l.ApplicantId == request.Filter.ApplicantId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var licenses = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                .Take(request.Filter.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<LicenseDto>();
            foreach (var license in licenses)
            {
                dtos.Add(await MapToDto(license));
            }

            return new PagedResult<LicenseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.Filter.PageNumber,
                PageSize = request.Filter.PageSize
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
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                IssuedAt = license.IssuedAt,
                ExpiresAt = license.ExpiresAt,
                Notes = license.Notes,
                DaysUntilExpiry = license.ExpiresAt.HasValue ? (license.ExpiresAt.Value - DateTime.UtcNow).Days : 0
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
                ApplicantEmail = applicant?.Email ?? "",
                AgencyName = agency?.Name ?? "",
                IssuedAt = license.IssuedAt,
                ExpiresAt = license.ExpiresAt,
                Notes = license.Notes,
                DaysUntilExpiry = license.ExpiresAt.HasValue ? (license.ExpiresAt.Value - DateTime.UtcNow).Days : 0
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
                .OrderByDescending(a => a.SubmittedAt)
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

    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly LicenseDbContext _context;

        public GetDashboardStatsHandler(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
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
            var expiredLicenses = await licenseQuery.CountAsync(l => l.Status == "Expired" || (l.ExpiresAt.HasValue && l.ExpiresAt < DateTime.UtcNow), cancellationToken);
            
            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            var licensesExpiringNext30Days = await licenseQuery
                .CountAsync(l => l.Status == "Active" && l.ExpiresAt.HasValue && l.ExpiresAt <= thirtyDaysFromNow, cancellationToken);

            var totalRevenue = await applicationQuery
                .Where(a => a.IsPaid)
                .SumAsync(a => a.ApplicationFee, cancellationToken);

            return new DashboardStatsDto
            {
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                ApprovedApplications = approvedApplications,
                RejectedApplications = rejectedApplications,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = expiredLicenses,
                ExpiringSoonLicenses = licensesExpiringNext30Days,
                TotalRevenue = totalRevenue,
                UnreadNotifications = 0, // Would need to join with notifications table
                PendingPayments = 0, // Would need to join with payments table
                RecentActivities = new List<RecentActivityDto>()
            };
        }
    }
}
