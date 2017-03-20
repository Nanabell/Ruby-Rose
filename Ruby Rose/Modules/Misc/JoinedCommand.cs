using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Misc
{
    [Name("Misc"), Group]
    public class JoinedCommand : ModuleBase
    {
        [Command("Joined"), Alias("Since")]
        [Summary("Return the Joindate of a User")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task Joined([Remainder]IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;
            await ReplyAsync("", embed: JoinedEmbed(user, user.GetColorFromUser()));
        }

        private EmbedBuilder JoinedEmbed(IGuildUser user, uint color)
        {
            var en = new CultureInfo("en-en");
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(color));

            embed.WithAuthor((author) =>
            {
                author.Name = $"{user}";
                author.IconUrl = user.GetAvatarUrl();
                author.Url = user.GetAvatarUrl();
            });

            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Joined";
                field.Value = user.JoinedAt != DateTimeOffset.MinValue
                    ? $"**{user.JoinedAt?.UtcDateTime.ToString(en.DateTimeFormat.LongDatePattern, en)}** at {user.JoinedAt?.UtcDateTime.ToString(en.DateTimeFormat.LongTimePattern, en)}"
                    : "<Error, please use a diffrent bot for now>";
            });
            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = "Span";
                if (user.JoinedAt != DateTimeOffset.MinValue)
                {
                    if (user.JoinedAt != null)
                        field.Value = "```cs\n" + Utils.DateTimeSpan.CompareDates(user.JoinedAt.Value.UtcDateTime, DateTime.UtcNow).TimeDisplay() + "\n```";
                }
                else field.Value = "<Error, please use a diffrent bot for now>";
            });

            embed.WithCurrentTimestamp();

            return embed;
        }
    }
}