using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class UsuarioConexaoDTO
    {
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? FotoPerfil { get; set; }
        public string? Bio { get; set; }
        public bool SeguidoPorMim { get; set; }
    }

    public class EstatisticasSociaisDTO
    {
        public int TotalSeguidores { get; set; }
        public int TotalSeguindo { get; set; }
        public bool SeguidoPorMim { get; set; }
    }

    public class StatusSeguirDTO
    {
        public bool Seguido { get; set; }
        public int TotalSeguidores { get; set; }
    }

    public class ItemFeedSocialDTO
    {
        public string Id { get; set; } = string.Empty;
        public string TipoAtividade { get; set; } = string.Empty; // "Avaliacao", "JogoZerado", "ListaCriada"
        public DateTime DataAtividade { get; set; }
        
        // Dados do Autor
        public int AutorId { get; set; }
        public string AutorNome { get; set; } = string.Empty;
        public string? AutorFoto { get; set; }

        // Dados do Jogo (se aplicavel)
        public int? JogoId { get; set; }
        public string? JogoTitulo { get; set; }
        public string? JogoImagem { get; set; }
        public string? NomeEmpresa { get; set; }

        // Dados da Avaliacao (se aplicavel)
        public int? AvaliacaoId { get; set; }
        public int? Nota { get; set; }
        public string? TextoAvaliacao { get; set; }
        public int TotalCurtidas { get; set; }
        public bool CurtidaPorMim { get; set; }
        public int TotalRespostas { get; set; }

        // Dados da Lista/Colecao (se aplicavel)
        public int? ListaId { get; set; }
        public string? ListaTitulo { get; set; }
        public string? ListaDescricao { get; set; }
        public int? TotalJogosLista { get; set; }
        public List<string>? CapasPreviewLista { get; set; }
    }
}
