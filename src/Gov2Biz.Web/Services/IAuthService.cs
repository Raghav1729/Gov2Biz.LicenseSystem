using Gov2Biz.Shared.DTOs;

namespace Gov2Biz.Web.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email, string tenantId);
        Task<bool> ValidateUserAsync(string email, string password, string tenantId);
    }
}
