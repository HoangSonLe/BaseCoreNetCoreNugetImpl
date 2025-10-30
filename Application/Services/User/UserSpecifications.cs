using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.DAL.Models.Specification;
using BaseNetCore.Core.src.Main.Utils;
using BaseSourceImpl.Domains.Entities.User;

namespace BaseSourceImpl.Application.Services.User
{
    public static class AuthSpecifications
    {
        /// <summary>
        /// Get user by ID
        /// </summary>
        public static BaseSpecification<UserEntity> ById(int userId)
        {
            return new BaseSpecification<UserEntity>().WithCriteria(u => u.Id == userId).WithTracking(false);
        }
        /// <summary>
        /// Search active users with pagination
        /// </summary>
        public static BaseSpecification<UserEntity> PagingSpecification(string searchText, int pageNumber, int pageSize)
        {
            // Normalize the search keyword (remove diacritics, lowercase)
            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(searchText);
            return new BaseSpecification<UserEntity>()
                .WithCriteria(u => u.State == EState.Active &&
                    u.NonUnicodeSearchString.Contains(normalizedKeyword))
                .WithTracking(false)
                .WithPagedResults(pageNumber, pageSize);
        }

    }
}
