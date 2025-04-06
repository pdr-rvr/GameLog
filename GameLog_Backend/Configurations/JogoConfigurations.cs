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
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Titulo)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(p => p.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.Imagem)
                .HasColumnType("varchar(MAX)")
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

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
