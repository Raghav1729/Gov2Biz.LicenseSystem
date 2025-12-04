using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.PaymentService.CQRS.Commands
{
    public record ProcessPaymentCommand(
        int PaymentId,
        string GatewayResponse,
        string Status
    ) : IRequest<PaymentDto>;
}
