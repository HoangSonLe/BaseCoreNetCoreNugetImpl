using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseSourceImpl.Domains.Entities.User
{
    public class UserEntityConfigurations : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.UserName).HasColumnName("UserName");
            builder.Property(p => p.Password).HasColumnName("Password");
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.Email).HasColumnName("Email");
            builder.Property(p => p.Phone).HasColumnName("Phone");
            builder.Property(p => p.TypeAccount).HasColumnName("TypeAccount");


            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            // Table
            builder.ToTable("Users");
        }
    }
}
