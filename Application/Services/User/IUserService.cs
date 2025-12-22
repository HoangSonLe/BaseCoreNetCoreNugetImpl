using BaseNetCore.Core.src.Main.Common.Models;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Presentation.Controllers.User.Models;

namespace BaseSourceImpl.Application.Services.User
{
    /// <summary>
    /// IUserService - Business Logic Interface
    /// Làm việc với DTO (nội bộ) và trả về ViewModel (cho client)
    /// </summary>
    public interface IUserService
    {
        Task<ValueResponse<UserViewModel>> GetCurrentUserInfo();
        Task<ValueResponse<UserViewModel>> GetByIdAsync(int id);
        Task<ValueResponse<UserViewModel>> GetByUserNameAsync(string userName);
        Task<PageResponse<UserViewModel>> GetPageAsync(UserSearchModel searchModel);
        Task<UserViewModel> CreateAsync(UserDto dto);
        Task<UserViewModel> UpdateAsync(UserDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
