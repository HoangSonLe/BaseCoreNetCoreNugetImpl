using AutoMapper;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseSourceImpl.Application.Services.Auth;
using BaseSourceImpl.Presentation.Controllers.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseSourceImpl.Presentation.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
        IAuthService authService,
        IMapper mapper,
        ILogger<AuthController> logger)
        {
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ValueResponse<JwtToken>> Login([FromBody] LoginRequest request)
        {
            return await _authService.Login(request);
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<ValueResponse<RefreshJwtToken>> RefreshToken([FromBody] string refreshToken)
        {
            return await _authService.RefreshToken(refreshToken);
        }

        [HttpPost]
        [Authorize]
        [Route("logout")]
        public async Task Logout()
        {
            await _authService.Logout();
        }

        // Force logout all sessions of the current authenticated user
        [HttpPost]
        [Authorize]
        [Route("force-logout-all")]
        public async Task<IActionResult> ForceLogoutAllForCurrentUser()
        {
            await _authService.ForceLogoutAllForCurrentUser();
            return NoContent();
        }
    }
}
