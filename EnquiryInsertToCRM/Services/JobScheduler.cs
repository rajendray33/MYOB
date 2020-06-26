using EnquiryInsertToCRM.Controllers;
using EnquiryInsertToCRM.Models;
using Quartz;
using Quartz.Impl;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EnquiryInsertToCRM.Services
{
    public class JobScheduler
    {

    }

    public class Scheduler_DealerDailyReportMail : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            using (HomeController ctrl = new HomeController())
            {
                ctrl.SendDealerDailyReport();
            }
            return Task.CompletedTask;
        }
    }

    public class JobScheduler_Init
    {
        public async void Start()
        {
            string userTimeZoneKey = "AUS Eastern Standard Time";
            //string userTimeZoneKey = "India Standard Time";
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneKey);

            #region Dealer Daily Reoprt Scheduler Mail
            IScheduler scheduler1 = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler1.Start();

            IJobDetail job1 = JobBuilder.Create<Scheduler_DealerDailyReportMail>().Build();

            ITrigger trigger1 = TriggerBuilder.Create()
              .WithIdentity("DealerDailyReportMail1", "DealerDailyReportMailGroup1")
              .WithDailyTimeIntervalSchedule(s => s
              .InTimeZone(userTimeZone)
              .OnEveryDay()
              .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
              .EndingDailyAfterCount(1))
              .Build();
            await scheduler1.ScheduleJob(job1, trigger1);
            #endregion


        }
    }

}