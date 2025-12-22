using BaseSourceImpl.Common.Enums;

namespace BaseSourceImpl.Application.DTOs.User
{
    /// <summary>
    /// UserDto - Sử dụng trong Business Logic Layer (Service)
    /// Chỉ chứa dữ liệu, KHÔNG có validation attributes
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public EGender Gender { get; set; }
        public string PositionName { get; set; }
        public string PropertyId = "1";
        public List<int> RoleIdList { get; set; } = new();
        public ETypeAccount TypeAccount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
    }
}
