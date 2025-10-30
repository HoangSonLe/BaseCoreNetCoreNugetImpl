using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BaseSourceImpl.Domains.Entities.RefreshToken
{
    public class RefreshTokenEntityConfigurations : IEntityTypeConfiguration<RefreshTokenEntity>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Columns
            builder.Property(p => p.Id).HasColumnName("Id");
            builder.Property(p => p.Token).HasColumnName("Token").IsRequired();
            builder.Property(p => p.SessionId).HasColumnName("SessionId").IsRequired();
            builder.Property(p => p.UserId).HasColumnName("UserId").IsRequired();
            builder.Property(p => p.IsValid).HasColumnName("IsValid").HasDefaultValue(true);
            builder.Property(p => p.ExpiresAt).HasColumnName("ExpiresAt");

            // Indexes for fast lookup
            builder.HasIndex(p => new { p.UserId, p.IsValid });
            builder.HasIndex(p => p.SessionId);

            // Table
            builder.ToTable("RefreshToken");
        }
    }
}
