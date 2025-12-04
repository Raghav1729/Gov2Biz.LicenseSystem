namespace Gov2Biz.Shared.Models
{
    public class Tenant
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string? ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string? AgencyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? PasswordHash { get; set; }
    }

    public class Agency
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class License
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public int ApplicantId { get; set; }
        public string AgencyId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RenewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual LicenseApplication Application { get; set; } = null!;
        public virtual User Applicant { get; set; } = null!;
        public virtual Agency Agency { get; set; } = null!;
    }

    public class LicenseApplication
    {
        public int Id { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ApplicantId { get; set; }
        public string AgencyId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public int? ReviewerId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ReviewerNotes { get; set; }
        public string? RejectionReason { get; set; }
        public decimal ApplicationFee { get; set; }
        public bool IsPaid { get; set; }

        // Navigation properties
        public virtual User Applicant { get; set; } = null!;
        public virtual Agency Agency { get; set; } = null!;
        public virtual User? Reviewer { get; set; } = null!;
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual User Uploader { get; set; } = null!;
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int RecipientId { get; set; }
        public string? EntityReference { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User Recipient { get; set; } = null!;
    }

    public class Payment
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public int PayerId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? GatewayResponse { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual LicenseApplication Application { get; set; } = null!;
        public virtual User Payer { get; set; } = null!;
    }
}