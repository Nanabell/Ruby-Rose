using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.TypeReaders;
using RubyRose.Database;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RubyRose
{
    public class CommandHandler
    {
        private static ConcurrentDictionary<ulong, bool> ResultAnnounce = new ConcurrentDictionary<ulong, bool>();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private IDependencyMap _map;
        private CoreConfig _config;

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var argPos = 0;
            var message = parameterMessage as SocketUserMessage;
            var author = message.Author as SocketGuildUser;

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

            var sResult = _commandService.Search(context, argPos);
            CommandInfo commandInfo = null;
            if (sResult.IsSuccess) commandInfo = sResult.Commands.First().Command;

            await Task.Run(async () =>
            {
                try
                {
                    var result = await _commandService.ExecuteAsync(context, argPos, _map);

                    logger.Info($"Command ran by {context.User} in {context.Guild.Name} - {context.Message.Content}");

                    if (!result.IsSuccess)
                    {
                        logger.Warn($"Command failed to run successfully. {result.ErrorReason}");

                        ResultAnnounce.TryGetValue(context.Guild.Id, out var IsEnabled);
                        if (IsEnabled)
                        {
                            string response = null;
                            switch (result)
                            {
                                case SearchResult searchResult:
                                    break;

                                case ParseResult parseResult:
                                    response = $":warning: There was an error parsing your command: `{parseResult.ErrorReason}`";
                                    break;

                                case PreconditionResult preconditionResult:
                                    response = $":warning: A precondition of your command failed: `{preconditionResult.ErrorReason}`";
                                    break;

                                case ExecuteResult executeResult:
                                    response = $":warning: Your command failed to execute. If this persists, contact the Bot Developer.\n`{executeResult.Exception.Message}`";
                                    logger.Error(executeResult.Exception);
                                    break;
                            }
                        }
                        else logger.Warn($"Suppressing Result on behalf of settings");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Something went wrong Executing a Command");
                }
            });
        }

        public static async Task ReloadResultAnnounce(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                ResultAnnounce.AddOrUpdate(settings.GuildId, settings.ResultAnnounce, (key, oldvalue) => settings.ResultAnnounce);
        }

        private string GetCommandName(CommandInfo info, SocketUserMessage message, int argPos)
        {
            if (info != null)
            {
                if (!info.Module.IsSubmodule)
                    return info.Name;
                else return info.Module.Name + " " + info.Name;
            }
            else return message.Content.Substring(argPos).Split(' ').First();
        }

        public async Task Install(IDependencyMap map)
        {
            logger.Debug("Creating new CommandService");
            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = RunMode.Sync,
                ThrowOnError = true
            });
            logger.Trace("Adding TypeReaders to CommandService");
            _commandService.AddTypeReader<CommandInfo>(new CommandInfoTypeReader(_commandService));
            _commandService.AddTypeReader<IAttachment>(new AttachmentsTypeReader());
            _client = map.Get<DiscordSocketClient>();
            _config = map.Get<CoreConfig>();
            _map = map;
            await ReloadResultAnnounce(_client, map.Get<MongoClient>());

            logger.Debug("Loading Modules from Entry Assembly");
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            logger.Info("Starting CommandHandler");
            _commandService.Log += Program.Logging;
            _client.MessageReceived += HandleCommand;
        }
    }
}