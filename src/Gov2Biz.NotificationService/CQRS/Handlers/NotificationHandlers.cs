using Gov2Biz.NotificationService.Data;
using Gov2Biz.NotificationService.Services;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.NotificationService.CQRS.Handlers
{
    public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
    {
        private readonly NotificationDbContext _context;
        private readonly INotificationSender _emailSender;
        private readonly INotificationSender _smsSender;
        private readonly INotificationSender _pushSender;

        public CreateNotificationHandler(
            NotificationDbContext context,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _emailSender = serviceProvider.GetRequiredKeyedService<INotificationSender>("email");
            _smsSender = serviceProvider.GetRequiredKeyedService<INotificationSender>("sms");
            _pushSender = serviceProvider.GetRequiredKeyedService<INotificationSender>("push");
        }

        public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                RecipientId = request.RecipientId,
                EntityReference = request.EntityReference,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            // Send notification based on type
            await SendNotificationAsync(notification, cancellationToken);

            return MapToDto(notification);
        }

        private async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                switch (notification.Type.ToLower())
                {
                    case "email":
                        await _emailSender.SendEmailAsync(
                            $"user{notification.RecipientId}@example.com",
                            notification.Title,
                            notification.Message,
                            cancellationToken);
                        break;
                    case "sms":
                        await _smsSender.SendSmsAsync(
                            "+1234567890",
                            notification.Message,
                            cancellationToken);
                        break;
                    case "push":
                        await _pushSender.SendPushNotificationAsync(
                            notification.RecipientId,
                            notification.Title,
                            notification.Message,
                            cancellationToken);
                        break;
                    case "system":
                        // System notifications are stored but not sent externally
                        break;
                    default:
                        // Default to system notification
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the notification creation
                // TODO: Add proper logging
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                EntityReference = notification.EntityReference
            };
        }
    }

    public class GetNotificationHandler : IRequestHandler<GetNotificationQuery, NotificationDto>
    {
        private readonly NotificationDbContext _context;

        public GetNotificationHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationDto> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found");

            return MapToDto(notification);
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                EntityReference = notification.EntityReference
            };
        }
    }

    public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, Gov2Biz.Shared.DTOs.PagedResult<NotificationDto>>
    {
        private readonly NotificationDbContext _context;

        public GetNotificationsHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.DTOs.PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Notifications.Where(n => n.RecipientId == request.RecipientId);

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(n => n.Type == request.Type);

            if (request.IsRead.HasValue)
                query = query.Where(n => n.IsRead == request.IsRead.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = notifications.Select(MapToDto).ToList();

            return new Gov2Biz.Shared.DTOs.PagedResult<NotificationDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                EntityReference = notification.EntityReference
            };
        }
    }

    public class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, NotificationDto>
    {
        private readonly NotificationDbContext _context;

        public MarkAsReadHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationDto> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(notification);
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                EntityReference = notification.EntityReference
            };
        }
    }

    public class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, bool>
    {
        private readonly NotificationDbContext _context;

        public MarkAllAsReadHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == request.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, int>
    {
        private readonly NotificationDbContext _context;

        public GetUnreadCountHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == request.UserId && !n.IsRead, cancellationToken);
        }
    }

    public class GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>
    {
        private readonly NotificationDbContext _context;

        public GetUserNotificationsHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == request.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Limit to last 50 notifications
                .ToListAsync(cancellationToken);

            return notifications.Select(MapToDto).ToList();
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                EntityReference = notification.EntityReference
            };
        }
    }

    public class DeleteNotificationHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly NotificationDbContext _context;

        public DeleteNotificationHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
