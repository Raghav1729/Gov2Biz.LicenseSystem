using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Gov2Biz.Shared.DTOs;

namespace Gov2Biz.Web.Services
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
    
    public interface ILicenseServiceClient
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(string? agencyId = null);
        Task<PagedResult<LicenseDto>> GetLicensesAsync(LicenseFilter filter);
        Task<PagedResult<LicenseApplicationDto>> GetApplicationsAsync(LicenseApplicationFilter filter);
        Task<LicenseDto> GetLicenseAsync(int id);
        Task<LicenseApplicationDto> GetApplicationAsync(int id);
        Task<LicenseApplicationDto> CreateApplicationAsync(CreateLicenseApplicationCommand command);
        Task<LicenseDto> IssueLicenseAsync(int applicationId);
        Task<LicenseDto> RenewLicenseAsync(int id, RenewLicenseCommand command);
        Task<LicenseApplicationDto> ApproveApplicationAsync(int id, ApproveLicenseApplicationCommand command);
        Task<LicenseApplicationDto> RejectApplicationAsync(int id, RejectLicenseApplicationCommand command);
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

            var baseUrl = _configuration["ServiceUrls:LicenseService"] ?? "http://licenseservice:5002";
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

        public async Task<PagedResult<LicenseDto>> GetLicensesAsync(LicenseFilter filter)
        {
            try
            {
                var queryString = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
                if (!string.IsNullOrEmpty(filter.AgencyId))
                    queryString += $"&agencyId={filter.AgencyId}";
                if (!string.IsNullOrEmpty(filter.Status))
                    queryString += $"&status={filter.Status}";
                if (filter.ApplicantId.HasValue)
                    queryString += $"&applicantId={filter.ApplicantId}";

                var response = await _httpClient.GetAsync($"/api/licenses{queryString}");
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
                return new LicenseDto();
            }
        }

        public async Task<LicenseDto> RenewLicenseAsync(int id, RenewLicenseCommand command)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/licenses/{id}/renew", content);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseDto>>();
                return result?.Data ?? new LicenseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing license {Id}", id);
                return new LicenseDto();
            }
        }

        public async Task<PagedResult<LicenseApplicationDto>> GetApplicationsAsync(LicenseApplicationFilter filter)
        {
            try
            {
                var queryString = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
                if (!string.IsNullOrEmpty(filter.AgencyId))
                    queryString += $"&agencyId={filter.AgencyId}";
                if (!string.IsNullOrEmpty(filter.Status))
                    queryString += $"&status={filter.Status}";
                if (filter.ApplicantId.HasValue)
                    queryString += $"&applicantId={filter.ApplicantId}";

                var response = await _httpClient.GetAsync($"/api/applications{queryString}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<LicenseApplicationDto>>>();
                return result?.Data ?? new PagedResult<LicenseApplicationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applications");
                return new PagedResult<LicenseApplicationDto>();
            }
        }

        public async Task<LicenseApplicationDto> GetApplicationAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/applications/{id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching application {Id}", id);
                return new LicenseApplicationDto();
            }
        }

        public async Task<LicenseApplicationDto> CreateApplicationAsync(CreateLicenseApplicationCommand command)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/applications", content);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating application");
                return new LicenseApplicationDto();
            }
        }

        public async Task<LicenseApplicationDto> ApproveApplicationAsync(int id, ApproveLicenseApplicationCommand command)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/applications/{id}/approve", content);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving application {Id}", id);
                return new LicenseApplicationDto();
            }
        }

        public async Task<LicenseApplicationDto> RejectApplicationAsync(int id, RejectLicenseApplicationCommand command)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/applications/{id}/reject", content);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LicenseApplicationDto>>();
                return result?.Data ?? new LicenseApplicationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting application {Id}", id);
                return new LicenseApplicationDto();
            }
        }
    }

    // Placeholder interfaces for other services
    public interface IDocumentServiceClient
    {
        Task<List<DocumentDto>> GetDocumentsAsync(int entityId, string entityType);
    }

    public interface INotificationServiceClient
    {
        Task<List<NotificationDto>> GetNotificationsAsync(int userId);
        Task<NotificationDto> GetNotificationAsync(int id);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<NotificationDto> CreateNotificationAsync(string message, int userId);
    }

    public interface IPaymentServiceClient
    {
        Task<PaymentDto> GetPaymentAsync(int id);
    }

    // Placeholder implementations
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

            var baseUrl = _configuration["ServiceUrls:DocumentService"] ?? "http://documentservice:5003";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<List<DocumentDto>> GetDocumentsAsync(int entityId, string entityType) 
        {
            return Task.FromResult(new List<DocumentDto>());
        }

        public Task<DocumentDto> GetDocumentAsync(int id) 
        {
            return Task.FromResult(new DocumentDto());
        }

        public Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command) 
        {
            return Task.FromResult(new DocumentDto());
        }

        public Task<bool> DeleteDocumentAsync(int id) 
        {
            return Task.FromResult(false);
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

            var baseUrl = _configuration["ServiceUrls:NotificationService"] ?? "http://notificationservice:5004";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<List<NotificationDto>> GetNotificationsAsync(int userId) 
        {
            return Task.FromResult(new List<NotificationDto>());
        }

        public Task<NotificationDto> GetNotificationAsync(int id) 
        {
            return Task.FromResult(new NotificationDto());
        }

        public Task<bool> MarkAsReadAsync(int notificationId) 
        {
            return Task.FromResult(false);
        }

        public Task<NotificationDto> CreateNotificationAsync(string message, int userId) 
        {
            return Task.FromResult(new NotificationDto());
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

            var baseUrl = _configuration["ServiceUrls:PaymentService"] ?? "http://paymentservice:5005";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<PaymentDto> GetPaymentAsync(int id) 
        {
            return Task.FromResult(new PaymentDto());
        }
    }
}
