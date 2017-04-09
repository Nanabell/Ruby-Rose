using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            _logger.Info($"Connected to {guild.Name}");

            var allSettings = _mongo.GetCollection<Settings>(Client);
            var settings = await allSettings.Find("{}").ToListAsync();

            _logger.Debug($"Checking if Guild {guild.Name} is Existent in Database");
            if (!settings.Exists(s => s.GuildId == guild.Id))
            {
                var newsettings = new Settings { GuildId = guild.Id };
                _logger.Debug($"Adding missing Guild {guild.Name} to Database");
                await allSettings.InsertOneAsync(newsettings);
            }
            else _logger.Debug($"Guild {guild.Name} Existent");
        }
    }
}