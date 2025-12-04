using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gov2Biz.Shared.DTOs;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.Configuration;
using Gov2Biz.LicenseService.Data;

namespace Gov2Biz.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly LicenseDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(LicenseDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await ValidateCredentials(request.Email, request.Password, request.TenantDomain);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email, password, or tenant domain"
                    };
                }

                var token = await GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var agency = !string.IsNullOrEmpty(user.AgencyId) 
                ? await _context.Agencies.FirstOrDefaultAsync(a => a.Id == user.AgencyId)
                : null;

            return MapToUserDto(user, agency);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email, string tenantId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);

            if (user == null) return null;

            var agency = !string.IsNullOrEmpty(user.AgencyId) 
                ? await _context.Agencies.FirstOrDefaultAsync(a => a.Id == user.AgencyId)
                : null;

            return MapToUserDto(user, agency);
        }

        public async Task<bool> ValidateUserAsync(string email, string password, string tenantId)
        {
            var user = await ValidateCredentials(email, password, tenantId);
            return user != null;
        }

        private async Task<User?> ValidateCredentials(string email, string password, string tenantDomain)
        {
            // First get tenant by domain
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Domain == tenantDomain && t.IsActive);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found for domain: {TenantDomain}", tenantDomain);
                return null;
            }

            // Get user by email and tenant
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenant.Id && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email} in tenant: {TenantId}", email, tenant.Id);
                return null;
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for email: {Email}", email);
                return null;
            }

            return user;
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("TenantId", user.TenantId),
                new Claim("AgencyId", user.AgencyId ?? "")
            };

            if (user.AgencyId != null)
            {
                var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == user.AgencyId);
                claims.Add(new Claim("AgencyName", agency?.Name ?? ""));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto MapToUserDto(User user, Agency? agency = null)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                AgencyId = user.AgencyId ?? "",
                AgencyName = agency?.Name ?? "",
                TenantId = user.TenantId,
                TenantName = "", // Would need to join with tenant table
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        private bool VerifyPassword(string password, string? passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;

            // For the default accounts, use simple verification
            // In production, use proper password hashing like BCrypt
            var parts = passwordHash.Split(':');
            if (parts.Length == 2)
            {
                var storedPassword = parts[1];
                return password == storedPassword;
            }

            return false;
        }

        private string HashPassword(string password)
        {
            // Simple hashing for demo - in production use BCrypt or similar
            return "plain:" + password;
        }
    }
}
