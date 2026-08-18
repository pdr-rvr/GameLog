namespace GameLog_Backend.Entities
{
    public class SegueUsuario : Entity<int>
    {
        public virtual Usuario UsuarioSeguidor { get; set; }
        public virtual Usuario UsuarioSeguido { get; set; }
    }
}
