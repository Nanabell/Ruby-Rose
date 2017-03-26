using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Modules.Fun
{
    [Name("Fun"), Group]
    public class OneTruePairCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public OneTruePairCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("OneTruePair"), Alias("OTP")]
        [Summary("Find your One True Pair..")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task OneTruePair()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var rnd = new Random();
            var selectionRole = Context.Guild.EveryoneRole;

            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstOrDefaultAsync();

            if (cGuild.OneTruePair != null)
            {
                selectionRole = Context.Guild.GetRole(cGuild.OneTruePair.Id);
                Log.Verbose($"OneTruePair Role found and accepted: {selectionRole.Name}");
            }
            else Log.Verbose("No OneTruePair Role set defaulting to Everyone Role");

            var allUsers = await Context.Guild.GetUsersAsync();
            var users = allUsers.Where(x => x.GetRoles().Any(r => r == selectionRole)).ToList();

            if (users.Count < 2)
            {
                await Context.Channel.SendEmbedAsync(
                    Embeds.Invalid("Not Enough Members in selected Otp Role. *Canceling..*"));
                return;
            }

            var first = users.ElementAt(rnd.Next(0, users.Count));
            users.Remove(first);
            var second = users.ElementAt(rnd.Next(0, users.Count));

            if (first.Id == application.Owner.Id || second.Id == application.Owner.Id)
            {
                var embed = new EmbedBuilder
                {
                    Description =
                        $":revolving_hearts: {application.Owner.Username} x `well Tehehe... {application.Owner.Username} belongs to me only` :revolving_hearts:",
                    Color = new Color(0xABECF1)
                };
                await Context.Channel.SendEmbedAsync(embed);
            }
            else if (first.Id == Context.Client.CurrentUser.Id || second.Id == Context.Client.CurrentUser.Id)
            {
                var embed = new EmbedBuilder
                {
                    Description =
                        $":revolving_hearts: {Context.Client.CurrentUser.Username} x `well Tehehe... I belong to {application.Owner.Username} only` :revolving_hearts:",
                    Color = new Color(0xA10808)
                };
                await Context.Channel.SendEmbedAsync(embed);
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Description = $":revolving_hearts: {first.Username} x {second.Username} :revolving_hearts:",
                    Color = new Color(0xC442D4)
                };
                await Context.Channel.SendEmbedAsync(embed);
            }
        }
    }
}