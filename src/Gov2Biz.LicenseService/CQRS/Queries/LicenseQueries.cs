using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.LicenseService.CQRS.Queries
{
    public record GetLicenseApplicationQuery(int Id) : IRequest<LicenseApplicationDto>;
    
    public record GetLicenseApplicationsQuery(
        string? AgencyId = null,
        string? Status = null,
        int? ApplicantId = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Gov2Biz.Shared.Responses.PagedResult<LicenseApplicationDto>>;

    public record GetLicenseQuery(int Id) : IRequest<LicenseDto>;
    
    public record GetLicensesQuery(
        string? AgencyId = null,
        string? Status = null,
        int? ApplicantId = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Gov2Biz.Shared.Responses.PagedResult<LicenseDto>>;

    public record GetUserLicensesQuery(int UserId) : IRequest<List<LicenseDto>>;

    public record GetUserApplicationsQuery(int UserId) : IRequest<List<LicenseApplicationDto>>;

    public record GetDashboardStatsQuery(string? AgencyId = null) : IRequest<Gov2Biz.Shared.Responses.DashboardStatsDto>;
}
