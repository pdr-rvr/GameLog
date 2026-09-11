using System;
using System.ComponentModel.DataAnnotations;

namespace GameLog_Backend.DTOs
{
    public class AvaliacaoDTO
    {
        public Guid AvaliacaoId { get; set; }
        public int Nota { get; set; }
        public Guid JogoId { get; set; }
        public string TextoAvaliacao { get; set; } = string.Empty;
        public string NomeJogo { get; set; } = string.Empty;
        public string? ImagemJogo { get; set; }
        public string? NomeEmpresa { get; set; }
        public Guid? EmpresaId { get; set; }
        public DateOnly? DataLancamentoJogo { get; set; }
        public Guid UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoPerfilUsuario { get; set; }
        public DateTime DataPublicacao { get; set; }
        public int TotalCurtidas { get; set; }
        public bool CurtidaPorMim { get; set; }
        public int TotalRespostas { get; set; }
    }

    public class CriarAvaliacaoDTO
    {
        [Required(ErrorMessage = "A nota é obrigatória.")]
        [Range(1, 5, ErrorMessage = "A nota da avaliação deve estar entre 1 e 5 estrelas.")]
        public int Nota { get; set; }

        [Required(ErrorMessage = "O jogo é obrigatório.")]
        public Guid JogoId { get; set; }

        [MaxLength(1000, ErrorMessage = "A análise pode ter no máximo 1000 caracteres.")]
        public string TextoAvaliacao { get; set; } = string.Empty;
    }

    public class EditarAvaliacaoDTO
    {
        [Required(ErrorMessage = "A nota é obrigatória.")]
        [Range(1, 5, ErrorMessage = "A nota da avaliação deve estar entre 1 e 5 estrelas.")]
        public int Nota { get; set; }

        [MaxLength(1000, ErrorMessage = "A análise pode ter no máximo 1000 caracteres.")]
        public string? TextoAvaliacao { get; set; }
    }

    public class RespostaDeAvaliacaoDTO
    {
        public Guid RespostaId { get; set; }
        public Guid AvaliacaoId { get; set; }
        public Guid UsuarioId { get; set; }
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
        [Required(ErrorMessage = "O comentário não pode ser vazio.")]
        [MaxLength(500, ErrorMessage = "O comentário pode ter no máximo 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;
    }
}