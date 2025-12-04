using Gov2Biz.PaymentService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.PaymentService.CQRS.Handlers
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
    {
        private readonly PaymentDbContext _context;
        private readonly IPaymentGateway _gateway;

        public CreatePaymentHandler(PaymentDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _gateway = serviceProvider.GetRequiredKeyedService<IPaymentGateway>("stripe");
        }

        public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var transactionId = GenerateTransactionId();

            var payment = new Payment
            {
                TransactionId = transactionId,
                PaymentMethod = request.PaymentMethod,
                Amount = request.Amount,
                Status = "Pending",
                Currency = request.Currency,
                ApplicationId = request.ApplicationId,
                PayerId = request.PayerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            // Process payment through gateway
            var paymentRequest = new PaymentRequest
            {
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                Description = $"Payment for application {request.ApplicationId}",
                CustomerEmail = $"user{request.PayerId}@example.com"
            };

            var gatewayResponse = await _gateway.ProcessPaymentAsync(paymentRequest, cancellationToken);

            payment.Status = gatewayResponse.Success ? "Completed" : "Failed";
            payment.GatewayResponse = gatewayResponse.GatewayResponse;
            payment.CompletedAt = gatewayResponse.Success ? DateTime.UtcNow : null;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(payment);
        }

        private string GenerateTransactionId()
        {
            return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N.Substring(0, 8).ToUpper()}";
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }

    public class GetPaymentHandler : IRequestHandler<GetPaymentQuery, PaymentDto>
    {
        private readonly PaymentDbContext _context;

        public GetPaymentHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {request.Id} not found");

            return await MapToDto(payment);
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }

    public class GetPaymentsHandler : IRequestHandler<GetPaymentsQuery, Gov2Biz.Shared.Responses.PagedResult<PaymentDto>>
    {
        private readonly PaymentDbContext _context;

        public GetPaymentsHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.Responses.PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Payments.AsQueryable();

            if (request.ApplicationId.HasValue)
                query = query.Where(p => p.ApplicationId == request.ApplicationId.Value);

            if (request.PayerId.HasValue)
                query = query.Where(p => p.PayerId == request.PayerId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(p => p.Status == request.Status);

            if (!string.IsNullOrEmpty(request.PaymentMethod))
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod);

            var totalCount = await query.CountAsync(cancellationToken);
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<PaymentDto>();
            foreach (var payment in payments)
            {
                dtos.Add(await MapToDto(payment));
            }

            return new Gov2Biz.Shared.Responses.PagedResult<PaymentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }

    public class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand, PaymentDto>
    {
        private readonly PaymentDbContext _context;
        private readonly IPaymentGateway _gateway;

        public RefundPaymentHandler(PaymentDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _gateway = serviceProvider.GetRequiredKeyedService<IPaymentGateway>("stripe");
        }

        public async Task<PaymentDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

            if (payment.Status != "Completed")
                throw new InvalidOperationException("Only completed payments can be refunded");

            var refundRequest = new RefundRequest
            {
                TransactionId = payment.TransactionId,
                Amount = request.Amount,
                Reason = request.Reason
            };

            var refundResponse = await _gateway.RefundPaymentAsync(refundRequest, cancellationToken);

            if (refundResponse.Success)
            {
                payment.Status = "Refunded";
                payment.GatewayResponse += $" | Refunded: {refundResponse.GatewayResponse}";
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(payment);
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }

    public class GetUserPaymentsHandler : IRequestHandler<GetUserPaymentsQuery, List<PaymentDto>>
    {
        private readonly PaymentDbContext _context;

        public GetUserPaymentsHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentDto>> Handle(GetUserPaymentsQuery request, CancellationToken cancellationToken)
        {
            var payments = await _context.Payments
                .Where(p => p.PayerId == request.PayerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = new List<PaymentDto>();
            foreach (var payment in payments)
            {
                dtos.Add(await MapToDto(payment));
            }

            return dtos;
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }

    public class GetPaymentStatsHandler : IRequestHandler<GetPaymentStatsQuery, PaymentStatsDto>
    {
        private readonly PaymentDbContext _context;

        public GetPaymentStatsHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentStatsDto> Handle(GetPaymentStatsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Payments.AsQueryable();

            if (request.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(p => p.CreatedAt <= request.EndDate.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(p => p.Status == request.Status);

            var payments = await query.ToListAsync(cancellationToken);

            return new PaymentStatsDto
            {
                TotalRevenue = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount),
                TotalPayments = payments.Count,
                SuccessfulPayments = payments.Count(p => p.Status == "Completed"),
                FailedPayments = payments.Count(p => p.Status == "Failed"),
                PendingPayments = payments.Count(p => p.Status == "Pending"),
                AveragePaymentAmount = payments.Any() ? payments.Average(p => p.Amount) : 0
            };
        }
    }

    public class GetPaymentByTransactionIdHandler : IRequestHandler<GetPaymentByTransactionIdQuery, PaymentDto>
    {
        private readonly PaymentDbContext _context;

        public GetPaymentByTransactionIdHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with transaction ID {request.TransactionId} not found");

            return await MapToDto(payment);
        }

        private async Task<PaymentDto> MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ApplicationNumber = $"APP-{payment.ApplicationId}",
                PayerName = $"User {payment.PayerId}",
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }
}
