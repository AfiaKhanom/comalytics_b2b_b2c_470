namespace NopStation.Plugin.Misc.ErpSqlIntegration;
public static class SqlIntegrationDefaults
{
    public static string HideGeneralBlock => "SqlIntegrationPage.HideGeneralBlock";
    public static int DefaultTimeOutPeriod => 1800;
    public static int AccountNoLengthLimit => 20;
    public static string SqlProductPublishedStatus => "Yes";

    #region JWT

    public static readonly string Token = "Authorization";
    public static readonly string SecretKey = "SecretKey";
    public static readonly string CustomerId = "CustomerId";

    #endregion
}
