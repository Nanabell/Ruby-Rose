using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Misc
{
    [Name("Misc"), Group]
    public class AvatarCommand : ModuleBase
    {
        [Command("Avatar"), Alias("Ava")]
        [Summary("Return the Users Avatar")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task Avatar([Remainder]IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;
            await ReplyAsync("", embed: AvatarEmbed(user, user.GetColorFromUser()));
        }

        private static EmbedBuilder AvatarEmbed(IUser user, uint color)
        {
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(color));

            embed.WithAuthor((author) =>
            {
                author.Name = $"{user}";
                author.IconUrl = user.GetAvatarUrl(ImageFormat.Auto);
                author.Url = user.GetAvatarUrl(ImageFormat.Auto, 1024);
            });

            embed.ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 1024);
            embed.WithFooter(footer =>
            {
                footer.Text = "I KNOW... NO ETA. DONT ASK";
            });
            embed.WithCurrentTimestamp();

            return embed;
        }
    }
}