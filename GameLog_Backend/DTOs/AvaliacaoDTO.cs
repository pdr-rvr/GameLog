namespace GameLog_Backend.DTOs
{
    public class AvaliacaoDTO
    {
        public int AvaliacaoId { get; set; }
        public int Nota { get; set; }
        public int JogoId { get; set; }
        public string TextoAvaliacao { get; set; }
        public string NomeJogo { get; set; } 
        public string NomeUsuario { get; set; }
        public DateTime DataPublicacao { get; set; }
        public int TotalCurtidas { get; set; }
    }

    public class CriarAvaliacaoDTO
    {
        public int Nota { get; set; }
        public int JogoId { get; set; }
        public string TextoAvaliacao { get; set; }
    }

    public class EditarAvaliacaoDTO
    {
        public int? Nota { get; set; }
        public string? TextoAvaliacao { get; set; }
    }
}