using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NLog;
using RubyRose.Database;
using RubyRose.Database.Models;
using RubyRose.Services.CustomResponse;
using RubyRose.Services.Logging;

namespace RubyRose.Services.EventHandler
{
    public static class EventHandlerService
    {
        private static DiscordSocketClient _client;
        private static CommandService _commandService;
        private static MongoClient _mongo;
        private static CoreConfig _config;
        private static IServiceProvider _provider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Task Install(IServiceProvider provider)
        {
            _client = provider.GetService<DiscordSocketClient>();
            _mongo = provider.GetService<MongoClient>();
            _config = provider.GetService<CoreConfig>();
            _commandService = provider.GetService<CommandService>();
            _provider = provider;
            return Task.CompletedTask;
        }

        public static Task StartHandlers()
        {
            _client.JoinedGuild += OnJoinedGuild;
            _client.GuildAvailable += OnGuildAvailable;
            _client.Ready += OnReady;
            _client.Log += OnLog;
            _commandService.Log += CommandServiceOnLog;

            return Task.CompletedTask;
        }

        private static Task OnJoinedGuild(SocketGuild socketGuild)
        {
            Task.Run(async () =>
            {
                await _mongo.GetCollection<Settings>(_client).GetByGuildAsync(socketGuild.Id);
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private static Task CommandServiceOnLog(LogMessage logMessage)
        {
            LogLevel level;

            switch (logMessage.Severity)
            {
                case LogSeverity.Debug:
                    level = LogLevel.Info;
                    break;

                case LogSeverity.Verbose:
                    level = LogLevel.Info;
                    break;

                case LogSeverity.Info:
                    level = LogLevel.Info;
                    break;

                case LogSeverity.Warning:
                    level = LogLevel.Warn;
                    break;

                case LogSeverity.Error:
                    level = LogLevel.Error;
                    break;

                case LogSeverity.Critical:
                    level = LogLevel.Fatal;
                    break;

                default:
                    level = LogLevel.Off;
                    break;
            }

            if (logMessage.Exception == null)
                Logger.Log(level, logMessage.Message);
            else
                Logger.Log(level, logMessage.Exception, logMessage.Message);

            return Task.CompletedTask;
        }

        private static Task OnLog(LogMessage logMessage)
        {
            LogLevel level;

            switch (logMessage.Severity)
            {
                case LogSeverity.Debug:
                    level = LogLevel.Trace;
                    break;

                case LogSeverity.Verbose:
                    level = LogLevel.Debug;
                    break;

                case LogSeverity.Info:
                    level = LogLevel.Info;
                    break;

                case LogSeverity.Warning:
                    level = LogLevel.Warn;
                    break;

                case LogSeverity.Error:
                    level = LogLevel.Error;
                    break;

                case LogSeverity.Critical:
                    level = LogLevel.Fatal;
                    break;

                default:
                    level = LogLevel.Off;
                    break;
            }

            if (logMessage.Exception == null)
                Logger.Log(level, logMessage.Message);
            else
                Logger.Log(level, logMessage.Exception, logMessage.Message);

            return Task.CompletedTask;
        }

        private static async Task OnReady()
        {
            await Task.Delay(500);
            Logger.Info($"Set Game to: {_config.Game}");
            await _client.SetGameAsync(_config.Game);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(2000);

                var logging = new MessageLoggingService(_provider);
                await logging.StartLogging();
/*
                var rwbyFight = new RwbyFightService(_provider);
                await rwbyFight.StartService();
                
                var rwbySleeper = new RwbySleepService(_provider);
                await rwbySleeper.StartService();
*/
            }).ConfigureAwait(false);
        }

        private static Task OnGuildAvailable(SocketGuild socketGuild)
        {
            Logger.Info($"Connected to {socketGuild}");
            return Task.CompletedTask;
        }
    }
}