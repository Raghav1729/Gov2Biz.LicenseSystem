using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.NotificationService.CQRS.Commands
{
    public record CreateNotificationCommand(
        string Title,
        string Message,
        string Type,
        int RecipientId,
        string? EntityReference = null
    ) : IRequest<NotificationDto>;

    public record MarkAsReadCommand(int Id, int UserId) : IRequest<bool>;

    public record MarkAllAsReadCommand(int UserId) : IRequest<bool>;

    public record DeleteNotificationCommand(int Id, int UserId) : IRequest<bool>;
}
