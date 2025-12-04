using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Gov2Biz.LicenseService.CQRS.Queries;
using Gov2Biz.Shared.DTOs;
using System.Security.Claims;

namespace Gov2Biz.LicenseService.Controllers
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LicensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LicensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("applications")]
        [Authorize(Roles = "Administrator,AgencyStaff,Applicant")]
        public async Task<ApiResponse<LicenseApplicationDto>> CreateApplication([FromBody] CreateLicenseApplicationCommand command)
        {
            try
            {
                // Set tenant and applicant from JWT claims
                var tenantId = User.FindFirst("TenantId")?.Value ?? "default";
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (int.TryParse(userId, out var applicantId))
                {
                    command = command with { ApplicantId = applicantId };
                }
                
                var result = await _mediator.Send(command);
                return new ApiResponse<LicenseApplicationDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseApplicationDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("applications/{id}")]
        public async Task<ApiResponse<LicenseApplicationDto>> GetApplication(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetLicenseApplicationQuery(id));
                return new ApiResponse<LicenseApplicationDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseApplicationDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("applications")]
        public async Task<ApiResponse<PagedResult<LicenseApplicationDto>>> GetApplications(
            [FromQuery] string? agencyId = null,
            [FromQuery] string? status = null,
            [FromQuery] int? applicantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetLicenseApplicationsQuery(agencyId, status, applicantId, pageNumber, pageSize));
                return new ApiResponse<PagedResult<LicenseApplicationDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<LicenseApplicationDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("applications/{id}/approve")]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<ApiResponse<LicenseDto>> ApproveApplication(int id, [FromBody] ApproveLicenseApplicationCommand command)
        {
            try
            {
                // Set reviewer ID from JWT claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                command = command with { Id = id, ReviewerId = userId ?? "0" };
                var result = await _mediator.Send(command);
                return new ApiResponse<LicenseDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("applications/{id}/reject")]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<ApiResponse<LicenseApplicationDto>> RejectApplication(int id, [FromBody] RejectLicenseApplicationCommand command)
        {
            try
            {
                // Set reviewer ID from JWT claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                command = command with { Id = id, ReviewerId = userId ?? "0" };
                var result = await _mediator.Send(command);
                return new ApiResponse<LicenseApplicationDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseApplicationDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<LicenseDto>> GetLicense(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetLicenseQuery(id));
                return new ApiResponse<LicenseDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<LicenseDto>>> GetLicenses(
            [FromQuery] string? agencyId = null,
            [FromQuery] string? status = null,
            [FromQuery] int? applicantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetLicensesQuery(agencyId, status, applicantId, pageNumber, pageSize));
                return new ApiResponse<PagedResult<LicenseDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<LicenseDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpPost("{applicationId}/issue")]
        [Authorize(Roles = "Administrator,AgencyStaff")]
        public async Task<ApiResponse<LicenseDto>> IssueLicense(int applicationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
                var result = await _mediator.Send(new IssueLicenseCommand(applicationId, userId));
                return new ApiResponse<LicenseDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("{id}/renew")]
        [Authorize(Roles = "Administrator,AgencyStaff,Applicant")]
        public async Task<ApiResponse<LicenseDto>> RenewLicense(int id, [FromBody] RenewLicenseCommand command)
        {
            try
            {
                var result = await _mediator.Send(command with { LicenseId = id });
                return new ApiResponse<LicenseDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseDto> { Success = false, Message = ex.Message };
            }
        }
    }
}
