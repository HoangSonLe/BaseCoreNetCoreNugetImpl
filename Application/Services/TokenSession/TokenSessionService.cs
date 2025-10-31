using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseSourceImpl.Domains.Entities.RefreshToken;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace BaseSourceImpl.Application.Services.TokenSession
{
    public class TokenSessionService : ITokenSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string SessionCachePrefix = "session_valid:";

        public TokenSessionService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task RefreshSessionAsync(RefreshTokenEntity existing, string newRefreshToken)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            var (newSid, newExpires) = ParseSidAndExpiry(newRefreshToken);
            if (string.IsNullOrEmpty(newSid)) newSid = existing.SessionId ?? Guid.NewGuid().ToString();

            existing.Token = newRefreshToken;
            existing.SessionId = newSid;
            existing.ExpiresAt = new DateTimeOffset(DateTime.SpecifyKind(newExpires, DateTimeKind.Utc));
            existing.IsValid = true;

            _unitOfWork.Repository<RefreshTokenEntity>().Update(existing);
            await _unitOfWork.SaveChangesAsync();

            CacheSession(newSid, existing.ExpiresAt);
        }

        public async Task InvalidateSessionBySidAsync(string sid, string? userId = null)
        {
            if (string.IsNullOrEmpty(sid)) return;

            var entity = await _unitOfWork.Repository<RefreshTokenEntity>()
                .FindAsync(i => i.SessionId == sid && i.IsValid && (userId == null || i.UserId == userId));

            if (entity != null)
            {
                _unitOfWork.Repository<RefreshTokenEntity>().Delete(entity);
                await _unitOfWork.SaveChangesAsync();
            }

            _cache.Remove(GetCacheKey(sid));
        }

        public async Task InvalidateAllSessionsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var sessions = await _unitOfWork.Repository<RefreshTokenEntity>()
                .GetAllAsync(i => i.UserId == userId && i.IsValid);

            if (sessions?.Any() == true)
            {
                foreach (var s in sessions)
                {
                    try { _cache.Remove(GetCacheKey(s.SessionId)); } catch { }
                }

                _unitOfWork.Repository<RefreshTokenEntity>().DeleteRange(sessions);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> IsSessionValidAsync(string sid)
        {
            if (string.IsNullOrEmpty(sid)) return false;
            var cacheKey = GetCacheKey(sid);
            if (_cache.TryGetValue(cacheKey, out bool cached)) return cached;

            var session = await _unitOfWork.Repository<RefreshTokenEntity>()
                .FindAsync(r => r.SessionId == sid && r.IsValid);

            var valid = session != null && (!session.ExpiresAt.HasValue || session.ExpiresAt.Value.UtcDateTime > DateTime.UtcNow);

            if (valid)
            {
                _cache.Set(cacheKey, true, ComputeTtl(session?.ExpiresAt));
            }
            else
            {
                _cache.Set(cacheKey, false, TimeSpan.FromSeconds(10));
            }

            return valid;
        }
        
        // --- helpers ---

        private static (string sid, DateTime expiresUtc) ParseSidAndExpiry(string jwt)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var sid = token.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
                var expires = token.ValidTo; // UTC
                return (sid ?? string.Empty, expires);
            }
            catch
            {
                return (string.Empty, DateTime.UtcNow.AddSeconds(60));
            }
        }

        private void CacheSession(string sid, DateTimeOffset? expiresAt)
        {
            if (string.IsNullOrEmpty(sid)) return;
            try
            {
                _cache.Set(GetCacheKey(sid), true, ComputeTtl(expiresAt));
            }
            catch
            {
                // swallow cache errors
            }
        }

        private static string GetCacheKey(string sid) => SessionCachePrefix + sid;

        private static TimeSpan ComputeTtl(DateTimeOffset? expiresAt)
        {
            TimeSpan ttl = TimeSpan.FromSeconds(60);
            if (expiresAt.HasValue)
            {
                ttl = expiresAt.Value.UtcDateTime - DateTime.UtcNow;
            }

            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(10);
            return ttl;
        }
    }
}
