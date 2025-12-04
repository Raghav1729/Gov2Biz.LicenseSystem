using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.LicenseService.CQRS.Commands
{
    public record CreateLicenseApplicationCommand(
        string LicenseType,
        string AgencyId,
        int ApplicantId,
        decimal Fee,
        string? Notes = null
    ) : IRequest<LicenseApplicationDto>;

    public record UpdateLicenseApplicationCommand(
        int Id,
        string Status,
        string? Notes = null,
        string? ReviewerId = null
    ) : IRequest<LicenseApplicationDto>;

    public record ApproveLicenseApplicationCommand(
        int Id,
        string ReviewerId,
        string? Notes = null
    ) : IRequest<LicenseDto>;

    public record RejectLicenseApplicationCommand(
        int Id,
        string ReviewerId,
        string Reason
    ) : IRequest<LicenseApplicationDto>;

    public record IssueLicenseCommand(
        int ApplicationId,
        string IssuerId
    ) : IRequest<LicenseDto>;

    public record RenewLicenseCommand(
        int LicenseId,
        DateTime NewExpiryDate
    ) : IRequest<LicenseDto>;
}
