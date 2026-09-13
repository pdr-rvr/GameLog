using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .HasColumnName("RefreshTokenId")
                .IsRequired();

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(rt => rt.Token)
                .IsUnique();

            builder.HasIndex(rt => rt.UsuarioId);

            builder.HasOne(rt => rt.Usuario)
                .WithMany()
                .HasForeignKey(rt => rt.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
