using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.Fun
{
    [Name("Fun"), Group]
    public class OneTruePairCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public OneTruePairCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
        }

        [Command("OneTruePair"), Alias("OTP")]
        [Summary("Find your One True Pair..")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task OneTruePair()
        {
            var rnd = new Random();

            var settings = await _mongo.GetCollection<Settings>(Context.Client).GetByGuildAsync(Context.Guild.Id);
            var selectionRole = Context.Guild.GetRole(settings.OtpRoleId) ?? Context.Guild.EveryoneRole;

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

            var embed = new EmbedBuilder
            {
                Description = $":revolving_hearts: {first.Username} x {second.Username} :revolving_hearts:",
                Color = new Color(0xC442D4)
            };
            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("OtpRole")]
        [MinPermission(AccessLevel.ServerAdmin)]
        public async Task SetRole(IRole role)
        {
            var collection = _mongo.GetCollection<Settings>(Context.Client);
            var settings = await collection.GetByGuildAsync(Context.Guild.Id);
            settings.OtpRoleId = role.Id;
            await collection.SaveAsync(settings);
            await Context.ReplyAsync("Otp Role has been updated!");
        }
    }
}