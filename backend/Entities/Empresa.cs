using System;

namespace GameLog_Backend.Entities
{
    public class Empresa : Entity<Guid>
    {
        public string NomeEmpresa { get; set; } = string.Empty;
    }
}
