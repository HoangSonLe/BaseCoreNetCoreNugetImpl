using BaseNetCore.Core.src.Main.Common.Models;

namespace BaseSourceImpl.Presentation.Models.Requests
{
    public class UserSearchModel : PageRequest
    {
        public string? SearchText { get; set; }
    }
}
