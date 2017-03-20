using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Misc
{
    [Name("Misc"), Group]
    public class ServerinfoCommand : ModuleBase
    {
        [Command("ServerInfo"), Alias("SInfo")]
        [Summary("Return Server Information")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30 ,Measure.Seconds)]
        public async Task ServerInfo()
        {
            await ReplyAsync("", embed: ServerEmbed(Context.Guild as SocketGuild, (Context.User as IGuildUser).GetColorFromUser()));
        }

        private static EmbedBuilder ServerEmbed(SocketGuild guild, uint color)
        {
            var en = new CultureInfo("en-en");
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(color));
            embed.WithThumbnailUrl(guild.IconUrl);

            embed.AddField((field) =>
            {
                field.IsInline = false;
                field.Name = guild.Name;
                field.Value = $"{guild.CreatedAt.ToString(en.DateTimeFormat.LongDatePattern, en)} {guild.CreatedAt.ToString(en.DateTimeFormat.LongTimePattern, en)}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Region";
                field.Value = guild.VoiceRegionId;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Users";
                field.Value = $"{guild.Users.Count}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Text Channels";
                field.Value = $"{guild.VoiceChannels.Count}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Voice Channels";
                field.Value = $"{guild.VoiceChannels.Count}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Roles";
                field.Value = $"{guild.Roles.Count}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Owner";
                field.Value = $"{guild.Owner}";
            });
            embed.WithFooter(footer =>
            {
                footer.Text = $"Server ID: {guild.Id}";
            });
            return embed;
        }
    }
}