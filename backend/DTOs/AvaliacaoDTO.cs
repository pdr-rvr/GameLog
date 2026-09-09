namespace GameLog_Backend.DTOs
{
    public class AvaliacaoDTO
    {
        public int AvaliacaoId { get; set; }
        public int Nota { get; set; }
        public int JogoId { get; set; }
        public string TextoAvaliacao { get; set; } = string.Empty;
        public string NomeJogo { get; set; } = string.Empty;
        public string? ImagemJogo { get; set; }
        public string? NomeEmpresa { get; set; }
        public int? EmpresaId { get; set; }
        public DateOnly? DataLancamentoJogo { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoPerfilUsuario { get; set; }
        public DateTime DataPublicacao { get; set; }
        public int TotalCurtidas { get; set; }
        public bool CurtidaPorMim { get; set; }
        public int TotalRespostas { get; set; }
    }

    public class CriarAvaliacaoDTO
    {
        public int Nota { get; set; }
        public int JogoId { get; set; }
        public string TextoAvaliacao { get; set; } = string.Empty;
    }

    public class EditarAvaliacaoDTO
    {
        public int? Nota { get; set; }
        public string? TextoAvaliacao { get; set; }
    }

    public class RespostaDeAvaliacaoDTO
    {
        public int RespostaId { get; set; }
        public int AvaliacaoId { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoPerfilUsuario { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public bool MinhaResposta { get; set; }
        public int TotalCurtidas { get; set; }
        public bool CurtidaPorMim { get; set; }
    }

    public class CriarRespostaDTO
    {
        public string Comentario { get; set; } = string.Empty;
    }
}