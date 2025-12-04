using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.PaymentService.CQRS.Queries
{
    public record GetPaymentQuery(int Id) : IRequest<PaymentDto>;
    
    public record GetPaymentsQuery(
        int? ApplicationId = null,
        int? PayerId = null,
        string? Status = null,
        string? PaymentMethod = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Gov2Biz.Shared.Responses.PagedResult<PaymentDto>>;

    public record GetUserPaymentsQuery(int PayerId) : IRequest<List<PaymentDto>>;

    public record GetPaymentByTransactionIdQuery(string TransactionId) : IRequest<PaymentDto>;

    public record GetPaymentStatsQuery(
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string? Status = null
    ) : IRequest<PaymentStatsDto>;
}

public class PaymentStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public decimal AveragePaymentAmount { get; set; }
}
