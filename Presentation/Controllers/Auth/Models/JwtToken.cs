namespace BaseSourceImpl.Presentation.Controllers.Auth.Models
{
    public record JwtToken
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public string UserId { get; init; }
        public string UserName { get; init; }
    }
    public record RefreshJwtToken
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
    }

}
