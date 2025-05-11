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
}