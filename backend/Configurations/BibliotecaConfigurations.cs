using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class BibliotecaConfigurations : IEntityTypeConfiguration<BibliotecaJogo>
    {
        public void Configure(EntityTypeBuilder<BibliotecaJogo> builder)
        {
            builder.ToTable("ItensBiblioteca");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName($"{nameof(BibliotecaJogo)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(b => b.Status)
                .IsRequired();

            builder.Property(b => b.DataAtualizacao)
                .IsRequired();

            builder.Property(b => b.EstaAtivo)
                .HasDefaultValue(true);

            // Chave única: um usuário só tem uma entrada de status por jogo
            builder.HasIndex(b => new { b.UsuarioId, b.JogoId })
                .IsUnique();

            builder.HasOne(b => b.Usuario)
                .WithMany()
                .HasForeignKey(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Jogo)
                .WithMany()
                .HasForeignKey(b => b.JogoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
