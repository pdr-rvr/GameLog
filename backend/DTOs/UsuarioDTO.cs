using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class UsuarioDTO
    {
        public Guid UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FotoDePerfil { get; set; }
        public string? Bio { get; set; }
        public bool EstaAtivo { get; set; }
    }

    public class CriarUsuarioDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? FotoDePerfil { get; set; }
        public string? Bio { get; set; }
    }

    public class UsuarioLoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class EditarUsuarioDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaAtual { get; set; } = string.Empty;
        public string? NovaSenha { get; set; }
        public string? FotoDePerfil { get; set; }
        public string? Bio { get; set; }
    }

    public class DeletarUsuarioDTO
    {
        public string Senha { get; set; } = string.Empty;
    }

    public class GeneroFavoritoDTO
    {
        public string Genero { get; set; } = string.Empty;
    }

    public class JogoRecomendacaoDTO
    {
        public Guid JogoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Imagem { get; set; } = string.Empty;
        public DateOnly DataLancamento { get; set; }
        public int ClassificacaoIndicativa { get; set; }
        public string GeneroFavorito { get; set; } = string.Empty;
        public string? NomeEmpresa { get; set; }
        public double? MediaAvaliacoes { get; set; }
        public string? MotivoRecomendacao { get; set; }
        public double Score { get; set; }
    }
}