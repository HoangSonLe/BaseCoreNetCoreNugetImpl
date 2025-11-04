using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Domains.Entities.RefreshToken
{
    /// <summary>
    /// Refresh token record (one per session).
    /// </summary>
    public class RefreshTokenEntity
    {
        [Key]
        public int Id { get; set; }

        // Stored refresh token string
        public string Token { get; set; }

        // Session identifier (sid) that is also embedded in access token
        public string SessionId { get; set; }

        // The user this refresh token belongs to
        public string UserId { get; set; }

        // Whether this refresh token / session is still valid
        public bool IsValid { get; set; }

        // Expiration time of the refresh token (UTC)
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
