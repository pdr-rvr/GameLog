namespace GameLog_Backend.Entities
{
    public interface ISoftDeletable
    {
        public bool EstaAtivo { get; set; }
    }

    public interface IAuditable : ISoftDeletable
    {
        // Interface para entidades com exclusão lógica e rastreabilidade
    }
}
