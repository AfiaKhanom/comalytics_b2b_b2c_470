using LinqToDB.Common;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;

/// <summary>
/// Represents the task model factory implementation
/// </summary>
public partial class SyncTaskModelFactory : ISyncTaskModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ISyncTaskService _syncTaskService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly INopFileProvider _fileProvider;

    #endregion

    #region Ctor

    public SyncTaskModelFactory(IDateTimeHelper dateTimeHelper,
        ISyncTaskService taskService,
        ISyncLogService erpSyncLogService,
        INopFileProvider fileProvider)
    {
        _dateTimeHelper = dateTimeHelper;
        _syncTaskService = taskService;
        _erpSyncLogService = erpSyncLogService;
        _fileProvider = fileProvider;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare task search model
    /// </summary>
    /// <param name="searchModel">SyncTask search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the task search model
    /// </returns>
    public virtual async Task<SyncTaskSearchModel> PrepareTaskSearchModelAsync(SyncTaskSearchModel searchModel)
    {
        if (searchModel is null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged task list model
    /// </summary>
    /// <param name="searchModel">SyncTask search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the task list model
    /// </returns>
    public virtual async Task<SyncTaskListModel> PrepareTaskListModelAsync(SyncTaskSearchModel searchModel)
    {
        if (searchModel is null)
            throw new ArgumentNullException(nameof(searchModel));

        //get schedule tasks
        var tasks = (await _syncTaskService.GetAllTasksAsync()).ToPagedList(searchModel);

        //prepare list model
        return await new SyncTaskListModel().PrepareToGridAsync(searchModel, tasks, () =>
        {
            return tasks.SelectAwait(async task =>
            {
                //fill in model values from the entity
                var taskModel = task.ToModel<SyncTaskModel>();

                //convert dates to the user time
                if (task.LastStartUtc.HasValue)
                {
                    taskModel.LastStartUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastStartUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                if (task.LastEndUtc.HasValue)
                {
                    taskModel.LastEndUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastEndUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                if (task.LastSuccessUtc.HasValue)
                {
                    taskModel.LastSuccessUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastSuccessUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                return taskModel;
            });
        });
    }

    /// <summary>
    /// Prepare a task model
    /// </summary>
    /// <param name="taskId">SyncTask id</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the task model
    /// </returns>
    public virtual async Task<SyncTaskModel> PrepareTaskModelByIdAsync(int taskId)
    {
        // Get task
        var task = await _syncTaskService.GetTaskByIdAsync(taskId);

        // Prepare model
        var model = task.ToModel<SyncTaskModel>();
        var dayOfWeekSlots = JsonConvert.DeserializeObject<List<SyncTaskDaySlotModel>>(task?.DayTimeSlots ?? string.Empty);

        #region DayOfWeek Slots arrangements

        // Initialize TaskTimeSlotModel for each day
        var timeSlotModel = new Dictionary<int, List<SyncTaskTimeSlotModel>>();

        for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            timeSlotModel[dayOfWeek] = new List<SyncTaskTimeSlotModel>();

            // Only proceed if dayOfWeekSlots has entries
            if (!dayOfWeekSlots.IsNullOrEmpty())
            {
                foreach (var daySlot in dayOfWeekSlots)
                {
                    if (daySlot.DayOfWeek == dayOfWeek)
                    {
                        foreach (var time in daySlot.TimeSlots)
                        {
                            timeSlotModel[dayOfWeek].Add(
                                new SyncTaskTimeSlotModel 
                                { 
                                    TimeSlot = time.TimeSlot, 
                                    IsSelected = true 
                                });
                        }
                    }
                }
            }
        }

        // Create daySlotModel based on timeSlotModel
        var daySlotModel = new List<SyncTaskDaySlotModel>();

        for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            daySlotModel.Add(new SyncTaskDaySlotModel
            {
                DayOfWeek = dayOfWeek,
                TimeSlots = timeSlotModel[dayOfWeek],
                IsSelected = timeSlotModel[dayOfWeek].Count != 0
            });
        }

        model.DayOfWeekSlots = daySlotModel;

        #endregion

        model.SyncLogSearchModel.SyncTaskName = task.Name;
        model.SyncLogSearchModel.SyncTaskId = task.Id;

        return model;
    }


    public async Task<SyncLogListModel> PrepareSyncLogListModelAsync(SyncLogSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get backup files
        var syncLogFiles = (await _erpSyncLogService.GetAllSyncLogFiles(searchModel.SyncTaskName,searchModel.SyncTaskId)).ToPagedList(searchModel);

        //prepare list model
        return await new SyncLogListModel().PrepareToGridAsync(searchModel, syncLogFiles, () =>
        {
            return syncLogFiles.SelectAwait(async file => new SyncLogModel
            {
                Name = _fileProvider.GetFileName(file),

                Length = $"{_fileProvider.FileLength(file) / 1024f:F2} Kb",

                TaskId = searchModel.SyncTaskId
            });
        });
    }

    #endregion
}