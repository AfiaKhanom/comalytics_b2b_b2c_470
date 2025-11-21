namespace NopStation.Plugin.Misc.B2B.ODataIntegration
{
    public static class D365IntegrationDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "NopStation.Plugin.Misc.B2B.ODataIntegration";

        /// <summary>
        /// Gets the default timeout period in seconds
        /// </summary>
        public static int DefaultTimeOutPeriod => 100;

        /// <summary>
        /// Gets the OAuth token type
        /// </summary>
        public static string TokenType => "Bearer";

        /// <summary>
        /// Gets the OAuth grant type
        /// </summary>
        public static string GrantType => "client_credentials";

        #region JWT

        public static readonly string Token = "Authorization";
        public static readonly string SecretKey = "SecretKey";
        public static readonly string CustomerId = "CustomerId";

        #endregion
    }
}