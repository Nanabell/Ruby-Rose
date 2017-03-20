using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RubyRose.Common;
using RubyRose.Config;

namespace RubyRose
{
    public class CommandHandler
    {
        private CommandService _commandService;
        private DiscordSocketClient _client;
        private IDependencyMap _map;

        public async Task Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _commandService = map.Get<CommandService>();
            _map = map;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var argPos = 0;
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            if (message.Content == Settings.Prefix) return;

            var author = message.Author as SocketGuildUser;
            if (author == null) return;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Settings.Prefix, ref argPos))) return;

            var context = new CommandContext(_client, message);
            var searchResult = _commandService.Search(context, argPos);
            CommandInfo commandInfo = null;
            if (searchResult.IsSuccess) commandInfo = searchResult.Commands.First().Command;

            await Task.Run(async () =>
            {
                try
                {
                    var result = await _commandService.ExecuteAsync(context, argPos, _map);

                    Logging.LogMessage("Info", "Command",
                        $"{commandInfo?.Name ?? message.Content.Substring(argPos).Split(' ').First()} => {(result.IsSuccess ? result.ToString() : result.Error.ToString())} | [{(context.IsPrivate ? "Private" : context.Guild.Name)}] {(context.IsPrivate ? "" : $"#{context.Channel.Name} ")}({context.User}) || {message.Content.Substring(message.Content.Split(' ').First().Length)}");

                    if (!result.IsSuccess)
                    {
                        if (result is ExecuteResult)
                        {
                            ExecuteError(context, (ExecuteResult) result, searchResult);
                        }
                        else if (result is PreconditionResult)
                        {
                            await context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition(((PreconditionResult) result).ErrorReason));
                        }
                        else if (result is ParseResult)
                        {
                            await context.Channel.SendEmbedAsync(Embeds.Invalid(((ParseResult) result).ErrorReason));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private static async void ExecuteError(ICommandContext context, ExecuteResult executeResult, SearchResult searchResult)
        {
            File.WriteAllText($"Crash-Reports/{DateTime.Now.ToFileTimeUtc()}.log",
                $"Exeption Occured at: {executeResult.Exception}, Stacktrace:\n{executeResult.Exception.StackTrace}");
            File.WriteAllText("Crash-Reports/latest.log",
                $"Exeption Occured at: {executeResult.Exception}, Stacktrace:\n{executeResult.Exception.StackTrace}");

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
            catch { Logging.LogMessage("Critial", "Command", $"Not enough Permission to Display Error Message. Gracefull fail.."); }
        }
    }
}