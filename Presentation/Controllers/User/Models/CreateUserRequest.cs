using BaseSourceImpl.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Presentation.Controllers.User.Models
{
    /// <summary>
    /// CreateUserRequest - Request model ?? t?o User m?i
    /// Ch?a validation attributes cho API input
    /// </summary>
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

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
