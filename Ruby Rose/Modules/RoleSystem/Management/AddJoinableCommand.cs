using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Modules.RoleSystem.Db;

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
        public async Task AddJoinable(string keyword, IRole role)
        {
            keyword = keyword.ToLower();
            var jrcollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<JoinSystemSerializer>("JoinSystem");

            var joinable = new JoinSystemSerializer
            {
                Keyword = keyword,
                Role = new Role
                {
                    Id = role.Id,
                    Name = role.Name
                }
            };

            if (!jrcollec.Find(f => f.Role.Id == role.Id).Any())
            {
                await jrcollec.InsertOneAsync(joinable);
                await Context.Channel.SendEmbedAsync(Embeds.Success($"{role.Name} now joinable",
                    $"The Role {role.Name} is now joinable with the Keyword: {keyword.ToFirstUpper()}"));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(
                    Embeds.Invalid($"The Role {role.Name} is already marked as Joinable."));
            }
        }
    }
}