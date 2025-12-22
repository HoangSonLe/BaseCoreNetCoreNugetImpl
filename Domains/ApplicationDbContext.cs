using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Utils;
using BaseSourceImpl.Domains.Entities.Permission;
using BaseSourceImpl.Domains.Entities.RefreshToken;
using BaseSourceImpl.Domains.Entities.Role;
using BaseSourceImpl.Domains.Entities.RolePermisson;
using BaseSourceImpl.Domains.Entities.User;
using BaseSourceImpl.Domains.Entities.UserRole;
using Microsoft.EntityFrameworkCore;

namespace BaseSourceImpl.Domains
{

    public class ApplicationDbContext : PostgresDBContext
    {
        public ApplicationDbContext(DbContextOptions options)
        : base(options)
        {
        }
        public virtual DbSet<UserEntity> Users { get; set; }
        public virtual DbSet<RefreshTokenEntity> RefreshTokenEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new UserEntityConfigurations());
            modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfigurations());
            modelBuilder.ApplyConfiguration(new RoleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionEntityConfiguration());



            modelBuilder.Entity<UserEntity>().HasData(
                new UserEntity()
                {
                    Id = 1,
                    UserName = "Admin",
                    Password = "$2a$10$eEuboxhND2XMLkDzekp51eTVeeLhXWPkHbF5hOFyCif9nnINow9/G",//123456
                    Name = "Admin",
                    Phone = "",
                    TypeAccount = Common.Enums.ETypeAccount.ADMIN,
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = EState.Active,
                    Email = "",
                    NonUnicodeSearchString = SearchFieldUtils.NormalizeSearchText("Admin")
                },
                new UserEntity()
                {
                    Id = 2,
                    UserName = "Dev",
                    Password = "$2a$10$eEuboxhND2XMLkDzekp51eTVeeLhXWPkHbF5hOFyCif9nnINow9/G",//123456
                    Name = "Dev",
                    Phone = "",
                    TypeAccount = Common.Enums.ETypeAccount.DEV,
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = EState.Active,
                    Email = "",
                    NonUnicodeSearchString = SearchFieldUtils.NormalizeSearchText("Dev")
                }
            );
            modelBuilder.Entity<RoleEntity>().HasData(
                new RoleEntity()
                {
                    Id = 1,
                    Name = "Administrator",
                    Code = "ADMIN",
                    Description = "Role with full permissions",
                },
                new RoleEntity()
                {
                    Id = 2,
                    Name = "Developer",
                    Code = "DEV",
                    Description = "Role for development purposes",
                }
            );
            modelBuilder.Entity<UserRoleEntity>().HasData(
                new UserRoleEntity()
                {
                    UserId = 1,
                    RoleId = 1,
                },
                new UserRoleEntity()
                {
                    UserId = 2,
                    RoleId = 2,
                }
            );
            modelBuilder.Entity<PermissionEntity>().HasData(
                new PermissionEntity()
                {
                    Id = 1,
                    Name = "Read User",
                    Code = "USER_READ",
                    Description = "Read User",
                },
                new PermissionEntity()
                {
                    Id = 2,
                    Name = "Write User",
                    Code = "USER_WRITE",
                    Description = "Write User",
                },
                new PermissionEntity()
                {
                    Id = 3,
                    Name = "Delete User",
                    Code = "USER_DELETE",
                    Description = "Delete User",
                },
                new PermissionEntity()
                {
                    Id = 4,
                    Name = "Read Permission",
                    Code = "PERMISSION_READ",
                    Description = "Read Permission",
                }
            );
            modelBuilder.Entity<RolePermissionEntity>().HasData(
                new RolePermissionEntity()
                {
                    RoleId = 1,
                    PermissionId = 1,
                },
                new RolePermissionEntity()
                {
                    RoleId = 1,
                    PermissionId = 2,
                },
                new RolePermissionEntity()
                {
                    RoleId = 1,
                    PermissionId = 3,
                },
                new RolePermissionEntity()
                {
                    RoleId = 2,
                    PermissionId = 1,
                }
            );



            base.OnModelCreating(modelBuilder);
        }

        // Override SaveChanges to auto-generate search strings
        public override int SaveChanges()
        {
            GenerateSearchStrings();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GenerateSearchStrings();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void GenerateSearchStrings()
        {
            var entries = ChangeTracker.Entries<ISearchableEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.GenerateSearchString();
            }
        }

    }
}
