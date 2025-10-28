using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Utils;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new UserEntityConfigurations());
            modelBuilder.Entity<UserEntity>().HasData(
                new UserEntity()
                {
                    Id = 1,
                    UserName = "Admin",
                    Password = "/cA7ZZQqtyOGVwe1kEbPSg==", //123456
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
                    Password = "/cA7ZZQqtyOGVwe1kEbPSg==", //123456
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
