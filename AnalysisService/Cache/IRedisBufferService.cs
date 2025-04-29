using AnalysisService.Models;

namespace AnalysisService.Cache
{
    public interface IRedisBufferService
    {
        Task DeleteRequestAsync(string userId);
        Task<AnalysisRequest?> GetRequestAsync(string userId);
        Task SaveRequestAsync(string userId, AnalysisRequest request);
    }
}