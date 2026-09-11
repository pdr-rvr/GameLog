using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class JogoDTO
    {
        public Guid JogoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Imagem { get; set; } = string.Empty;
        public DateOnly DataLancamento { get; set; }
        public int ClassificacaoIndicativa { get; set; }
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public Guid? PublicadoraId { get; set; }
        public string? NomePublicadora { get; set; }
        public string? NomeDesenvolvedora => NomeEmpresa;
        public bool EstaAtivo { get; set; }
        public double? MediaAvaliacoes { get; set; }
        public List<string> Generos { get; set; } = new List<string>(); 
        public int TotalAvaliacoes { get; set; }
        public int? RawgId { get; set; }
        public bool EhExterno { get; set; }
    }

    public class MetadadosFiltrosDTO
    {
        public List<string> Generos { get; set; } = new();
        public List<string> Empresas { get; set; } = new();
        public List<int> Anos { get; set; } = new();
    }
}
