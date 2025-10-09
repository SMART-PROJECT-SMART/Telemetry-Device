using Quartz;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Quartz.Jobs;

namespace TelemetryDevices.Services.Quartz.TelemetryDeviceStatusManager
{
    public class QuartzTelemetryDeviceStatusManager : IQuartzTelemetryDeviceStatusManager
    {
        private readonly IScheduler _scheduler;

        public QuartzTelemetryDeviceStatusManager(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task<bool> StartSchedular(int intervalSeconds)
        {
            IJobDetail job = CreateJob();
            ITrigger trigger = CreateTrigger(intervalSeconds);

            await _scheduler.ScheduleJob(job, trigger);
            await _scheduler.Start();

            return true;
        }

        public async Task<bool> StopSchedular()
        {
            JobKey jobKey = new JobKey(
                TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_JOB_KEY,
                TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_GROUP_NAME
            );

            if (await _scheduler.CheckExists(jobKey))
            {
                await _scheduler.DeleteJob(jobKey);
            }

            if (_scheduler.IsStarted)
            {
                await _scheduler.Shutdown();
            }

            return true;
        }

        private IJobDetail CreateJob()
        {
            return JobBuilder.Create<TelemetryDeviceStatusJob>()
                .WithIdentity(
                    TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_JOB_KEY,
                    TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_GROUP_NAME
                )
                .Build();
        }

        private ITrigger CreateTrigger(int intervalSeconds)
        {
            return TriggerBuilder.Create()
                .WithIdentity(
                    TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_TRIGGER_KEY,
                    TelemetryDeviceConstants.Quartz.TELEMETRY_DEVICE_STATUS_UPDATE_GROUP_NAME
                )
                .StartNow()
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(intervalSeconds)
                            .RepeatForever())
                .Build();
        }
    }
}
