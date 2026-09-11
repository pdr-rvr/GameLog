using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class JogoFavoritoConfigurations : IEntityTypeConfiguration<JogoFavoritoUsuario>
    {
        public void Configure(EntityTypeBuilder<JogoFavoritoUsuario> builder)
        {
            builder.ToTable("JogosFavoritosUsuarios");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .HasColumnName($"{nameof(JogoFavoritoUsuario)}Id")
                .IsRequired();

            builder.Property(f => f.Posicao)
                .IsRequired();

            builder.Property(f => f.EstaAtivo)
                .HasDefaultValue(true);

            // Chave única: um usuário só pode ter um jogo por posição de 1 a 5
            builder.HasIndex(f => new { f.UsuarioId, f.Posicao })
                .IsUnique();

            builder.HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Jogo)
                .WithMany()
                .HasForeignKey(f => f.JogoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
