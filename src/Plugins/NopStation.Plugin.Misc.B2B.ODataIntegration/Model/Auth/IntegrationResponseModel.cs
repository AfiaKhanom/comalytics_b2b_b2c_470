namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Model.Auth;
public class IntegrationResponseModel
{
    public IntegrationResponseModel()
    {
        ErrorList = new List<string>();
    }

    public string Message { get; set; }

    public List<string> ErrorList { get; set; }
}
