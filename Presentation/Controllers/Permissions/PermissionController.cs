using AutoMapper;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseSourceImpl.Presentation.Controllers.Permissions.Models;
using Microsoft.AspNetCore.Mvc;

namespace BaseSourceImpl.Presentation.Controllers.Permissions
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {

        public PermissionController(
        IMapper mapper,
        ILogger<PermissionController> logger)
        {
        }

        [HttpGet]
        [Route("")]
        public async Task<ValueResponse<PermissionGroup>> GetPermissionList()
        {
            var permissionList = new List<BasePermission>
            {
                new BasePermission
                {
                    Code = 1.ToString(),
                    Name = "Quản lý người dùng"
                }
            };

            return await Task.FromResult(
                new ValueResponse<PermissionGroup>(new PermissionGroup()
                {
                    Menus = permissionList,
                    Actions = permissionList

                })
            );
        }

    }
}
