namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models.Auth;
public class IntegrationResponseModel
{
    public IntegrationResponseModel()
    {
        ErrorList = new List<string>();
    }

    public string Message { get; set; }

    public List<string> ErrorList { get; set; }
}
