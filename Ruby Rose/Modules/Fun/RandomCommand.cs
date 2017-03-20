using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Fun
{
    [Name("Fun"), Group]
    public class RandomCommand : ModuleBase
    {
        [Command("Random"), Alias("Randu")]
        [Summary("Pick a random User from the Server")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task Random()
        {
            var users = await Context.Guild.GetUsersAsync();
            var rnd = new Random();
            var user = users.ElementAt(rnd.Next(0, users.Count));
            await Context.Channel.SendEmbedAsync(Embeds.Success("", $"__{user}__"));
        }
    }
}