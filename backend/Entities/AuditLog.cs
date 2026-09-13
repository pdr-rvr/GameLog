using System;
using GameLog_Backend.Helpers;

namespace GameLog_Backend.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = UuidV7Helper.NewGuid();
        public string? UsuarioId { get; set; }
        public string Entidade { get; set; } = string.Empty;
        public string EntidadeId { get; set; } = string.Empty;
        public string TipoAcao { get; set; } = string.Empty;
        public string? ValoresAntigos { get; set; }
        public string? ValoresNovos { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? CorrelationId { get; set; }
    }
}
