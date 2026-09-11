using System;

namespace GameLog_Backend.DTOs
{
    public class EmpresaDTO
    {
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public bool EstaAtivo { get; set; }
        public int TotalJogos { get; set; }
        public double? MediaNotasJogos { get; set; }
    }
}
