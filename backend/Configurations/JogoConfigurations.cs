using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class JogoConfigurations : IEntityTypeConfiguration<Jogo>
    {
        public void Configure(EntityTypeBuilder<Jogo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(Jogo)}Id")
                .IsRequired();

            builder.Property(p => p.Titulo)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(p => p.Descricao)
                .IsRequired(false);

            builder.Property(p => p.Imagem)
                .IsRequired(false);

            builder.Property(p => p.DataLancamento)
                .IsRequired();

            builder.HasMany(g => g.Generos)
                .WithMany(g => g.Jogos)
                .UsingEntity(j => j.ToTable("JogoGenero"));


            builder.Property(p => p.ClassificacaoIndicativa)
                .IsRequired();

            builder.HasOne(e => e.Empresa)
                .WithMany()
                .IsRequired();

            builder.HasOne(e => e.Publicadora)
                .WithMany()
                .IsRequired(false);

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
