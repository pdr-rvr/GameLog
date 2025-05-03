using GameLog_Backend.Entities;

namespace GameLog_Backend.DTOs
{
    public class JogoDTO
    {
        public int JogoId {  get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Imagem { get; set; }
        public DateOnly DataLancamento { get; set; }
        public int ClassificacaoIndicativa { get; set; }
        public int EmpresaId { get; set; }
        public bool EstaAtivo { get; set; }
    }
}
