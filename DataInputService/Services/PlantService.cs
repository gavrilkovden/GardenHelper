using DataInputService.Domain.Entities;
using DataInputService.Domain;
using AutoMapper;
using DataInputService.DTO;

namespace DataInputService.Services
{
    public class PlantService : IPlantService
    {
        private readonly IMapper _mapper;
        private readonly PlantDbContext _context;
        private readonly IRabbitMqPublisher _publisher;

        public PlantService(IMapper mapper, PlantDbContext context, IRabbitMqPublisher publisher)
        {
            _mapper = mapper;
            _context = context;
            _publisher = publisher;
        }

        public async Task ProcessAndSendAsync(PlantInputModelDTO model)
        {
            var plant = _mapper.Map<Plant>(model);

            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();

            await _publisher.PublishAsync(model); // отправляем в RabbitMQ
        }
    }
}
