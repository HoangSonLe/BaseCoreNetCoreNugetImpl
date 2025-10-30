using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Presentation.Controllers.User.Models
{
    /// <summary>
    /// ChangePasswordRequest - Request model ?? ??i password
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
