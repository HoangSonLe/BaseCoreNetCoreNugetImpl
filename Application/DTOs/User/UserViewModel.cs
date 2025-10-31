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

        public string? Email { get; set; }

        public string Phone { get; set; }

        public List<int> RoleIdList { get; set; } = new();

        public ETypeAccount TypeAccount { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
