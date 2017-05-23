using Discord;
using Discord.Commands;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Threading.Tasks;
using RubyRose.Database.Models;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.RoleSystem.Management
{
    [Name("Role System Management"), Group]
    public class RemoveJoinableCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public RemoveJoinableCommand(IServiceProvider map)
        {
            _mongo = map.GetService<MongoClient>();
        }

        [Command("RemoveJoinable")]
        [Summary("Remove the marker from a marked Joinable role")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task RemoveJoinable(string name)
        {
            name = name.ToLower();

            var allJoinables = _mongo.GetCollection<Joinables>(Context.Client);
            var joinable = await GetJoinableAsync(allJoinables, Context.Guild, name);

            if (joinable != null)
            {
                await allJoinables.DeleteAsync(joinable);
                await ReplyAsync($"Joinable `{name}` dropped from Database");
                Logger.Info($"Deleted Joinable {name} on {Context.Guild.Name}");
            }
            else
            {
                Logger.Warn($"Failed to delete joinable {name} on {Context.Guild.Name}, not Existent");
                await ReplyAsync($"Joinable `{name.ToFirstUpper()}` not Existent");
            }
        }

        private async Task<Joinables> GetJoinableAsync(IMongoCollection<Joinables> collection, IGuild guild, string name)
        {
            var joinablesCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await joinablesCursor.FirstOrDefaultAsync();
        }
    }
}