using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gov2Biz.Web.Services
{
    public interface ILicenseServiceClient
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(string? agencyId = null);
        Task<PagedResult<LicenseApplicationDto>> GetApplicationsAsync(LicenseApplicationFilter filter);
        Task<PagedResult<LicenseDto>> GetLicensesAsync(LicenseFilter filter);
        Task<LicenseApplicationDto> GetApplicationAsync(int id);
        Task<LicenseDto> GetLicenseAsync(int id);
        Task<LicenseApplicationDto> CreateApplicationAsync(CreateLicenseApplicationCommand command);
        Task<LicenseDto> ApproveApplicationAsync(int id, ApproveLicenseApplicationCommand command);
        Task<LicenseApplicationDto> RejectApplicationAsync(int id, RejectLicenseApplicationCommand command);
        Task<LicenseDto> IssueLicenseAsync(int applicationId);
        Task<LicenseDto> RenewLicenseAsync(int id, RenewLicenseCommand command);
    }

    public interface IDocumentServiceClient
    {
        Task<DocumentDto> GetDocumentAsync(int id);
        Task<List<DocumentDto>> GetDocumentsAsync(string entityType, int entityId);
        Task<DocumentDto> UploadDocumentAsync(UploadDocumentRequest request);
        Task<bool> DeleteDocumentAsync(int id);
    }

    public interface INotificationServiceClient
    {
        Task<List<NotificationDto>> GetNotificationsAsync(int recipientId);
        Task<NotificationDto> GetNotificationAsync(int id);
        Task<bool> MarkAsReadAsync(int id);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationCommand command);
    }

    public interface IPaymentServiceClient
    {
        Task<PaymentDto> GetPaymentAsync(int id);
        Task<List<PaymentDto>> GetPaymentsAsync(int applicationId);
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentCommand command);
        Task<PaymentDto> ProcessPaymentAsync(int paymentId);
    }

    public class LicenseServiceClient : ILicenseServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LicenseServiceClient> _logger;

        public LicenseServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<LicenseServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["ServiceUrls:LicenseService"] ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string? agencyId = null)
        {
            try
            {
                var url = $"/api/licenses/dashboard/stats{(agencyId != null ? $"?agencyId={agencyId}" : "")}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardStatsDto>>();
                return result?.Data ?? new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return new DashboardStatsDto();
            }
        }

        public async Task<PagedResult<LicenseApplicationDto>> GetApplicationsAsync(LicenseApplicationFilter filter)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"status={filter.Status}");
                if (!string.IsNullOrEmpty(filter.LicenseType)) queryParams.Add($"licenseType={filter.LicenseType}");
                if (!string.IsNullOrEmpty(filter.AgencyId)) queryParams.Add($"agencyId={filter.AgencyId}");
                if (filter.ApplicantId.HasValue) queryParams.Add($"applicantId={filter.ApplicantId}");
                queryParams.Add($"pageNumber={filter.PageNumber}");
                queryParams.Add($"pageSize={filter.PageSize}");

                var url = $"/api/licenses/applications?{string.Join("&", queryParams)}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<LicenseApplicationDto>>>();
                return result?.Data ?? new PagedResult<LicenseApplicationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching license applications");
                return new PagedResult<LicenseApplicationDto>();
            }
        }

        public async Task<PagedResult<LicenseDto>> GetLicensesAsync(LicenseFilter filter)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"status={filter.Status}");
                if (!string.IsNullOrEmpty(filter.Type)) queryParams.Add($"type={filter.Type}");
                if (!string.IsNullOrEmpty(filter.AgencyId)) queryParams.Add($"agencyId={filter.AgencyId}");
                if (filter.ApplicantId.HasValue) queryParams.Add($"applicantId={filter.ApplicantId}");
                queryParams.Add($"pageNumber={filter.PageNumber}");
                queryParams.Add($"pageSize={filter.PageSize}");

                var url = $"/api/licenses?{string.Join("&", queryParams)}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<LicenseDto>>>();
                return result?.Data ?? new PagedResult<LicenseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching licenses");
                return new PagedResult<LicenseDto>();
            }
        }

        public async Task<LicenseApplicationDto> GetApplicationAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/licenses/applications/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching license application {Id}", id);
                return new LicenseApplicationDto();
            }
        }

        public async Task<LicenseDto> GetLicenseAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/licenses/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseDto>>();
                return result?.Data ?? new LicenseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching license {Id}", id);
                return new LicenseDto();
            }
        }

        public async Task<LicenseApplicationDto> CreateApplicationAsync(CreateLicenseApplicationCommand command)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/licenses/applications", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating license application");
                throw;
            }
        }

        public async Task<LicenseDto> ApproveApplicationAsync(int id, ApproveLicenseApplicationCommand command)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/licenses/applications/{id}/approve", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseDto>>();
                return result?.Data ?? new LicenseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving license application {Id}", id);
                throw;
            }
        }

        public async Task<LicenseApplicationDto> RejectApplicationAsync(int id, RejectLicenseApplicationCommand command)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/licenses/applications/{id}/reject", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting license application {Id}", id);
                throw;
            }
        }

        public async Task<LicenseDto> IssueLicenseAsync(int applicationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/licenses/{applicationId}/issue", null);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseDto>>();
                return result?.Data ?? new LicenseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing license for application {ApplicationId}", applicationId);
                throw;
            }
        }

        public async Task<LicenseDto> RenewLicenseAsync(int id, RenewLicenseCommand command)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/licenses/{id}/renew", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseDto>>();
                return result?.Data ?? new LicenseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing license {Id}", id);
                throw;
            }
        }
    }

    public class DocumentServiceClient : IDocumentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentServiceClient> _logger;

        public DocumentServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DocumentServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["ServiceUrls:DocumentService"] ?? "http://localhost:5002";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<DocumentDto> GetDocumentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/documents/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<DocumentDto>>();
                return result?.Data ?? new DocumentDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching document {Id}", id);
                return new DocumentDto();
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsAsync(string entityType, int entityId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/documents?entityType={entityType}&entityId={entityId}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<DocumentDto>>>();
                return result?.Data ?? new List<DocumentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching documents for {EntityType} {EntityId}", entityType, entityId);
                return new List<DocumentDto>();
            }
        }

        public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentRequest request)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = request.File.OpenReadStream();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.File.ContentType);
                content.Add(fileContent, "file", request.File.FileName);
                content.Add(new StringContent(request.EntityType), "entityType");
                content.Add(new StringContent(request.EntityId.ToString()), "entityId");
                content.Add(new StringContent(request.DocumentType), "documentType");
                content.Add(new StringContent(request.UploadedBy.ToString()), "uploadedBy");
                if (!string.IsNullOrEmpty(request.Notes))
                    content.Add(new StringContent(request.Notes), "notes");

                var response = await _httpClient.PostAsync("/api/documents/upload", content);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<DocumentDto>>();
                return result?.Data ?? new DocumentDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/documents/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {Id}", id);
                return false;
            }
        }
    }

    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["ServiceUrls:NotificationService"] ?? "http://localhost:5003";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(int recipientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notifications?recipientId={recipientId}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<NotificationDto>>>();
                return result?.Data ?? new List<NotificationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for recipient {RecipientId}", recipientId);
                return new List<NotificationDto>();
            }
        }

        public async Task<NotificationDto> GetNotificationAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notifications/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<NotificationDto>>();
                return result?.Data ?? new NotificationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification {Id}", id);
                return new NotificationDto();
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/notifications/{id}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {Id} as read", id);
                return false;
            }
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationCommand command)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/notifications", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<NotificationDto>>();
                return result?.Data ?? new NotificationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }
    }

    public class PaymentServiceClient : IPaymentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentServiceClient> _logger;

        public PaymentServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PaymentServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["ServiceUrls:PaymentService"] ?? "http://localhost:5004";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PaymentDto> GetPaymentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/payments/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();
                return result?.Data ?? new PaymentDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment {Id}", id);
                return new PaymentDto();
            }
        }

        public async Task<List<PaymentDto>> GetPaymentsAsync(int applicationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/payments?applicationId={applicationId}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PaymentDto>>>();
                return result?.Data ?? new List<PaymentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payments for application {ApplicationId}", applicationId);
                return new List<PaymentDto>();
            }
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentCommand command)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/payments", command);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();
                return result?.Data ?? new PaymentDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        public async Task<PaymentDto> ProcessPaymentAsync(int paymentId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/payments/{paymentId}/process", null);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();
                return result?.Data ?? new PaymentDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
