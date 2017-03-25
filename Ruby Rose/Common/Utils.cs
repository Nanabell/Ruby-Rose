using Newtonsoft.Json;
using RubyRose.Database;
using System;
using System.IO;

namespace RubyRose.Common
{
    public static class Utils
    {
        public struct DateTimeSpan
        {
            public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
            {
                this.Years = years;
                this.Months = months;
                this.Days = days;
                this.Hours = hours;
                this.Minutes = minutes;
                this.Seconds = seconds;
                this.Milliseconds = milliseconds;
            }

            public int Years { get; }
            public int Months { get; }
            public int Days { get; }
            public int Hours { get; }
            public int Minutes { get; }
            public int Seconds { get; }
            public int Milliseconds { get; }

            private enum Phase { Years, Months, Days, Done }

            public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
            {
                if (date2 < date1)
                {
                    var sub = date1;
                    date1 = date2;
                    date2 = sub;
                }

                var current = date1;
                var years = 0;
                var months = 0;
                var days = 0;

                var phase = Phase.Years;
                var span = new DateTimeSpan();
                var officialDay = current.Day;

                while (phase != Phase.Done)
                {
                    switch (phase)
                    {
                        case Phase.Years:
                            if (current.AddYears(years + 1) > date2)
                            {
                                phase = Phase.Months;
                                current = current.AddYears(years);
                            }
                            else
                            {
                                years++;
                            }
                            break;

                        case Phase.Months:
                            if (current.AddMonths(months + 1) > date2)
                            {
                                phase = Phase.Days;
                                current = current.AddMonths(months);
                                if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                                    current = current.AddDays(officialDay - current.Day);
                            }
                            else
                            {
                                months++;
                            }
                            break;

                        case Phase.Days:
                            if (current.AddDays(days + 1) > date2)
                            {
                                current = current.AddDays(days);
                                var timespan = date2 - current;
                                span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                                phase = Phase.Done;
                            }
                            else
                            {
                                days++;
                            }
                            break;

                        case Phase.Done:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return span;
            }
        }

        public static Credentials LoadConfig()
        {
            return JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("./credentials.json"));
        }
    }
}