using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class RespostaDeAvaliacaoConfigurations : IEntityTypeConfiguration<RespostaDeAvaliacao>
    {
        public void Configure(EntityTypeBuilder<RespostaDeAvaliacao> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(RespostaDeAvaliacao)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Comentario)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
