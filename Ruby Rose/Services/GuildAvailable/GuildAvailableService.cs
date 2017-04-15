using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using System.Threading.Tasks;
using RubyRose.Database.Models;

namespace RubyRose.Services.GuildAvailable
{
    public class GuildAvailableService : ServiceBase
    {
        private MongoClient _mongo;

        protected override Task PreDisable()
        {
            Client.GuildAvailable -= Client_GuildAvailable;
            return Task.CompletedTask;
        }

        protected override Task PreEnable()
        {
            _mongo = Map.Get<MongoClient>();
            Client.GuildAvailable += Client_GuildAvailable;
            return Task.CompletedTask;
        }

        protected override bool WaitForReady()
            => false;

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            Logger.Info($"Connected to {guild.Name}");

            var allSettings = _mongo.GetCollection<Settings>(Client);
            var settings = await allSettings.Find("{}").ToListAsync();

            Logger.Debug($"Checking if Guild {guild.Name} is Existent in Database");
            if (!settings.Exists(s => s.GuildId == guild.Id))
            {
                var newsettings = new Settings { GuildId = guild.Id };
                Logger.Debug($"Adding missing Guild {guild.Name} to Database");
                await allSettings.InsertOneAsync(newsettings);
            }
            else Logger.Debug($"Guild {guild.Name} Existent");
        }
    }
}