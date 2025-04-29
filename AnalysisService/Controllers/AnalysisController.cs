using AnalysisService.Models;
using AnalysisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AIAnalysisService _analysisService;

        public AnalysisController(AIAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeAsync([FromBody] AnalysisRequest request)
        {
            var result = await _analysisService.AnalyzeAsync(request);
            return Ok(result);
        }
    }
}
