using Microsoft.AspNetCore.Http;
using MediatR;

namespace Gov2Biz.Shared.DTOs
{
    // Authentication DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TenantDomain { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AgencyId { get; set; } = string.Empty;
        public string TenantDomain { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AgencyId { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // License Application DTOs
    public class CreateLicenseApplicationDto
    {
        public string LicenseType { get; set; } = string.Empty;
        public string AgencyId { get; set; } = string.Empty;
        public decimal ApplicationFee { get; set; }
        public string ApplicantNotes { get; set; } = string.Empty;
        public int ApplicantId { get; set; }
    }

    public class LicenseApplicationDto
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ApplicationFee { get; set; }
        public bool IsPaid { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public string? ReviewerNotes { get; set; }
        public string? RejectionReason { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public int PaymentCount { get; set; }
    }

    // License DTOs
    public class LicenseDto
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RenewedAt { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    // Document DTOs
    public class DocumentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
        public string? Notes { get; set; }
    }

    // Notification DTOs
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? EntityReference { get; set; }
    }

    // Payment DTOs
    public class PaymentDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public string? GatewayResponse { get; set; }
        public string? Notes { get; set; }
    }

    // Agency DTOs
    public class AgencyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int ActiveLicensesCount { get; set; }
        public int PendingApplicationsCount { get; set; }
    }

    // Tenant DTOs
    public class TenantDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
        public int AgencyCount { get; set; }
        public int LicenseCount { get; set; }
    }

    // Dashboard DTOs
    public class DashboardStatsDto
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public int ExpiringSoonLicenses { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingPayments { get; set; }
        public int UnreadNotifications { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
    }

    // Pagination DTOs
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // Filter DTOs
    public class LicenseApplicationFilter
    {
        public string? Status { get; set; }
        public string? LicenseType { get; set; }
        public string? AgencyId { get; set; }
        public int? ApplicantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class LicenseFilter
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? AgencyId { get; set; }
        public int? ApplicantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ExpiringSoon { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // Background Job DTOs
    public class LicenseRenewalJobDto
    {
        public int LicenseId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string ApplicantEmail { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string AgencyId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public int DaysUntilExpiry { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? LastNotifiedAt { get; set; }
    }

    // CQRS Commands
    public record CreateLicenseApplicationCommand(
        string LicenseType,
        string AgencyId,
        decimal ApplicationFee,
        string ApplicantNotes,
        int ApplicantId
    ) : IRequest<LicenseApplicationDto>;

    public record ApproveLicenseApplicationCommand(
        int ApplicationId,
        string ReviewerNotes,
        int ReviewerId
    ) : IRequest<LicenseDto>;

    public record RejectLicenseApplicationCommand(
        int ApplicationId,
        string RejectionReason,
        int ReviewerId
    ) : IRequest<LicenseApplicationDto>;

    public record IssueLicenseCommand(
        int ApplicationId,
        int IssuerId
    ) : IRequest<LicenseDto>;

    public record RenewLicenseCommand(
        int LicenseId,
        int RenewedBy,
        int RenewalPeriodMonths = 12
    ) : IRequest<LicenseDto>;

    public record CreateNotificationCommand(
        string Title,
        string Message,
        string Type,
        int RecipientId,
        string? EntityReference = null
    ) : IRequest<NotificationDto>;

    public record CreatePaymentCommand(
        int ApplicationId,
        int PayerId,
        decimal Amount,
        string PaymentMethod,
        string Currency = "USD"
    ) : IRequest<PaymentDto>;

    // Document CQRS Commands and Queries
    public record UploadDocumentCommand(
        IFormFile File,
        string EntityType,
        int EntityId,
        string DocumentType,
        int UploadedBy,
        string? Notes = null
    ) : IRequest<DocumentDto>;

    public record DeleteDocumentCommand(int DocumentId) : IRequest<bool>;

    public record GetDocumentQuery(int DocumentId) : IRequest<DocumentDto>;

    public record GetDocumentsQuery(
        string? EntityType = null,
        int? EntityId = null,
        string? DocumentType = null,
        int? UploadedBy = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<DocumentDto>>;

    public record GetEntityDocumentsQuery(string EntityType, int EntityId) : IRequest<List<DocumentDto>>;

    public record DownloadDocumentQuery(int DocumentId) : IRequest<byte[]>;

    // Payment CQRS Commands and Queries
    public record RefundPaymentCommand(int PaymentId, string Reason) : IRequest<PaymentDto>;

    public record GetPaymentQuery(int PaymentId) : IRequest<PaymentDto>;

    public record GetPaymentsQuery(
        int? ApplicationId = null,
        int? PayerId = null,
        string? Status = null,
        string? PaymentMethod = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<PaymentDto>>;

    public record GetUserPaymentsQuery(int UserId) : IRequest<List<PaymentDto>>;

    public record GetPaymentStatsQuery(
        string? AgencyId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string? Status = null
    ) : IRequest<object>;

    public record GetPaymentByTransactionIdQuery(string TransactionId) : IRequest<PaymentDto>;

    // Notification CQRS Commands and Queries
    public record MarkAsReadCommand(int NotificationId) : IRequest<NotificationDto>;

    public record MarkAllAsReadCommand(int UserId) : IRequest<bool>;

    public record GetNotificationQuery(int NotificationId) : IRequest<NotificationDto>;

    public record GetNotificationsQuery(
        int RecipientId,
        string? Type = null,
        bool? IsRead = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<NotificationDto>>;

    public record GetUserNotificationsQuery(int UserId) : IRequest<List<NotificationDto>>;

    public record GetUnreadCountQuery(int UserId) : IRequest<int>;

    public record DeleteNotificationCommand(int NotificationId) : IRequest<bool>;

    // CQRS Queries
    public record GetLicenseQuery(int LicenseId) : IRequest<LicenseDto>;
    public record GetLicensesQuery(LicenseFilter Filter) : IRequest<PagedResult<LicenseDto>>;
    public record GetLicenseApplicationQuery(int ApplicationId) : IRequest<LicenseApplicationDto>;
    public record GetLicenseApplicationsQuery(LicenseApplicationFilter Filter) : IRequest<PagedResult<LicenseApplicationDto>>;
    public record GetUserLicensesQuery(int UserId) : IRequest<List<LicenseDto>>;
    public record GetUserApplicationsQuery(int UserId) : IRequest<List<LicenseApplicationDto>>;
    public record GetDashboardStatsQuery(string? AgencyId = null) : IRequest<DashboardStatsDto>;
}