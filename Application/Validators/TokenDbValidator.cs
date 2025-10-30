using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Security.Token;
using BaseSourceImpl.Application.Services.TokenSession;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace BaseSourceImpl.Application.Validators
{
    public class TokenDbValidator : ITokenValidator
    {
        private readonly ITokenService _tokenService;
        private readonly ITokenSessionService _sessionService;

        public TokenDbValidator(IUnitOfWork unitOfWork, ITokenService tokenService, IMemoryCache cache, ITokenSessionService sessionService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string rawToken, HttpContext httpContext)
        {
            if (principal == null) return false;

            // Prefer sid-based validation and require it.
            var sid = principal.FindFirst("sid")?.Value;
            if (string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(rawToken))
            {
                sid = _tokenService.GetSidFromToken(rawToken);
            }

            if (!string.IsNullOrEmpty(sid))
            {
                // If session is valid -> accept; otherwise reject immediately
                return await _sessionService.IsSessionValidAsync(sid);
            }

            // NO SID => reject. Avoid permissive user-level fallback that allows stale tokens to work.
            // If you need per-token revocation: implement JTI blacklist and check jti here.
            return false;
        }
    }
}
