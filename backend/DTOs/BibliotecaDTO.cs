using System;
using System.Collections.Generic;
using GameLog_Backend.Entities;

namespace GameLog_Backend.DTOs
{
    public class ItemBibliotecaDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public string TituloJogo { get; set; }
        public string ImagemJogo { get; set; }
        public string? NomeEmpresa { get; set; }
        public int? EmpresaId { get; set; }
        public string? DataLancamento { get; set; }
        public int Status { get; set; }
        public string StatusNome { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public int? MinhaNota { get; set; }
        public double? MediaAvaliacoes { get; set; }
    }

    public class SalvarItemBibliotecaDTO
    {
        public int JogoId { get; set; }
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
        public int JogoId { get; set; }
        public string TituloJogo { get; set; }
        public string ImagemJogo { get; set; }
        public string? NomeEmpresa { get; set; }
        public int? EmpresaId { get; set; }
        public string? DataLancamento { get; set; }
        public double? MediaAvaliacoes { get; set; }
    }

    public class ItemFavoritoPosicaoDTO
    {
        public int Posicao { get; set; }
        public int JogoId { get; set; }
    }

    public class SalvarJogosFavoritosDTO
    {
        public List<ItemFavoritoPosicaoDTO> Favoritos { get; set; } = new List<ItemFavoritoPosicaoDTO>();
    }
}
