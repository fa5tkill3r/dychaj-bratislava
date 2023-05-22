using AutoMapper;
using BP.Data.DbModels;
using BP.Data.Dto.Response;

namespace BP.Infrastructure;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Location, LocationDto>();
    }
}