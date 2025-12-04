using Microsoft.AspNetCore.Mvc;
using MediatR;
using Gov2Biz.PaymentService.CQRS.Commands;
using Gov2Biz.PaymentService.CQRS.Queries;
using Gov2Biz.Shared.Responses;
using Gov2Biz.Shared.DTOs;

namespace Gov2Biz.PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<PaymentDto>> CreatePayment([FromBody] Gov2Biz.Shared.DTOs.CreatePaymentCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return new ApiResponse<PaymentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaymentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<PaymentDto>> GetPayment(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetPaymentQuery(id));
                return new ApiResponse<PaymentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaymentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<ApiResponse<PaymentDto>> GetPaymentByTransactionId(string transactionId)
        {
            try
            {
                var result = await _mediator.Send(new GetPaymentByTransactionIdQuery(transactionId));
                return new ApiResponse<PaymentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaymentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<Gov2Biz.Shared.DTOs.PagedResult<PaymentDto>>> GetPayments(
            [FromQuery] int? applicationId = null,
            [FromQuery] int? payerId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetPaymentsQuery(applicationId, payerId, status, paymentMethod, pageNumber, pageSize));
                return new ApiResponse<Gov2Biz.Shared.DTOs.PagedResult<PaymentDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Gov2Biz.Shared.DTOs.PagedResult<PaymentDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("user/{payerId}")]
        public async Task<ApiResponse<List<PaymentDto>>> GetUserPayments(int payerId)
        {
            try
            {
                var result = await _mediator.Send(new GetUserPaymentsQuery(payerId));
                return new ApiResponse<List<PaymentDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PaymentDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpPost("{id}/refund")]
        public async Task<ApiResponse<PaymentDto>> RefundPayment(int id, [FromBody] RefundPaymentRequest request)
        {
            try
            {
                var result = await _mediator.Send(new RefundPaymentCommand(id, request.Reason));
                return new ApiResponse<PaymentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaymentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("stats")]
        public async Task<ApiResponse<object>> GetPaymentStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _mediator.Send(new GetPaymentStatsQuery(StartDate: startDate, EndDate: endDate, Status: status));
                return new ApiResponse<object> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, Message = ex.Message };
            }
        }
    }

    public class RefundPaymentRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RefundedBy { get; set; } = string.Empty;
    }
}
