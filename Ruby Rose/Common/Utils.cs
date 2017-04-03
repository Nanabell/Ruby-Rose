using Newtonsoft.Json;
using RubyRose.Database;
using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;

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

        public static void LoadNLogSettings()
        {
#if DEBUG
            var CcTarget = new ColoredConsoleTarget()
            {
                Layout = @"${date:HH\:mm\:ss} - [ ${pad:padding=5:inner=${level:uppercase=true}} ] ${message}${onexception:${newline}EXCEPTION\: ${exception:format=ToString}}",
                Name = "Console",
            };
            LogManager.Configuration.AddTarget(CcTarget);
            LogManager.Configuration.AddRuleForAllLevels("Console");
#endif
            var debugTarget = new FileTarget
            {
                Name = "Debug",
                Layout = @"${date:HH\:mm\:ss} [ ${pad:padding=5:inner=${Level}} ] ${message}${onexception:${newline}${exception:format=ToString}}",
                LineEnding = LineEndingMode.Default,
                MaxArchiveFiles = 7,
                ArchiveFileName = @"${basedir}../../../../Logs/Debug-{#}.log",
                ArchiveDateFormat = "yyyMMdd",
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveOldFileOnStartup = true,
                FileName = @"${basedir}../../../../Logs/Debug.log",
                DeleteOldFileOnStartup = true
            };
            LogManager.Configuration.AddTarget(debugTarget);
            LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, "Debug");

            var mainTarget = new FileTarget
            {
                Name = "Main",
                Layout = @"${date:HH\:mm\:ss} [ ${pad:padding=5:inner=${Level}} ] ${message}${onexception:${newline}${exception:format=ToString}}",
                LineEnding = LineEndingMode.Default,
                MaxArchiveFiles = 7,
                ArchiveFileName = @"${basedir}../../../../Logs/RubyRose-{#}.log",
                ArchiveDateFormat = "yyyMMdd",
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveOldFileOnStartup = true,
                FileName = @"${basedir}../../../../Logs/RubyRose.log",
                DeleteOldFileOnStartup = true
            };
            LogManager.Configuration.AddTarget(mainTarget);
            LogManager.Configuration.AddRule(LogLevel.Info, LogLevel.Fatal, "Main");

            LogManager.ReconfigExistingLoggers();
        }

        public static Credentials LoadConfig()
        {
            return JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("./credentials.json"));
        }
    }
}