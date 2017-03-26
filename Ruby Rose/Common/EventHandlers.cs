using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Custom_Reactions;
using RubyRose.Database;
using Serilog;
using System;
using System.Threading.Tasks;

namespace RubyRose.Common
{
    public class EventHandlers
    {
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
            Log.Verbose($"Set Game to: {_credentials.NowPlaying}");
            await _client.SetGameAsync(_credentials.NowPlaying);
        }

        private async Task GuildAvailable(SocketGuild guild)
        {
            Log.Information($"Connected to {guild.Name}");

            var c = _mongo.GetDiscordDb(_client);
            var cGuild = await c.Find(g => g.Id == guild.Id).FirstOrDefaultAsync();
            if (cGuild == null)
            {
                await c.InsertOneAsync(new DatabaseModel { Id = guild.Id, Command = null, Joinable = null, OneTruePair = null, Tag = null, User = null });
                Log.Information($"Added {guild.Name} to the Database");
            }
        }

        private Task LogEvents(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Debug:
                    { Log.Debug(msg.Message); break; }
                case LogSeverity.Verbose:
                    { Log.Verbose(msg.Message); break; }
                case LogSeverity.Info:
                    { Log.Information(msg.Message); break; }
                case LogSeverity.Warning:
                    { Log.Warning(msg.Message); break; }
                case LogSeverity.Error:
                    { Log.Error(msg.Message); break; }
            }
            return Task.CompletedTask;
        }
    }
}