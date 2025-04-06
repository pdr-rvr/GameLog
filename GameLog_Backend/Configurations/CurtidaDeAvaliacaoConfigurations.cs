using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class CurtidaDeAvaliacaoConfigurations : IEntityTypeConfiguration<CurtidaDeAvaliacao>
    {
        public void Configure(EntityTypeBuilder<CurtidaDeAvaliacao> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(CurtidaDeAvaliacao)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Curtida)
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
