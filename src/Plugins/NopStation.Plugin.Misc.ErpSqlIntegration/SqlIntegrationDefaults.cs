namespace NopStation.Plugin.Misc.ErpSqlIntegration;
public static class SqlIntegrationDefaults
{
    public static string HideGeneralBlock => "SqlIntegrationPage.HideGeneralBlock";
    public static int DefaultTimeOutPeriod => 1800;
    public static int AccountNoLengthLimit => 20;

    private static readonly HashSet<string> _validPublishedValues =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "yes", "y", "1", "true"
            };

    public static bool IsPublished(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return _validPublishedValues.Contains(value.Trim());
    }

    #region JWT

    public static readonly string Token = "Authorization";
    public static readonly string SecretKey = "SecretKey";
    public static readonly string CustomerId = "CustomerId";

    #endregion
}
