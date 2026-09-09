using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameLog_Backend.DTOs
{
    public class ItemListaDTO
    {
        public int ItemId { get; set; }
        public int JogoId { get; set; }
        public string TituloJogo { get; set; } = string.Empty;
        public string? ImagemJogo { get; set; }
        public string? NomeEmpresa { get; set; }
        public int? EmpresaId { get; set; }
        public string? DataLancamento { get; set; }
        public double? MediaAvaliacoes { get; set; }
        public int Ordem { get; set; }
        public DateTime DataAdicionado { get; set; }
    }

    public class ListaDeJogosDTO
    {
        public int ListaId { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoPerfilUsuario { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool EstaPublica { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public int TotalJogos { get; set; }
        public List<string> CapasPreview { get; set; } = new List<string>();
        public List<ItemListaDTO> Itens { get; set; } = new List<ItemListaDTO>();
    }

    public class CriarListaDTO
    {
        [Required(ErrorMessage = "O título da coleção é obrigatório.")]
        [MinLength(2, ErrorMessage = "O título deve ter pelo menos 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        public bool EstaPublica { get; set; } = true;
        public List<int>? JogosIds { get; set; } = new List<int>();
    }

    public class EditarListaDTO
    {
        [Required(ErrorMessage = "O título da coleção é obrigatório.")]
        [MinLength(2, ErrorMessage = "O título deve ter pelo menos 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        public bool EstaPublica { get; set; } = true;
        public List<int>? JogosIds { get; set; }
    }

    public class AdicionarJogoListaDTO
    {
        [Required(ErrorMessage = "O jogo é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Identificador de jogo inválido.")]
        public int JogoId { get; set; }
    }
}
