using Microsoft.AspNetCore.Mvc;
using MediatR;
using Gov2Biz.NotificationService.CQRS.Commands;
using Gov2Biz.NotificationService.CQRS.Queries;
using Gov2Biz.Shared.Responses;

namespace Gov2Biz.NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<NotificationDto>> CreateNotification([FromBody] CreateNotificationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return new ApiResponse<NotificationDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<NotificationDto>> GetNotification(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetNotificationQuery(id));
                return new ApiResponse<NotificationDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<NotificationDto>>> GetNotifications(
            [FromQuery] int recipientId,
            [FromQuery] string? type = null,
            [FromQuery] bool? isRead = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetNotificationsQuery(recipientId, type, isRead, pageNumber, pageSize));
                return new ApiResponse<PagedResult<NotificationDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<NotificationDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ApiResponse<List<NotificationDto>>> GetUserNotifications(int userId)
        {
            try
            {
                var result = await _mediator.Send(new GetUserNotificationsQuery(userId));
                return new ApiResponse<List<NotificationDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificationDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("unread/{userId}")]
        public async Task<ApiResponse<int>> GetUnreadCount(int userId)
        {
            try
            {
                var result = await _mediator.Send(new GetUnreadCountQuery(userId));
                return new ApiResponse<int> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("{id}/read")]
        public async Task<ApiResponse<bool>> MarkAsRead(int id, [FromBody] MarkAsReadRequest request)
        {
            try
            {
                var result = await _mediator.Send(new MarkAsReadCommand(id, request.UserId));
                return new ApiResponse<bool> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        [HttpPut("user/{userId}/read-all")]
        public async Task<ApiResponse<bool>> MarkAllAsRead(int userId)
        {
            try
            {
                var result = await _mediator.Send(new MarkAllAsReadCommand(userId));
                return new ApiResponse<bool> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse<bool>> DeleteNotification(int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _mediator.Send(new DeleteNotificationCommand(id, userId));
                return new ApiResponse<bool> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }

    public class MarkAsReadRequest
    {
        public int UserId { get; set; }
    }
}
