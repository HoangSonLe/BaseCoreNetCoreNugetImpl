using BaseSourceImpl.Domains.Entities.RolePermisson;
using BaseSourceImpl.Domains.Entities.UserRole;

namespace BaseSourceImpl.Domains.Entities.Role
{
    public class RoleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
        public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    }
}
