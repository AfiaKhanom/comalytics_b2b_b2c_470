using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Settings;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<ErpOrderSettingsModel, ErpPlaceOrderSettings>().ReverseMap();
        CreateMap<ErpOrderItemDataSettingsModel, ErpPlaceOrderItemSettings>().ReverseMap();
    }

    public int Order => 1;
}