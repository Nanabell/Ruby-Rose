using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Misc
{
    [Name("Misc"), Group]
    public class UserinfoCommand : ModuleBase
    {
        [Command("Userinfo"), Alias("UInfo", "Joined")]
        [Summary("Retun some Userinformation")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task Userinfo([Remainder] IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;
            var userList = await Context.Guild.GetUsersAsync();
            await Context.Channel.SendEmbedAsync(_embedBuilder(user, userList, user.GetColorFromUser()));
        }

        private static EmbedBuilder _embedBuilder(IGuildUser user, IReadOnlyCollection<IGuildUser> guildUsers,
            uint color)
        {
            var serverRanking = guildUsers.Where(u => u.JoinedAt != DateTimeOffset.MinValue)
                .OrderBy(x => x.JoinedAt)
                .ToList();
            var accountRanking = guildUsers.OrderBy(x => x.CreatedAt).ToList();

            var en = new CultureInfo("en-en");
            var embed = new EmbedBuilder
            {
                Color = new Color(color),
                ThumbnailUrl = user.GetAvatarUrl(),
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{user} {(user.Nickname != null ? $"({user.Nickname})" : "")}",
                    IconUrl = user.GetAvatarUrl(),
                    Url = user.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"User ID: {user.Id}"
                }
            };
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Created";
                field.Value =
                    $"{user.CreatedAt.UtcDateTime.ToString(en.DateTimeFormat.LongDatePattern, en)} {user.CreatedAt.UtcDateTime.ToString(en.DateTimeFormat.LongTimePattern, en)}";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Joined";
                field.Value = user.JoinedAt != DateTimeOffset.MinValue
                    ? $"{user.JoinedAt?.UtcDateTime.ToString(en.DateTimeFormat.LongDatePattern, en)} {user.JoinedAt?.UtcDateTime.ToString(en.DateTimeFormat.LongTimePattern, en)}"
                    : "<Error>";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Created Span";
                field.Value = "```cs\n" + Utils.DateTimeSpan.CompareDates(user.CreatedAt.UtcDateTime, DateTime.UtcNow)
                                  .TimeDisplay() + "\n```";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Joined Span";
                if (user.JoinedAt != DateTimeOffset.MinValue)
                {
                    if (user.JoinedAt != null)
                        field.Value = "```cs\n" + Utils.DateTimeSpan
                                          .CompareDates(user.JoinedAt.Value.UtcDateTime, DateTime.UtcNow)
                                          .TimeDisplay() + "\n```";
                    else field.Value = "<Error>";
                }
                else field.Value = "<Error>";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Account Ranking";
                field.Value =
                    $"{(accountRanking != null ? $"{accountRanking.IndexOf(user)} / {accountRanking.Count + 1}" : "<Cache Error>")}";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Server Ranking";
                field.Value = user.JoinedAt != DateTimeOffset.MinValue
                    ? $"{serverRanking.IndexOf(user) + 1} / {serverRanking.Count}"
                    : "<Error>";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Status";
                field.Value = $"{user.Status}";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Playing";
                field.Value = $"{(user.Game != null ? $"{user.Game}" : "<None>")}";
            });

            return embed;
        }
    }
}