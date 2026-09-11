using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class BuscaItemJogoDTO
    {
        public Guid JogoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Imagem { get; set; }
        public int? AnoLancamento { get; set; }
        public string? NomeEmpresa { get; set; }
        public List<string> Generos { get; set; } = new();
        public double? MediaAvaliacoes { get; set; }
        public int? RawgId { get; set; }
        public bool EhExterno { get; set; }
    }

    public class BuscaItemUsuarioDTO
    {
        public Guid UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoDePerfil { get; set; }
        public string? Bio { get; set; }
    }

    public class BuscaItemListaDTO
    {
        public Guid ListaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public Guid UsuarioId { get; set; }
        public string NomeCriador { get; set; } = string.Empty;
        public int TotalJogos { get; set; }
        public List<string> CapasPreview { get; set; } = new();
    }

    public class BuscaGlobalDTO
    {
        public string Termo { get; set; } = string.Empty;
        public List<BuscaItemJogoDTO> Jogos { get; set; } = new();
        public List<BuscaItemUsuarioDTO> Usuarios { get; set; } = new();
        public List<BuscaItemListaDTO> Listas { get; set; } = new();
        public int TotalResultados => Jogos.Count + Usuarios.Count + Listas.Count;
    }
}
