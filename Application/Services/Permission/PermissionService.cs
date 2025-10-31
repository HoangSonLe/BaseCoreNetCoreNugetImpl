using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseSourceImpl.Domains.Entities.Permission;
using BaseSourceImpl.Domains.Entities.RolePermisson;
using BaseSourceImpl.Domains.Entities.UserRole;
using Microsoft.EntityFrameworkCore;

namespace BaseSourceImpl.Application.Services.Permission
{
    public class PermissionService : IPermissionService
    {
        private readonly PostgresDBContext _db;

        public PermissionService(PostgresDBContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<string>> GetPermissionsByUserIdAsync(string userId)
        {
            if (!int.TryParse(userId, out var uid))
                return Array.Empty<string>();

            // Single SQL JOIN across UserRole -> RolePermission -> Permission
            var codes = await (from ur in _db.Set<UserRoleEntity>()
                               join rp in _db.Set<RolePermissionEntity>() on ur.RoleId equals rp.RoleId
                               join p in _db.Set<PermissionEntity>() on rp.PermissionId equals p.Id
                               where ur.UserId == uid && rp.State == EState.Active && p.Code != null
                               select p.Code!)
                              .Distinct()
                              .ToListAsync();

            return codes;
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            if (!int.TryParse(userId, out var uid) || string.IsNullOrWhiteSpace(permission))
                return false;

            permission = permission.Trim();

            var exists = await (from ur in _db.Set<UserRoleEntity>()
                                join rp in _db.Set<RolePermissionEntity>() on ur.RoleId equals rp.RoleId
                                join p in _db.Set<PermissionEntity>() on rp.PermissionId equals p.Id
                                where ur.UserId == uid
                                      && rp.State == EState.Active
                                      && p.Code != null
                                      && EF.Functions.ILike(p.Code, permission) // case-insensitive on Postgres
                                select p.Id)
                               .AnyAsync();

            return exists;
        }
    }
}
