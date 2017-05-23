using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace RubyRose.Common.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        public RatelimitAttribute(uint times, double period, Measure measure)
        {
            _invokeLimit = times;

            //TODO: C# 7 candidate switch expression
            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;

                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;

                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;

                case Measure.Seconds:
                    _invokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(measure), measure, null);
            }
        }

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        public RatelimitAttribute(uint times, TimeSpan period)
        {
            _invokeLimit = times;
            _invokeLimitPeriod = period;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var now = DateTime.UtcNow;
            CommandTimeout t;
            var timeout = (_invokeTracker.TryGetValue(context.User.Id, out t)
                           && ((now - t.FirstInvoke) < _invokeLimitPeriod))
                ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked > _invokeLimit)
            {
                var gone = now - timeout.FirstInvoke;
                var remain = _invokeLimitPeriod.Subtract(gone);
                var end = DateTime.UtcNow.Add(remain);
                var span = Utils.DateTimeSpan.CompareDates(now, end);
                return Task.FromResult(
                    PreconditionResult.FromError(
                        $"You are Ratelimited. Try again in {(span.TimeDisplay().Length != 0 ? span.TimeDisplay() : "less than a second")}"));
            }
            _invokeTracker[context.User.Id] = timeout;
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private class CommandTimeout
        {
            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }

    /// <summary> Sets the scale of the period parameter. </summary>
    public enum Measure
    {
        /// <summary> Period is measured in days. </summary>
        Days,

        /// <summary> Period is measured in hours. </summary>
        Hours,

        /// <summary> Period is measured in minutes. </summary>
        Minutes,

        //// <summary> Period is measured in seconds. </summary>
        Seconds
    }
}