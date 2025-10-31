using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseSourceImpl.Domains.Entities.RolePermisson
{
    public class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
    {
        public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            // Primary key
            builder.HasKey(p => new { p.RoleId, p.PermissionId });

            // Columns
            builder.Property(p => p.RoleId).HasColumnName("RoleId");
            builder.Property(p => p.PermissionId).HasColumnName("PermissionId");

            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(p => new { p.RoleId, p.PermissionId }).IsUnique();

            builder.Ignore(rp => rp.Role);
            builder.Ignore(rp => rp.Permission);
            // Table
            builder.ToTable("RolePermission");
        }
    }
}
