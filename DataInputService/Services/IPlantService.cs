using DataInputService.DTO;

namespace DataInputService.Services
{
    public interface IPlantService
    {
        Task ProcessAndSendAsync(PlantInputModelDTO model);
    }
}
