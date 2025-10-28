using BaseSourceImpl.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Presentation.Models.Requests
{
    /// <summary>
    /// UpdateUserRequest - Request model ?? c?p nh?t User
    /// Ch?a validation attributes cho API input
    /// </summary>
    public class UpdateUserRequest
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }

        public List<int> RoleIdList { get; set; } = new();

        public ETypeAccount TypeAccount { get; set; }
    }
}
