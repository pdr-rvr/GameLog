using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class UsuarioDTO
    {
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
        public string Email { get; set; }
        public string FotoDePerfil { get; set; }
        public bool EstaAtivo { get; set; }
    }

    public class CriarUsuarioDTO
    {
        public string NomeUsuario { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string FotoDePerfil { get; set; }
    }

    public class UsuarioLoginDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }

	public class EditarUsuarioDTO
	{
		public string NomeUsuario { get; set; }
		public string Email { get; set; }
		public string SenhaAtual { get; set; }
		public string? NovaSenha { get; set; }
		public string? FotoDePerfil { get; set; }
	}

	public class DeletarUsuarioDTO
	{
		public string Senha { get; set; }
	}

    public class GeneroFavoritoDTO
    {
        public string Genero {  get; set; }
    }

    public class JogoRecomendacaoDTO
    {
        public int JogoId { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Imagem { get; set; }
        public DateOnly DataLancamento { get; set; }
        public string GeneroFavorito { get; set; }
    }
}