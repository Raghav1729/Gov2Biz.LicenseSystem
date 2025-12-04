using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gov2Biz.Shared.DTOs;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.Configuration;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Gov2Biz.Web.Data;

namespace Gov2Biz.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly ApplicationDbContext _context;

        public AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthService> logger, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {request.Email}, tenant: {request.TenantDomain}");

                // Find user by email and tenant
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == request.TenantDomain);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {request.Email}");
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid email, password, or tenant domain" 
                    };
                }

                // Simple password check (in production, use proper hashing)
                if (user.PasswordHash != $"plain:{request.Password}")
                {
                    _logger.LogWarning($"Invalid password for user: {request.Email}");
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid email, password, or tenant domain" 
                    };
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Inactive user attempted login: {request.Email}");
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Account is inactive" 
                    };
                }

                // Get agency information if user has an agency
                var agencyName = "";
                if (!string.IsNullOrEmpty(user.AgencyId))
                {
                    var agency = await _context.Agencies.FindAsync(user.AgencyId);
                    agencyName = agency?.Name ?? "";
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = $"{user.FirstName} {user.LastName}",
                    Role = user.Role,
                    AgencyId = user.AgencyId ?? "",
                    AgencyName = agencyName,
                    TenantId = user.TenantId,
                    TenantName = user.TenantId, // Simplified
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                _logger.LogInformation($"User logged in successfully: {request.Email}");

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = "local-auth", // Not used for cookie auth
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var baseUrl = _configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5001";
                var userUrl = $"{baseUrl}/api/auth/user/{userId}";

                var response = await _httpClient.GetAsync(userUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<UserDto>(responseContent, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ValidateUserAsync(string email, string password, string tenantId)
        {
            var loginResponse = await LoginAsync(new LoginRequest 
            { 
                Email = email, 
                Password = password, 
                TenantDomain = tenantId 
            });
            return loginResponse.Success;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email, string tenantId)
        {
            try
            {
                var baseUrl = _configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5001";
                var userUrl = $"{baseUrl}/api/auth/user/by-email/{email}?tenantId={tenantId}";

                var response = await _httpClient.GetAsync(userUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<UserDto>(responseContent, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return null;
            }
        }
    }
}
