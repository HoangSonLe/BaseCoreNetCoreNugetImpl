namespace BaseSourceImpl.Presentation.Controllers.Permissions.Models
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionCode { get; set; }
        public string PermissionAction { get; set; }
        public string PermisstionGroup { get; set; }
    }
    public class BasePermission
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class PermissionGroup
    {
        public List<BasePermission> Menus { get; set; }
        public List<BasePermission> Actions { get; set; }
    }
}
