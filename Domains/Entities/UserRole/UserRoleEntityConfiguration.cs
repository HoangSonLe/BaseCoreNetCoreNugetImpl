using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseSourceImpl.Domains.Entities.UserRole
{
    public class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRoleEntity>
    {
        public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
        {
            // Primary key
            builder.HasKey(p => new { p.RoleId, p.UserId });

            // Columns
            builder.Property(p => p.RoleId).HasColumnName("RoleId");
            builder.Property(p => p.UserId).HasColumnName("UserId");


            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(p => new { p.RoleId, p.UserId }).IsUnique();
            // Relationships
            builder.Ignore(rp => rp.Role);
            builder.Ignore(rp => rp.User);
            // Table
            builder.ToTable("UserRole");
        }
    }
}
