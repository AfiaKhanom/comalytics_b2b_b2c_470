using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure;

/// <summary>
/// Represents AutoMapper configuration for the plugin
/// </summary>
public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<GeofenceZone, GeofenceZoneModel>()
            .ForMember(dest => dest.CoordinatesJson, opt => opt.MapFrom(src => src.CoordinatesJson))
            .ReverseMap();

        CreateMap<GeofenceDeliverySettings, ConfigurationModel>().ReverseMap();
    }

    public int Order => 1;
}
