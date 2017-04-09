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
            if (message == null) return;
            if (message.Content == _config.Prefix) return;

            var author = message.Author as SocketGuildUser;
            if (author == null) return;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_config.Prefix, ref argPos))) return;

            var context = new CommandContext(_client, message);
            var searchResult = _commandService.Search(context, argPos);
            CommandInfo commandInfo = null;
            if (searchResult.IsSuccess) commandInfo = searchResult.Commands.First().Command;

            await Task.Run(async () =>
            {
                try
                {
                    var result = await _commandService.ExecuteAsync(context, argPos, _map);

                    if (!result.IsSuccess)
                    {
                        logger.Warn($"{GetCommandName(commandInfo, message, argPos)}" +
                            $" [{(context.IsPrivate ? "Private" : context.Guild.Name)}]" +
                            $" {(context.IsPrivate ? "" : $"#{context.Channel.Name} ")}" +
                            $"({context.User})" +
                            $" {message.Content.Substring(GetCommandName(commandInfo, message, argPos).Length + _config.Prefix.Length)}" +
                            $" => {result.Error}");

                        ResultAnnounce.TryGetValue(context.Guild.Id, out var IsEnabled);
                        if (IsEnabled)
                        {
                            if (result is ExecuteResult)
                            {
                                ExecuteError(context, (ExecuteResult)result, searchResult);
                            }
                            else if (result is PreconditionResult)
                            {
                                await context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition(((PreconditionResult)result).ErrorReason));
                            }
                            else if (result is ParseResult)
                            {
                                await context.Channel.SendEmbedAsync(Embeds.Invalid(((ParseResult)result).ErrorReason));
                            }
                        }
                        else logger.Warn($"Suppressing Result on behalf of settings");
                    }
                    else
                    {
                        logger.Info($"{GetCommandName(commandInfo, message, argPos)}" +
                            $" [{(context.IsPrivate ? "Private" : context.Guild.Name)}]" +
                            $" {(context.IsPrivate ? "" : $"#{context.Channel.Name} ")}" +
                            $"({context.User})" +
                            $" {message.Content.Substring(GetCommandName(commandInfo, message, argPos).Length + _config.Prefix.Length)}");
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
                ResultAnnounce.TryAdd(settings.GuildId, settings.ResultAnnounce);
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
            _commandService.Log += Program.logging;
            _client.MessageReceived += HandleCommand;
        }

        private static async void ExecuteError(ICommandContext context, ExecuteResult executeResult, SearchResult searchResult)
        {
            logger.Error(executeResult.Exception, $"An Error Occured on {context.Guild.Name} caused by {context.User} Exeption:");
            var embed = new EmbedBuilder
            {
                Title = "Error executing command",
                Description = string.Format("User {0} failed to execute command **{1}**.\n\nError Reason:\n{2}", context.User,
                    searchResult.Commands.FirstOrDefault().Command.Name, executeResult.ErrorReason.LimitLengh(1000)),
                Color = new Color(255, 127, 0),
                ThumbnailUrl = context.User.GetAvatarUrl(),

                Footer = new EmbedFooterBuilder
                {
                    Text = "Please Report this to the BotOwner!"
                }
            };

            try
            {
                await context.Channel.SendEmbedAsync(embed);
            }
            catch { logger.Warn("Not enough Permission to Display Error Message. Gracefull fail.."); }
        }
    }
}