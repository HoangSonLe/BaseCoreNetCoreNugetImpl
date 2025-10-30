using BaseNetCore.Core.src.Main.Common.Models;

namespace BaseSourceImpl.Presentation.Controllers.User.Models
{
    public class UserSearchModel : PageRequest
    {
        public string? SearchText { get; set; }
    }
}
