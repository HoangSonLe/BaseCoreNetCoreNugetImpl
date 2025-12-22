using AutoMapper;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Presentation.Controllers.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseSourceImpl.Presentation.Controllers.User
{
    /// <summary>
    /// UserController - API Endpoints
    /// Presentation Layer - S? d?ng AutoMapper
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu c?u JWT token cho t?t c? endpoints
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(
        IUserService userService,
        IMapper mapper,
        ILogger<UserController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// L?y user theo ID
        /// </summary>
        [HttpGet("me")]
        public async Task<ValueResponse<UserViewModel>> GetCurrentUserInfo()
        {
            return await _userService.GetCurrentUserInfo();
        }


        /// <summary>
        /// L?y user theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ValueResponse<UserViewModel>> GetById(int id)
        {
            return await _userService.GetByIdAsync(id);
        }

        /// <summary>
        /// L?y t?t c? users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PageResponse<UserViewModel>), StatusCodes.Status200OK)]
        public async Task<PageResponse<UserViewModel>> GetPageAsync([FromQuery] UserSearchModel searchModel)
        {
            return await _userService.GetPageAsync(searchModel);
        }

        /// <summary>
        /// T?o user m?i - Public endpoint (ví d?: ??ng ký)
        /// </summary>
        [HttpPost]
        public async Task Create([FromBody] CreateUserRequest request)
        {
            // Map Request -> DTO using AutoMapper
            var dto = _mapper.Map<UserDto>(request);

            await _userService.CreateAsync(dto);
        }

        /// <summary>
        /// C?p nh?t user
        /// </summary>
        [HttpPut("{id}")]
        public async Task Update(int id, [FromBody] UpdateUserRequest request)
        {
            var dto = _mapper.Map<UserDto>(request);

            await _userService.UpdateAsync(dto);
        }

        /// <summary>
        /// Xóa user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _userService.DeleteAsync(id);
        }


        [HttpGet("test")]
        public async Task TestPermission(int id)
        {

        }
        [HttpGet("test/ABC")]
        public async Task TestABCPermission(int id)
        {

        }

        [AllowAnonymous]
        [HttpGet("anonymous-test")]

        public async Task TestAllowAnonymous(int id)
        {

        }
    }
}
