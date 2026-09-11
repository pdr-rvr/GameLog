using System;

namespace GameLog_Backend.Entities
{
    public class Usuario : Entity<Guid>
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? FotoDePerfil { get; set; }
        public string? Bio { get; set; }
    }
}
