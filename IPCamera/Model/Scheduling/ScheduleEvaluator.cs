using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.Scheduling
{
    public class ScheduleEvaluator
    {
        Schedule schedule;

        public ScheduleEvaluator(Schedule schedule)
        {
            this.schedule = schedule;
        }

        public bool CanRecord()
        {


            foreach (var p in schedule.InactivePeriods)
            {
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, p.StartHour, p.StartMinute, 0);
                DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, p.StartHour, p.StartMinute, 0);

                if (DateTime.Now > start && DateTime.Now < end)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
