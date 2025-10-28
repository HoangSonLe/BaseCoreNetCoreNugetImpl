using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseSourceImpl.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Domains.Entities.User
{
    /// <summary>
    /// User Entity - ??i di?n cho b?ng User trong database
    /// </summary>
    public class UserEntity : BaseSearchableEntity
    {
        [Key]
        public int Id { get; set; }

        [SearchableField]
        public string UserName { get; set; }

        public string Password { get; set; }

        [SearchableField]
        public string Name { get; set; }

        [SearchableField]
        public string? Email { get; set; }

        [SearchableField]
        public string Phone { get; set; }

        public List<int> RoleIdList { get; set; }

        public ETypeAccount TypeAccount { get; set; }
    }
}
