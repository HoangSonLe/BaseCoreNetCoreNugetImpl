using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseSourceImpl.Domains.Entities.Permission
{
    public class PermissionEntityConfiguration : IEntityTypeConfiguration<PermissionEntity>
    {
        public void Configure(EntityTypeBuilder<PermissionEntity> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Columns
            builder.Property(p => p.Id).HasColumnName("Id");
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.Code).HasColumnName("Code");
            builder.Property(p => p.Description).HasColumnName("Description");

            // Table
            builder.ToTable("Permission");
        }
    }
}
