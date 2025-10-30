using BaseNetCore.Core.src.Main.Common.Models;
using BaseSourceImpl.Presentation.Controllers.Auth.Models;

namespace BaseSourceImpl.Application.Services.Auth
{
    /// <summary>
    /// IUserService - Business Logic Interface
    /// Làm việc với DTO (nội bộ) và trả về ViewModel (cho client)
    /// </summary>
    public interface IAuthService
    {
        Task<ValueResponse<JwtToken>> Login(LoginRequest loginRequest);
        Task<ValueResponse<RefreshJwtToken>> RefreshToken(string refreshToken);
        Task Logout();

        // Force logout all sessions for the current authenticated user
        Task ForceLogoutAllForCurrentUser();
    }
}
