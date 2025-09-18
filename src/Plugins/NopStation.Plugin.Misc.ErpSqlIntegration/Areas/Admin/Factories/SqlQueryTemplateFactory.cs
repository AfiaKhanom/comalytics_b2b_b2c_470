using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Factories;

public class SqlQueryTemplateModelFactory : ISqlQueryTemplateModelFactory
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IPluginService _pluginService;
    private readonly ILocalizationService _localizationService;
    private readonly ISqlQueryTemplatService _sqlQueryTemplatService;

    #endregion

    #region Ctor

    public SqlQueryTemplateModelFactory(ISettingService settingService,
        IStoreContext storeContext,
        IPluginService pluginService,
        ILocalizationService localizationService,
        ISqlQueryTemplatService sqlQueryTemplatService)
    {
        _settingService = settingService;
        _storeContext = storeContext;
        _pluginService = pluginService;
        _localizationService = localizationService;
        _sqlQueryTemplatService = sqlQueryTemplatService;
    }

    #endregion

    #region Methods

    public async Task<SqlQueryTemplateSearchModel> PrepareSqlQueryTemplateModelSearchModelAsync(SqlQueryTemplateSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    public async Task<SqlQueryTemplateListModel> PrepareSqlQueryTemplateListModelAsync(SqlQueryTemplateSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var sqlQueryTemplates = await _sqlQueryTemplatService.GetAllSqlQueryTemplatesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new SqlQueryTemplateListModel().PrepareToGridAsync(searchModel, sqlQueryTemplates, () =>
        {
            return sqlQueryTemplates.SelectAwait(async queryTemplate =>
            {
                return new SqlQueryTemplateModel()
                {
                    Id = queryTemplate.Id,
                    Query = queryTemplate.Query,
                    ErpSyncLevelId = queryTemplate.ErpSyncLevelId
                };
            });
        });

        return model;
    }

    public async Task<SqlQueryTemplateModel> PrepareSqlQueryTemplateModelAsync(SqlQueryTemplateModel model, SqlQueryTemplate sqlQueryTemplate)
    {
        var ignoredSync = new[]
        { 
            ErpSyncLevel.SalesRep,
            ErpSyncLevel.ErpNopUser,
            ErpSyncLevel.LoginLogout,
            ErpSyncLevel.SalesOrg
        };

        if (sqlQueryTemplate != null)
        {
            if (model == null)
            {
                model = new SqlQueryTemplateModel
                {
                    Query = sqlQueryTemplate.Query,
                    ErpSyncLevelId = sqlQueryTemplate.ErpSyncLevelId
                };
            }

            model.ErpSyncLevels = Enum.GetValues(typeof(ErpSyncLevel))
                .Cast<ErpSyncLevel>()
                .Where(e => (int)e != sqlQueryTemplate.ErpSyncLevelId &&
                                        !ignoredSync.Contains(e))
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                }).ToList();

            model.ErpSyncLevels.Insert(0,
                new SelectListItem
                {
                    Text = ((ErpSyncLevel)sqlQueryTemplate.ErpSyncLevelId).ToString(),
                    Value = sqlQueryTemplate.ErpSyncLevelId.ToString()
                });

            return model;
        }

        model.ErpSyncLevels = Enum.GetValues(typeof(ErpSyncLevel))
        .Cast<ErpSyncLevel>()
        .Where(e => !ignoredSync.Contains(e))
        .Select(e => new SelectListItem
        {
            Text = e.ToString(),
            Value = ((int)e).ToString()
        }).ToList();

        model.ErpSyncLevels.Insert(0,
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0"
                });

        return model;
    }

    #endregion
}