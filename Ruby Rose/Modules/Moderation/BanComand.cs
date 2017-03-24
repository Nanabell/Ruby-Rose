using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Moderation
{
    internal class BanComand : ModuleBase
    {
        [Command("Ban")]
        [MinPermission(AccessLevel.ServerModerator), RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, int prunedays = 7)
        {
            if ((Context.User as IGuildUser).GetRoles().OrderByDescending(x => x.Position).First().Position > user.GetRoles().OrderByDescending(x => x.Position).First().Position || Context.User.Id == Context.Guild.OwnerId)
            {
                await Context.Guild.AddBanAsync(user, prunedays);
                await Context.Channel.SendEmbedAsync(Embeds.Success("*Banned!*", $"{user} has been banned from the Guild!"));
            }
            else await Context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition($"You cant ban someone who is above or equal to you in the role hierarchy"));
        }
    }
}