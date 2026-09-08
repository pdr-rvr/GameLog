namespace GameLog_Backend.DTOs
{
    public class EmpresaDTO
    {
        public int EmpresaId { get; set; }
        public string NomeEmpresa { get; set; }
        public bool EstaAtivo { get; set; }
        public int TotalJogos { get; set; }
        public double? MediaNotasJogos { get; set; }
    }
}
