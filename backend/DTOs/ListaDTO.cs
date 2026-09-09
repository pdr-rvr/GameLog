using System;
using System.Collections.Generic;

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
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool EstaPublica { get; set; } = true;
        public List<int>? JogosIds { get; set; } = new List<int>();
    }

    public class EditarListaDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool EstaPublica { get; set; } = true;
        public List<int>? JogosIds { get; set; }
    }

    public class AdicionarJogoListaDTO
    {
        public int JogoId { get; set; }
    }
}
