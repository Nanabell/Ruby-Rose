using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation")]
    public class SoftBanCommand : ModuleBase
    {
        [Command("SoftBan")]
        [MinPermission(AccessLevel.ServerModerator)]
        [RequireBotPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IGuildUser user, int prunedays = 7)
        {
            var invokerpos = getPosition((Context.User as IGuildUser));
            var targetpos = getPosition(user);
            var botpos = getPosition(await Context.Guild.GetCurrentUserAsync());
            if (botpos > targetpos)
            {
                if (invokerpos > targetpos || Context.User.Id == Context.Guild.OwnerId)
                {
                    await Context.Guild.AddBanAsync(user, prunedays);
                    await Context.Guild.RemoveBanAsync(user);
                    await Context.Channel.SendEmbedAsync(Embeds.Success("*Softbanned!*", $"{user} has been softbanned from the Guild!"));
                }
                else await Context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition($"You cant softban someone who is above or equal to you in the role hierarchy"));
            }
            else await Context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition("I cant softban somone who is higher or equal to me in the role hierarchy"));
        }

        private int getPosition(IGuildUser user)
            => user.GetRoles().OrderByDescending(x => x.Position).FirstOrDefault().Position;
    }
}