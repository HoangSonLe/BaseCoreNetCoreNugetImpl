using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseSourceImpl.Domains.Entities.Role;
using BaseSourceImpl.Domains.Entities.User;

namespace BaseSourceImpl.Domains.Entities.UserRole
{
    public class UserRoleEntity : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation (không có ràng buộc FK thật)
        public UserEntity? User { get; set; }
        public RoleEntity? Role { get; set; }
    }
}
