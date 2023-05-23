using AutoMapper;
using BP.Data.DbModels;
using BP.Data.Dto.Response;

namespace BP.Infrastructure;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Sensor, ModuleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Module.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Module.Name))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Module.Location));
        
        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
    }
}