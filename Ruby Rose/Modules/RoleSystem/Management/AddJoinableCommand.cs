using Discord;
using Discord.Commands;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RubyRose.Modules.RoleSystem.Management
{
    [Name("Role System Management"), Group]
    public class AddJoinableCommand : ModuleBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public AddJoinableCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("AddJoinable")]
        [Summary("Mark a Role as Joinable with a Keyword")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task AddJoinable(string name, IRole role, int RoleLevel = 0)
        {
            name = name.ToLower();
            var allJoinables = _mongo.GetCollection<Joinables>(Context.Client);
            var joinables = await GetJoinablesAsync(allJoinables, Context.Guild);

            var newjoin = new Joinables
            {
                GuildId = Context.Guild.Id,
                Name = name,
                RoleId = role.Id,
                Level = RoleLevel,
            };

            if (!joinables.Exists(x => x.Name == name))
            {
                await allJoinables.InsertOneAsync(newjoin);
                await ReplyAsync($"Joinable `{name.ToFirstUpper()}` added to Database");
                logger.Info($"New Joinable {name} on {Context.Guild.Id}");
            }
            else
            {
                await ReplyAsync($"Joinable {name.ToFirstUpper()} already existent");
                logger.Warn($"Failed to add new Joinable {name} to {Context.Guild.Name}, already Existent");
            }
        }

        private async Task<List<Joinables>> GetJoinablesAsync(IMongoCollection<Joinables> collection, IGuild guild)
        {
            var joinablesCursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            return await joinablesCursor.ToListAsync();
        }
    }
}