using DataInputService.DTO;
using DataInputService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataInputService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantInputController : ControllerBase
    {
        private readonly IPlantService _plantService;

        public PlantInputController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        [HttpPost]
        public async Task<IActionResult> PostPlantData([FromBody] PlantInputModelDTO input)
        {
            await _plantService.ProcessAndSendAsync(input);
            return Ok();
        }
    }
}
