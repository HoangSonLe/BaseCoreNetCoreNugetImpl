using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseSourceImpl.Domains.Entities.Role
{
    public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Columns
            builder.Property(p => p.Id).HasColumnName("Id");
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.Code).HasColumnName("Code");
            builder.Property(p => p.Description).HasColumnName("Description");

            // Table
            builder.ToTable("Roles");
        }
    }

}
