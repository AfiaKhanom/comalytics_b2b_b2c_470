using AutoMapper;
using System.Text.Json;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<GeofenceDeliverySettings, ConfigurationModel>().ReverseMap();

        CreateMap<GeofenceZone, GeofenceZoneModel>()
            .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => DeserializeCoordinates(src.CoordinatesJson)))
            .ForMember(dest => dest.CoordinatesJson, opt => opt.MapFrom(src => src.CoordinatesJson))
            .ReverseMap()
            .ForMember(dest => dest.CoordinatesJson, opt => opt.MapFrom(src => SerializeCoordinates(src.Coordinates)));
    }

    public int Order => 1;

    private static List<CoordinateDto> DeserializeCoordinates(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<CoordinateDto>();
        try
        {
            return JsonSerializer.Deserialize<List<CoordinateDto>>(json) ?? new List<CoordinateDto>();
        }
        catch (JsonException)
        {
            // Return empty list when coordinates JSON is malformed; this is handled gracefully downstream
            return new List<CoordinateDto>();
        }
    }

    private static string SerializeCoordinates(IList<CoordinateDto> coordinates)
    {
        if (coordinates == null || coordinates.Count == 0)
            return "[]";
        return JsonSerializer.Serialize(coordinates, new JsonSerializerOptions { WriteIndented = false });
    }
}
