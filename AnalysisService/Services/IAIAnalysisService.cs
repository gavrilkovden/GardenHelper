using AnalysisService.Models;

namespace AnalysisService.Services
{
    public interface IAIAnalysisService
    {
        Task<AnalysisResultDto> AnalyzeAsync(AnalysisRequest request);
    }
}
