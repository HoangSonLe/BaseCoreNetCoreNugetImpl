using AutoMapper;
using BaseNetCore.Core.src.Main.BLL.Services;
using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Security;
using BaseNetCore.Core.src.Main.Security.Algorithm;
using BaseNetCore.Core.src.Main.Security.Token;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Application.Services.TokenSession;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Common.ErrorCodes;
using BaseSourceImpl.Domains.Entities.RefreshToken;
using BaseSourceImpl.Domains.Entities.User;
using BaseSourceImpl.Presentation.Controllers.Auth.Models;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BaseSourceImpl.Application.Services.Auth
{
    /// <summary>
    /// UserService Implementation
    /// Business Logic Layer
    /// </summary>
    public partial class AuthService : BaseService<UserEntity>, IAuthService
    {
        private readonly AesAlgorithm _aes;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly ITokenSessionService _sessionService;

        public AuthService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            AesAlgorithm aesAlgorithm,
            IMemoryCache cache,
            ITokenSessionService sessionService) // injected
            : base(unitOfWork, httpContextAccessor)
        {
            _tokenService = tokenService;
            _userService = userService;
            _aes = aesAlgorithm;
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        public async Task<ValueResponse<JwtToken>> Login(LoginRequest loginRequest)
        {
            UserViewModel userViewModel = (await _userService.GetByUserNameAsync(loginRequest.UserName)).Value;

            if (!PasswordEncoder.Verify(loginRequest.Password, userViewModel.Password))
                throw new InvalidCredentialException();

            var sid = Guid.NewGuid().ToString();
            var claims = BuildUserClaims(userViewModel, sid);

            var access = _tokenService.GenerateAccessToken(claims);
            var refresh = _tokenService.GenerateRefreshToken(claims);

            // Persist session using known sid (no need to parse token)
            var expiresUtc = new JwtSecurityTokenHandler().ReadJwtToken(refresh).ValidTo;
            var refreshEntity = new RefreshTokenEntity
            {
                Token = refresh,
                SessionId = sid,
                UserId = userViewModel.Id.ToString(),
                IsValid = true,
                ExpiresAt = DateTime.SpecifyKind(expiresUtc, DateTimeKind.Utc)
            };
            UnitOfWork.Repository<RefreshTokenEntity>().Add(refreshEntity);
            await UnitOfWork.SaveChangesAsync();

            var jwtToken = new JwtToken
            {
                AccessToken = access,
                RefreshToken = refresh,
                UserId = _aes.Encrypt(userViewModel.Id.ToString()),
                UserName = userViewModel.UserName
            };

            return new ValueResponse<JwtToken>(jwtToken);
        }

        public async Task<ValueResponse<RefreshJwtToken>> RefreshToken(string refreshToken)
        {
            var principal = _tokenService.ValidateToken(refreshToken);
            if (principal == null) throw new InvalidCredentialException();

            var refreshEntity = await UnitOfWork.Repository<RefreshTokenEntity>()
                .FindAsync(i => i.Token == refreshToken && i.IsValid);
            if (refreshEntity == null) throw new InvalidCredentialException();

            var newAccess = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefresh = _tokenService.GenerateRefreshToken(principal.Claims);

            // rotate via session service (updates DB and cache)
            await _sessionService.RefreshSessionAsync(refreshEntity, newRefresh);

            var result = new RefreshJwtToken
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh
            };
            return new ValueResponse<RefreshJwtToken>(result);
        }

        public async Task Logout()
        {
            try
            {
                var sid = TryExtractSidFromCurrentRequest();
                if (!string.IsNullOrEmpty(sid))
                {
                    await _sessionService.InvalidateSessionBySidAsync(sid, CurrentUserId.ToString());
                    return;
                }

                await _sessionService.InvalidateAllSessionsForUserAsync(CurrentUserId.ToString());
            }
            catch
            {
                throw new ServerErrorException();
            }
        }

        /// <summary>
        /// Force logout all sessions that belong to the current user.
        /// This invalidates refresh sessions for the current user and clears their session cache.
        /// </summary>
        public async Task ForceLogoutAllForCurrentUser()
        {
            try
            {
                var userId = CurrentUserId.ToString();
                await _sessionService.InvalidateAllSessionsForUserAsync(userId);
            }
            catch
            {
                throw new ServerErrorException();
            }
        }

        // --- Helpers ---
        private List<Claim> BuildUserClaims(UserViewModel user, string? sid = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("TypeAccount", user.TypeAccount.ToString())
            };
            claims.AddRange(user.RoleIdList.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

            if (!string.IsNullOrEmpty(sid))
            {
                claims.Add(new Claim("sid", sid));
            }

            return claims;
        }

        private string? TryExtractSidFromCurrentRequest()
        {
            try
            {
                var authHeader = HttpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader)) return null;

                const string bearerPrefix = "Bearer ";
                var token = authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring(bearerPrefix.Length).Trim()
                    : authHeader.Trim();

                return _tokenService.GetSidFromToken(token);
            }
            catch
            {
                return null;
            }
        }
    }
}
