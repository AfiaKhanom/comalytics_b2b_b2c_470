using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.ErpSqlIntegration.Enums;
using NopStation.Plugin.Misc.ErpSqlIntegration.Extensions;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models.Auth;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Filters;

public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    #region Ctor

    public TokenAuthorizeAttribute()
        : base(typeof(TokenAuthorizeAttributeFilter)) { }

    #endregion

    #region Nested class

    public class TokenAuthorizeAttributeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext actionContext)
        {
            var result = ParseAuthorizationHeader(actionContext);
            if (result != JWTAuthResult.Success)
            {
                Challenge(actionContext, result);
                return;
            }
        }

        protected virtual JWTAuthResult ParseAuthorizationHeader(
            AuthorizationFilterContext actionContext
        )
        {
            if (
                actionContext.HttpContext.Request.Headers.TryGetValue(
                    SqlIntegrationDefaults.Token,
                    out var checkToken
                )
            )
            {
                var sqlIntegrationSettings =
                    EngineContext.Current.Resolve<SqlIntegrationSettings>();
                var token = checkToken.FirstOrDefault();
                if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                    token = token.Substring("Bearer ".Length);
                var secretKey = sqlIntegrationSettings.IntegrationSecretKey;
                try
                {
                    var payload =
                        JwtHelper.JwtDecoder.DecodeToObject(token, secretKey, true);

                    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
                    payload.TryGetValue("createdon", out var createdon);
                    payload.TryGetValue("exp", out var exp);

                    if (now >= (double)createdon && now <= (double)exp)
                        return JWTAuthResult.Success;

                    return JWTAuthResult.Expired;
                }
                catch (Exception e)
                {
                    return JWTAuthResult.Invalid;
                }
            }

            return JWTAuthResult.Invalid;
        }

        private void Challenge(AuthorizationFilterContext actionContext, JWTAuthResult result)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var response = new IntegrationResponseModel
            {
                ErrorList = new List<string>
                {
                    result == JWTAuthResult.Invalid
                        ? localizationService
                            .GetResourceAsync("Integration.Response.InvalidToken")
                            .Result
                        : localizationService
                            .GetResourceAsync("Integration.Response.TokenExpired")
                            .Result,
                },
            };

            actionContext.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };

            return;
        }
    }

    #endregion
}
