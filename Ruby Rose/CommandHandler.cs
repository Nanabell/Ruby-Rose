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
        private static bool temp;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ConcurrentDictionary<ulong, bool> IsEnabled = new ConcurrentDictionary<ulong, bool>();
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private MongoClient _mongo;
        private IDependencyMap _map;
        private Credentials _credentials;

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var argPos = 0;
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            if (message.Content == _credentials.Prefix) return;

            var author = message.Author as SocketGuildUser;
            if (author == null) return;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_credentials.Prefix, ref argPos))) return;

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
                        logger.Warn($"[Commanmd] {GetCommandName(commandInfo, message, argPos)}" +
                            $" [{(context.IsPrivate ? "Private" : context.Guild.Name)}]" +
                            $" {(context.IsPrivate ? "" : $"#{context.Channel.Name} ")}" +
                            $"({context.User})" +
                            $" {message.Content.Substring(GetCommandName(commandInfo, message, argPos).Length + _credentials.Prefix.Length)}" +
                            $" => {result.Error}");

                        IsEnabled.TryGetValue(context.Guild.Id, out temp);
                        if (temp)
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
                        else logger.Warn($"[Command] Suppressing Result on behalf of settings");
                    }
                    else
                    {
                        logger.Info($"[Commanmd] {GetCommandName(commandInfo, message, argPos)}" +
                            $" [{(context.IsPrivate ? "Private" : context.Guild.Name)}]" +
                            $" {(context.IsPrivate ? "" : $"#{context.Channel.Name} ")}" +
                            $"({context.User})" +
                            $" {message.Content.Substring(GetCommandName(commandInfo, message, argPos).Length + _credentials.Prefix.Length)}");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "[Command] Something went wrong Executing a Command");
                }
            });
        }

        public static void MongoLoader(MongoClient mongo, DiscordSocketClient client)
        {
            var allSettings = mongo.GetCollection<Settings>(client).Find("{}").ToList();

            foreach (var setting in allSettings)
                IsEnabled.AddOrUpdate(setting.GuildId, setting.ExecutionErrorAnnounce, (key, oldvalue) => setting.ExecutionErrorAnnounce);
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
            logger.Debug("[CommandService] Creating new CommandService");
            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Info,
#if DEBUG
                DefaultRunMode = RunMode.Sync,
                ThrowOnError = true
#elif RELEASE
                DefaultRunMode = RunMode.Async
#endif
            });
            logger.Trace("[CommandService] Adding TypeReaders to CommandService");
            _commandService.AddTypeReader<CommandInfo>(new CommandInfoTypeReader(_commandService));
            _commandService.AddTypeReader<IAttachment>(new AttachmentsTypeReader());
            _client = map.Get<DiscordSocketClient>();
            _mongo = map.Get<MongoClient>();
            _credentials = map.Get<Credentials>();
            _map = map;

            logger.Debug("[CommandService] Loading Modules from Entry Assembly");
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            logger.Info("[CommandService] Starting CommandHandler");
            _client.MessageReceived += HandleCommand;
        }

        private static async void ExecuteError(ICommandContext context, ExecuteResult executeResult, SearchResult searchResult)
        {
            logger.Error(executeResult.Exception, $"[Command] An Error Occured on {context.Guild.Name} caused by {context.User} Exeption:");
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
            catch { logger.Warn("[Command]  Not enough Permission to Display Error Message. Gracefull fail.."); }
        }
    }
}