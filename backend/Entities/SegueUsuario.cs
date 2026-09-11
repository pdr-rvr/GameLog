using System;

namespace GameLog_Backend.Entities
{
    public class SegueUsuario : Entity<Guid>
    {
        public virtual Usuario UsuarioSeguidor { get; set; } = null!;
        public virtual Usuario UsuarioSeguido { get; set; } = null!;
    }
}
