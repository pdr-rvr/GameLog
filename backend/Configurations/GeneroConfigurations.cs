using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class GeneroConfigurations : IEntityTypeConfiguration<Genero>
    {
        public void Configure(EntityTypeBuilder<Genero> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(Genero)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.TituloGenero)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(p => p.TituloGenero)
                .IsUnique();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
