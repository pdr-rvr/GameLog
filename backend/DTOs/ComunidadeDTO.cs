using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class TendenciasComunidadeDTO
    {
        public List<JogoDTO> JogosMaisDiscutidos { get; set; } = new();
        public List<AvaliacaoDTO> AvaliacoesMaisCurtidas { get; set; } = new();
        public List<ListaDeJogosDTO> ListasEmDestaque { get; set; } = new();
        public int TotalAvaliacoesPlataforma { get; set; }
        public int TotalJogadoresAtivos { get; set; }
    }
}
