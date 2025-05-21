using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class AvaliacaoConfigurations : IEntityTypeConfiguration<Avaliacao>
    {
        public void Configure(EntityTypeBuilder<Avaliacao> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(Avaliacao)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.Nota)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Avaliacao_Nota_Range",
                "[Nota] >= 0 AND [Nota] <= 5" 
            ));

            builder.Property(e => e.Nota)
            .IsRequired()
            .HasAnnotation("Range", new[] { 0, 5 });

            builder.HasOne(e => e.Jogo)
                .WithMany()
                .IsRequired();

            builder.Property(p => p.TextoAvaliacao)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.DataPublicacao)
                .IsRequired();

            builder.HasMany(e => e.CurtidasDeAvaliacao)
                .WithOne()
                .IsRequired();

            builder.HasMany(e => e.RespostasDeAvaliacao)
                .WithOne()
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
