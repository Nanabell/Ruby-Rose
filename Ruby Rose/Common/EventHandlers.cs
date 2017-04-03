using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Custom_Reactions;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RubyRose.Common
{
    public class EventHandlers
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly MongoClient _mongo;
        private readonly Credentials _credentials;

        public EventHandlers(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _credentials = map.Get<Credentials>();
            _mongo = map.Get<MongoClient>();
        }

        public void Install()
        {
            _client.Log += LogEvents;
            _client.Ready += Ready;
            _client.MessageReceived += RwbyFight.MessageHandler;
            _client.GuildAvailable += GuildAvailable;
        }

        private async Task Ready()
        {
            logger.Info("[SettingsManager] Loading Settings for Guilds");
            await SettingsManager.LoadSettings();

            logger.Info($"[Gateway] Set Game to: {_credentials.NowPlaying}");
            await _client.SetGameAsync(_credentials.NowPlaying);
        }

        private async Task GuildAvailable(SocketGuild guild)
        {
            logger.Info($"[Gateway] Connected to {guild.Name}");

            var allSettings = _mongo.GetCollection<Settings>(_client);
            var settings = await allSettings.Find("{}").ToListAsync();

            logger.Debug($"[Database] Checking if Guild {guild.Name} is Existent in Database");
            if (!settings.Exists(s => s.GuildId == guild.Id))
            {
                var newsettings = new Settings
                {
                    GuildId = guild.Id,
                    CustomReactions = true,
                    ExecutionErrorAnnounce = true
                };
                logger.Debug($"[Database] Adding missing Guild {guild.Name} to Database");
                await allSettings.InsertOneAsync(newsettings);
            }
            else logger.Debug($"[Database] Guild {guild.Name} Existent");
        }

        private Task LogEvents(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Debug:
                    { logger.Trace($"[{msg.Source}] {msg.Message}"); break; }
                case LogSeverity.Verbose:
                    { logger.Debug($"[{msg.Source}] {msg.Message}"); break; }
                case LogSeverity.Info:
                    { logger.Info($"[{msg.Source}] {msg.Message}"); break; }
                case LogSeverity.Warning:
                    { logger.Warn($"[{msg.Source}] {msg.Message}"); break; }
                case LogSeverity.Error:
                    { logger.Error($"[{msg.Source}] {msg.Message}"); break; }
            }
            return Task.CompletedTask;
        }
    }
}