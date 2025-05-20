using AutoMapper;
using DataInputService.Domain.Entities;
using DataInputService.DTO;

namespace DataInputService.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // DTO -> Entity
            CreateMap<PlantInputModelDTO, Plant>();

            // Entity -> DTO (если нужно)
            CreateMap<Plant, PlantInputModelDTO>();
        }
    }
}
