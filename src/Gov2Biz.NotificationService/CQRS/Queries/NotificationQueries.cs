using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.NotificationService.CQRS.Queries
{
    public record GetNotificationQuery(int Id) : IRequest<NotificationDto>;
    
    public record GetNotificationsQuery(
        int RecipientId,
        string? Type = null,
        bool? IsRead = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Gov2Biz.Shared.Responses.PagedResult<NotificationDto>>;

    public record GetUnreadCountQuery(int RecipientId) : IRequest<int>;

    public record GetUserNotificationsQuery(int RecipientId) : IRequest<List<NotificationDto>>;
}
