using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Factories;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Controllers;

public class SqlQueryTemplateController : NopStationAdminController
{
    #region Fields

    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILogger _logger;
    private readonly ISqlQueryTemplateModelFactory _sqlQueryTemplateModelFactory;
    private readonly ISqlQueryTemplatService _sqlQueryTemplatService;
    private readonly SqlClient _sqlClient;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;

    #endregion Fields

    #region Ctor

    public SqlQueryTemplateController(IErpActivityLogsService erpActivityLogsService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        INopFileProvider nopFileProvider,
        ILogger logger,
        ISqlQueryTemplateModelFactory sqlQueryTemplateModelFactory,
        ISqlQueryTemplatService sqlQueryTemplatService,
        SqlClient sqlClient,
        IStoreContext storeContext,
        ISettingService settingService)
    {
        _erpActivityLogsService = erpActivityLogsService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _fileProvider = nopFileProvider;
        _logger = logger;
        _sqlQueryTemplateModelFactory = sqlQueryTemplateModelFactory;
        _sqlQueryTemplatService = sqlQueryTemplatService;
        _sqlClient = sqlClient;
        _storeContext = storeContext;
        _settingService = settingService;
    }

    #endregion Ctor
   
    #region Methods

    public async Task<IActionResult> SqlQueryTemplates()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateModelSearchModelAsync(new SqlQueryTemplateSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(SqlQueryTemplateSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateListModelAsync(searchModel);

        return Json(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateModelAsync(new SqlQueryTemplateModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(SqlQueryTemplateModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var sqlQueryTemplate = await _sqlQueryTemplatService.GetSqlQueryTemplateBySyncLevelIdAsync(model.ErpSyncLevelId);
        if (sqlQueryTemplate is not null)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.ErpSqlIntegration.SqlQueryTemplate.AlreadyExistForThisSyncLevel"));
        }

        if (ModelState.IsValid)
        {
            sqlQueryTemplate = new SqlQueryTemplate
            {
                Query = model.Query,
                ErpSyncLevelId = model.ErpSyncLevelId
            };
            await _sqlQueryTemplatService.InsertSqlQueryTemplateAsync(sqlQueryTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ErpSqlIntegration.SqlQueryTemplate.Added"));

            if (!continueEditing)
                return RedirectToAction("SqlQueryTemplates");

            return RedirectToAction("Edit", new { id = sqlQueryTemplate.Id });
        }

        //prepare model
        model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateModelAsync(model, null);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var sqlQueryTemplate = await _sqlQueryTemplatService.GetSqlQueryTemplateByIdAsync(id);
        if (sqlQueryTemplate == null)
            return RedirectToAction("SqlQueryTemplates");

        //prepare model
        var model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateModelAsync(null, sqlQueryTemplate);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Edit(SqlQueryTemplateModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var sqlQueryTemplate = await _sqlQueryTemplatService.GetSqlQueryTemplateByIdAsync(model.Id);
        if (sqlQueryTemplate == null)
            return RedirectToAction("SqlQueryTemplates");

        if (ModelState.IsValid)
        {
            sqlQueryTemplate = await _sqlQueryTemplatService.GetSqlQueryTemplateBySyncLevelIdAsync(model.ErpSyncLevelId);

            if (sqlQueryTemplate == null)
            {
                sqlQueryTemplate = new SqlQueryTemplate
                {
                    Query = model.Query,
                    ErpSyncLevelId = model.ErpSyncLevelId
                };
                await _sqlQueryTemplatService.InsertSqlQueryTemplateAsync(sqlQueryTemplate);
            }
            else
            {
                sqlQueryTemplate.Query = model.Query;
                await _sqlQueryTemplatService.UpdateSqlQueryTemplateAsync(sqlQueryTemplate);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ErpSqlIntegration.SqlQueryTemplate.Updated"));

            if (!continueEditing)
                return RedirectToAction("SqlQueryTemplates");

            return RedirectToAction("Edit", new { id = sqlQueryTemplate.Id });
        }

        //prepare model
        model = await _sqlQueryTemplateModelFactory.PrepareSqlQueryTemplateModelAsync(model, sqlQueryTemplate);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> GetSqlQueryBySyncLevelId(int erpSyncLevelId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return BadRequest();

        //prepare model
        var model = await _sqlQueryTemplatService.GetSqlQueryTemplateBySyncLevelIdAsync(erpSyncLevelId);

        if (model == null)
            return Json(string.Empty);

        return Json(model.Query);
    }

    #endregion Methods
}