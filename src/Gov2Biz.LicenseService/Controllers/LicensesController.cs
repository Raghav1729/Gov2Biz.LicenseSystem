using Microsoft.AspNetCore.Mvc;
using MediatR;
using Gov2Biz.LicenseService.CQRS.Commands;
using Gov2Biz.LicenseService.CQRS.Queries;
using Gov2Biz.Shared.Responses;

namespace Gov2Biz.LicenseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LicensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("applications")]
        public async Task<ApiResponse<LicenseApplicationDto>> CreateApplication([FromBody] CreateLicenseApplicationCommand command)
        {
            try
            {
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
        public async Task<ApiResponse<LicenseDto>> ApproveApplication(int id, [FromBody] ApproveLicenseApplicationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command with { Id = id });
                return new ApiResponse<LicenseDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LicenseDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("applications/{id}/reject")]
        public async Task<ApiResponse<LicenseApplicationDto>> RejectApplication(int id, [FromBody] RejectLicenseApplicationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command with { Id = id });
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

        [HttpGet("dashboard/stats")]
        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStats([FromQuery] string? agencyId = null)
        {
            try
            {
                var result = await _mediator.Send(new GetDashboardStatsQuery(agencyId));
                return new ApiResponse<DashboardStatsDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DashboardStatsDto> { Success = false, Message = ex.Message };
            }
        }
    }
}
