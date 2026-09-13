using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IBuscaGlobalService
    {
        Task<BuscaGlobalDTO> BuscarAsync(string? q, int limite = 5);
    }
}
