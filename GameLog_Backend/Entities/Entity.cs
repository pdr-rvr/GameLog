namespace GameLog_Backend.Entities
{
    public class Entity<T> : IAuditable
    {
        public T Id { get; set; }
        public bool EstaAtivo { get; set; }
    }
}
