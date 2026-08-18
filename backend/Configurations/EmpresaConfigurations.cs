using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class EmpresaConfigurations : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(Empresa)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.NomeEmpresa)
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(p => p.NomeEmpresa)
                .IsUnique();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
