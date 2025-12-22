using BaseNetCore.Core.src.Main.Utils;
using BaseSourceImpl.Common.Enums;

namespace BaseSourceImpl.Application.DTOs.User
{
    /// <summary>
    /// UserViewModel - Trả về cho Client/UI
    /// Không chứa thông tin nhạy cảm như Password
    /// </summary>
    public class UserViewModel
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }
        public EGender Gender { get; set; }
        public string GenderName => EnumUtils.GetDescription<EGender>(this.Gender);
        public string? Email { get; set; }
        public string Phone { get; set; }
        public string PositionName { get; set; }
        public string PropertyId { get; set; } = "1";
        public List<int> RoleIdList { get; set; } = new();
        public ETypeAccount TypeAccount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
