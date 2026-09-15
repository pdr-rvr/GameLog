using System.Threading;
using System.Threading.Tasks;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IAuditRetentionService
    {
        Task<int> ExpurgaLogsAntigosAsync(int diasRetencao = 90, CancellationToken cancellationToken = default);
    }
}
