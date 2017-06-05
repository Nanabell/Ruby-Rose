using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common.TypeReaders;
using RubyRose.Database;
using System;
using System.Reflection;
using System.Threading.Tasks;
using RubyRose.Common;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose
{
    public class CommandHandler
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly MongoClient _mongo;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _provider;
        private readonly CoreConfig _config;

        public CommandHandler(IServiceProvider provider)
        {
            _commandService = provider.GetService<CommandService>();
            _client = provider.GetService<DiscordSocketClient>();
            _mongo = provider.GetService<MongoClient>();
            _config = provider.GetService<CoreConfig>();
            _provider = provider;

            _logger.Info("Loading TypeReaders");
            _commandService.AddTypeReader<CommandInfo>(new CommandInfoTypeReader(_commandService));
            _commandService.AddTypeReader<IAttachment>(new AttachmentsTypeReader());

            _client.MessageReceived += HandleCommand;
        }

        internal async Task StartServiceAsync()
        {
            _logger.Info("Loading Modules from Assembly");
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
            _logger.Info("CommandHandler started");
        }

        public async Task HandleCommand(SocketMessage socketMessage)
        {
            var argPos = 0;

            if (socketMessage is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser)
                {
                    var context = new CommandContext(_client, message);

                    if (message.Content == _config.Prefix)
                        return;
                    if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                          message.HasStringPrefix(_config.Prefix, ref argPos)))
                        return;
                    if (!context.Channel.CheckChannelPermission(ChannelPermission.SendMessages,
                        await context.Guild.GetCurrentUserAsync()))
                        return;

                    var _ = Task.Run(async () =>
                    {
                        var result = await _commandService.ExecuteAsync(context, argPos, _provider);

                        if (!result.IsSuccess)
                        {
                            _logger.Warn($"Command failed to run successfully. {result.ErrorReason}");

                            string response = null;
                            switch (result)
                            {
                                case SearchResult searchResult:
                                    if (!searchResult.IsSuccess)
                                        _logger.Debug(searchResult.Error);
                                    else
                                        _logger.Info(searchResult.Text);
                                    break;

                                case ParseResult parseResult:
                                    response =
                                        $":warning: There was an error parsing your command: `{parseResult.ErrorReason}`";
                                    break;

                                case PreconditionResult preconditionResult:
                                    response =
                                        $":warning: A precondition of your command failed: `{preconditionResult.ErrorReason}`";
                                    break;

                                case ExecuteResult executeResult:
                                    response =
                                        $":warning: Your command failed to execute. If this persists, contact the Bot Developer.\n`{executeResult.Exception.Message}`";
                                    _logger.Error(executeResult.Exception);
                                    break;
                            }
                            var settings = await _mongo.GetCollection<Settings>(_client)
                                .GetByGuildAsync(context.Guild.Id);

                            if (response != null && settings.IsErrorReporting)
                                await context.ReplyAsync(response);
                        }
                    });
                }
            }
        }
    }
}