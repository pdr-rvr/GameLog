using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class AuditLogConfigurations : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName($"{nameof(AuditLog)}Id")
                .IsRequired();

            builder.Property(x => x.Entidade)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.EntidadeId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.TipoAcao)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.UsuarioId)
                .HasMaxLength(100);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);

            builder.Property(x => x.TimestampUtc)
                .IsRequired();

            builder.Property(x => x.ValoresAntigos)
                .HasColumnType("text");

            builder.Property(x => x.ValoresNovos)
                .HasColumnType("text");

            builder.HasIndex(x => x.TimestampUtc);
            builder.HasIndex(x => new { x.Entidade, x.EntidadeId });
            builder.HasIndex(x => x.UsuarioId);
        }
    }
}
