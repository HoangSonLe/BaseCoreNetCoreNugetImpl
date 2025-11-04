using BaseSourceImpl.Domains.Entities.RefreshToken;

namespace BaseSourceImpl.Application.Services.TokenSession
{
    public interface ITokenSessionService
    {
        /// <summary>
        /// Rotate an existing refresh-session record to a new refresh token (update token, expiry, sid) and refresh cache.
        /// </summary>
        Task RefreshSessionAsync(RefreshTokenEntity existing, string newRefreshToken);

        /// <summary>
        /// Invalidate a single session by sid (delete or mark invalid) and remove cache.
        /// </summary>
        Task InvalidateSessionBySidAsync(string sid, string? userId = null);

        /// <summary>
        /// Invalidate all sessions for a user and clear related cache entries.
        /// </summary>
        Task InvalidateAllSessionsForUserAsync(string userId);

        /// <summary>
        /// Return true when a session id exists, is valid and not expired.
        /// </summary>
        Task<bool> IsSessionValidAsync(string sid);
    }
}
