using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class UsuarioConfigurations : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(Usuario)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.NomeUsuario)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(p => p.NomeUsuario)
                .IsUnique();

            builder.Property(p => p.Email)
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(p => p.Email)
                .IsUnique();

            builder.Property(p => p.Senha)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(p => p.FotoDePerfil)
                .HasColumnType("varchar(MAX)")
                .IsRequired(false);

            builder.HasMany(e => e.Avaliacoes)
                .WithOne()
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
