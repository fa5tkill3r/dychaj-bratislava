﻿using AutoMapper;
using BP.Data.DbHelpers;
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
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude));

        CreateMap<Module, ModuleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

        CreateMap<Reading, ReadingDto>()
            .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => ConvertDateTime(src.DateTime)))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
        
        CreateMap<Sensor, SensorWithReadingsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GetName()))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Module.Location))
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Readings, opt => opt.Ignore());

        CreateMap<Sensor, SensorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GetName()))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Module.Location))
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
    }

    private DateTimeOffset ConvertDateTime(DateTime dateTime)
    {
        var cetOffset = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time").GetUtcOffset(dateTime);

        var result = new DateTimeOffset(dateTime);

        return result;
    }
}