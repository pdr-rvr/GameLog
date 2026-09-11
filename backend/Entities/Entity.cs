using System;
using GameLog_Backend.Helpers;

namespace GameLog_Backend.Entities
{
    public class Entity<T> : IAuditable
    {
        public T Id { get; set; } = default!;
        public bool EstaAtivo { get; set; } = true;

        public Entity()
        {
            if (typeof(T) == typeof(Guid))
            {
                Id = (T)(object)UuidV7Helper.NewGuid();
            }
        }
    }
}
