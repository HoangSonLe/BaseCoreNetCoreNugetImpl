using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseSourceImpl.Domains.Entities.Permission;
using BaseSourceImpl.Domains.Entities.Role;

namespace BaseSourceImpl.Domains.Entities.RolePermisson
{
    public class RolePermissionEntity : BaseAuditableEntity
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public RoleEntity? Role { get; set; }
        public PermissionEntity? Permission { get; set; }
    }
}
