using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class SqlResponseModel<T> : BaseResponseModel
{
    public T Data { get; set; }
    public bool Success { get; set; }
}
