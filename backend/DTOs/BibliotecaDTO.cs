using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GameLog_Backend.Entities;

namespace GameLog_Backend.DTOs
{
    public class ItemBibliotecaDTO
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid JogoId { get; set; }
        public string TituloJogo { get; set; } = string.Empty;
        public string ImagemJogo { get; set; } = string.Empty;
        public string? NomeEmpresa { get; set; }
        public Guid? EmpresaId { get; set; }
        public string? DataLancamento { get; set; }
        public int Status { get; set; }
        public string StatusNome { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public int? MinhaNota { get; set; }
        public double? MediaAvaliacoes { get; set; }
    }

    public class SalvarItemBibliotecaDTO
    {
        [Required(ErrorMessage = "O jogo é obrigatório.")]
        public Guid JogoId { get; set; }

        [Required(ErrorMessage = "O status do jogo é obrigatório.")]
        [EnumDataType(typeof(StatusJogo), ErrorMessage = "Status do jogo inválido.")]
        public StatusJogo Status { get; set; }
    }

    public class EstatisticasBibliotecaDTO
    {
        public int TotalJogos { get; set; }
        public int TotalQueroJogar { get; set; }
        public int TotalJogando { get; set; }
        public int TotalZerados { get; set; }
        public int TotalPausados { get; set; }
        public int TotalAbandonados { get; set; }
    }

    public class JogoFavoritoDTO
    {
        public int Posicao { get; set; }
        public Guid JogoId { get; set; }
        public string TituloJogo { get; set; } = string.Empty;
        public string ImagemJogo { get; set; } = string.Empty;
        public string? NomeEmpresa { get; set; }
        public Guid? EmpresaId { get; set; }
        public string? DataLancamento { get; set; }
        public double? MediaAvaliacoes { get; set; }
    }

    public class ItemFavoritoPosicaoDTO
    {
        [Required]
        [Range(1, 5, ErrorMessage = "A posição do favorito deve estar entre 1 e 5.")]
        public int Posicao { get; set; }

        [Required(ErrorMessage = "Identificador de jogo inválido.")]
        public Guid JogoId { get; set; }
    }

    public class SalvarJogosFavoritosDTO
    {
        public List<ItemFavoritoPosicaoDTO> Favoritos { get; set; } = new List<ItemFavoritoPosicaoDTO>();
    }
}
