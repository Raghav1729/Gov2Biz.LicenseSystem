using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.PaymentService.CQRS.Commands
{
    public record CreatePaymentCommand(
        int ApplicationId,
        int PayerId,
        decimal Amount,
        string PaymentMethod,
        string Currency = "USD"
    ) : IRequest<PaymentDto>;

    public record ProcessPaymentCommand(
        int PaymentId,
        string GatewayResponse,
        string Status
    ) : IRequest<PaymentDto>;

    public record RefundPaymentCommand(
        int PaymentId,
        decimal Amount,
        string Reason,
        string RefundedBy
    ) : IRequest<PaymentDto>;
}
