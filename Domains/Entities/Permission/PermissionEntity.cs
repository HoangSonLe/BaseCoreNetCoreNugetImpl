using BaseSourceImpl.Domains.Entities.RolePermisson;

namespace BaseSourceImpl.Domains.Entities.Permission
{
    public class PermissionEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    }
}
