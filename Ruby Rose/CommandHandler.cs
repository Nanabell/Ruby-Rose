using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common.TypeReaders;
using RubyRose.Database;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using RubyRose.Common;
using RubyRose.Database.Models;

namespace RubyRose
{
    public class CommandHandler
    {
        private static readonly ConcurrentDictionary<ulong, bool> ResultAnnounce = new ConcurrentDictionary<ulong, bool>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private IDependencyMap _map;
        private CoreConfig _config;

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var argPos = 0;
            var message = parameterMessage as SocketUserMessage;
            var author = message?.Author as SocketGuildUser;

            var context = new CommandContext(_client, message);

            if (message == null)
                return;
            if (message.Content == _config.Prefix)
                return;
            if (author == null)
                return;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_config.Prefix, ref argPos)))
                return;
            if (!context.Channel.CheckChannelPermission(ChannelPermission.SendMessages, await context.Guild.GetCurrentUserAsync()))
                return;

            await Task.Run(async () =>
            {
                try
                {
                    var result = await _commandService.ExecuteAsync(context, argPos, _map);

                    Logger.Info($"Command ran by {context.User} in {context.Guild.Name} - {context.Message.Content}");

                    if (!result.IsSuccess)
                    {
                        Logger.Warn($"Command failed to run successfully. {result.ErrorReason}");

                        ResultAnnounce.TryGetValue(context.Guild.Id, out var isEnabled);
                        if (isEnabled)
                        {
                            string response = null;
                            switch (result)
                            {
                                case SearchResult _:
                                    break;

                                case ParseResult parseResult:
                                    response = $":warning: There was an error parsing your command: `{parseResult.ErrorReason}`";
                                    break;

                                case PreconditionResult preconditionResult:
                                    response = $":warning: A precondition of your command failed: `{preconditionResult.ErrorReason}`";
                                    break;

                                case ExecuteResult executeResult:
                                    response = $":warning: Your command failed to execute. If this persists, contact the Bot Developer.\n`{executeResult.Exception.Message}`";
                                    Logger.Error(executeResult.Exception);
                                    break;
                            }

                            if (response != null)
                                await context.ReplyAsync(response);
                        }
                        else Logger.Warn("Suppressing Result on behalf of settings");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Something went wrong Executing a Command");
                }
            });
        }

        public static async Task ReloadResultAnnounce(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                ResultAnnounce.AddOrUpdate(settings.GuildId, settings.ResultAnnounce, (key, oldvalue) => settings.ResultAnnounce);
        }

        public async Task Install(IDependencyMap map)
        {
            Logger.Debug("Creating new CommandService");
            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = RunMode.Sync,
                ThrowOnError = true
            });
            Logger.Trace("Adding TypeReaders to CommandService");
            _commandService.AddTypeReader<CommandInfo>(new CommandInfoTypeReader(_commandService));
            _commandService.AddTypeReader<IAttachment>(new AttachmentsTypeReader());
            _client = map.Get<DiscordSocketClient>();
            _config = map.Get<CoreConfig>();
            _map = map;
            await ReloadResultAnnounce(_client, map.Get<MongoClient>());

            Logger.Debug("Loading Modules from Entry Assembly");
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            Logger.Info("Starting CommandHandler");
            _commandService.Log += Program.Logging;
            _client.MessageReceived += HandleCommand;
        }
    }
}