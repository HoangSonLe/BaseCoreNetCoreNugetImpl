using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Utils;
using BaseSourceImpl.Domains.Entities.RefreshToken;
using BaseSourceImpl.Domains.Entities.User;
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
            modelBuilder.Entity<UserEntity>().HasData(
                new UserEntity()
                {
                    Id = 1,
                    UserName = "Admin",
                    Password = "$2a$10$eEuboxhND2XMLkDzekp51eTVeeLhXWPkHbF5hOFyCif9nnINow9/G",//123456
                    Name = "Admin",
                    Phone = "",
                    TypeAccount = Common.Enums.ETypeAccount.ADMIN,
                    RoleIdList = new List<int>() { 1, 2, 3, 4 },
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
                    RoleIdList = new List<int>() { 1, 2, 3, 4 },
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = EState.Active,
                    Email = "",
                    NonUnicodeSearchString = SearchFieldUtils.NormalizeSearchText("Dev")
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
