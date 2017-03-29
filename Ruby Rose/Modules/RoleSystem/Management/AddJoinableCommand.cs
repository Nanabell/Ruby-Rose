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

namespace RubyRose.Modules.RoleSystem.Management
{
    [Name("Role System Management"), Group]
    public class AddJoinableCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public AddJoinableCommand(IDependencyMap map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            _mongo = map.Get<MongoClient>();
        }

        [Command("AddJoinable")]
        [Summary("Mark a Role as Joinable with a Keyword")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task AddJoinable(string keyword, IRole role, int RoleLevel = 0)
        {
            keyword = keyword.ToLower();
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstOrDefaultAsync();

            var newjoin = new Joinables
            {
                Keyword = keyword,
                Level = RoleLevel,
                Role = new Roles
                {
                    Id = role.Id
                }
            };
            if (cGuild.Joinable == null)
            {
                await c.UpdateOneAsync(f => f.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.AddToSet("Joinable", newjoin), new UpdateOptions { IsUpsert = true });
                Log.Information($"Added Role {role.Name} to Joinable Database with Keyword: {keyword}");
                await Context.Channel.SendEmbedAsync(Embeds.Success($"{role.Name} now joinable", $"The Role {role.Name} is now joinable with the Keyword: {keyword.ToFirstUpper()}"));
            }
            else
            {
                if (!cGuild.Joinable.Any(f => f.Keyword == keyword))
                {
                    cGuild.Joinable.Add(newjoin);
                    await c.UpdateOneAsync(f => f.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Joinable", cGuild.Joinable), new UpdateOptions { IsUpsert = true });
                    Log.Information($"Added Role {role.Name} to Joinable Database with Keyword: {keyword}");
                    await Context.Channel.SendEmbedAsync(Embeds.Success($"{role.Name} now joinable", $"The Role {role.Name} is now joinable with the Keyword: {keyword.ToFirstUpper()}"));
                }
                else
                {
                    Log.Error($"Tried to add {keyword} to db but is already existing");
                    await Context.Channel.SendEmbedAsync(Embeds.Exeption($"Keyword {keyword} already assigned to a Role"));
                }
            }
        }
    }
}