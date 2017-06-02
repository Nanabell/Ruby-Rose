using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NLog;

namespace RubyRose.Common
{
    public static class Extentions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string LimitLengh(this string str, int maxLengh) => str.Length <= maxLengh
            ? str
            : str.Substring(0, maxLengh);

        public static IEnumerable<IRole> GetRoles(this IGuildUser user)
            => user.RoleIds.Select(x => user.Guild.GetRole(x));

        public static uint GetColorFromUser(this IGuildUser user)
        {
            var roles = user.GetRoles().OrderByDescending(role => role.Position);
            return roles.Any(role => role.Color.RawValue != 0)
                ? roles.First(role => role.Color.RawValue != 0).Color.RawValue
                : 0;
        }

        public static bool CheckChannelPermission(this IMessageChannel channel, ChannelPermission permission, IGuildUser guildUser)
        {
            var guildchannel = channel as IGuildChannel;

            ChannelPermissions perms;
            perms = guildchannel != null ? guildUser.GetPermissions(guildchannel) : ChannelPermissions.All(null);

            return perms.Has(permission);
        }

        public static async Task<IEnumerable<CommandInfo>> CheckConditionsAsync(this IEnumerable<CommandInfo> commandInfos,
            ICommandContext context, IServiceProvider map = null)
        {
            var ret = new List<CommandInfo>();
            foreach (var commandInfo in commandInfos)
            {
                if (!(await commandInfo.CheckPreconditionsAsync(context, map)).IsSuccess) continue;
                Logger.Trace($"[Precondition] Command {commandInfo.Name} passed all Checks");
                ret.Add(commandInfo);
            }
            return ret;
        }

        public static async Task<IUserMessage> ReplyAsync(this ICommandContext context, string message, bool mention = true)
            => await context.Channel.SendMessageAsync($"{(mention ? context.User.Mention + ", " : "")}{message}");

        public static async Task<IUserMessage> ReplyAsync(this ICommandContext context, Embed embed)
            => await context.Channel.SendMessageAsync(string.Empty, embed: embed);

        public static async Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder builder)
            => await channel.SendMessageAsync("", false, builder);

        public static string ToFirstUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string TimeDisplay(this Utils.DateTimeSpan time)
        {
            return
                $"{(time.Years != 0 ? (time.Years == 1 ? $"{time.Years} Year " : $"{time.Years} Years ") : "")}" +
                $"{(time.Months != 0 ? (time.Months == 1 ? $"{time.Months} Month " : $"{time.Months} Months ") : "")}" +
                $"{(time.Days != 0 ? (time.Days == 1 ? $"{time.Days} Day " : $"{time.Days} Days ") : "")}" +
                $"{(time.Hours != 0 ? (time.Hours == 1 ? $"{time.Hours} Hour " : $"{time.Hours} Hours ") : "")}" +
                $"{(time.Minutes != 0 ? (time.Minutes == 1 ? $"{time.Minutes} Minute " : $"{time.Minutes} Minutes ") : "")}" +
                $"{(time.Seconds != 0 ? (time.Seconds == 1 ? $"{time.Seconds} Second " : $"{time.Seconds} Seconds ") : "")}";
        }
    }
}