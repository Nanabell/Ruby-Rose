using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RubyRose.Services.EventHandler;

namespace RubyRose
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private MongoClient _mongo;
        private CoreConfig _config;
        private CommandHandler _handler;
        
        public async Task Start()
        {
            Logger.Info("\n" + @"
                                      bbbbbbbb
RRRRRRRRRRRRRRRRR                     b::::::b                                          RRRRRRRRRRRRRRRRR
R::::::::::::::::R                    b::::::b                                          R::::::::::::::::R
R::::::RRRRRR:::::R                   b::::::b                                          R::::::RRRRRR:::::R
RR:::::R     R:::::R                   b:::::b                                          RR:::::R     R:::::R
  R::::R     R:::::Ruuuuuu    uuuuuu   b:::::bbbbbbbbb    yyyyyyy           yyyyyyy       R::::R     R:::::R   ooooooooooo       ssssssssss       eeeeeeeeeeee
  R::::R     R:::::Ru::::u    u::::u   b::::::::::::::bb   y:::::y         y:::::y        R::::R     R:::::R oo:::::::::::oo   ss::::::::::s    ee::::::::::::ee
  R::::RRRRRR:::::R u::::u    u::::u   b::::::::::::::::b   y:::::y       y:::::y         R::::RRRRRR:::::R o:::::::::::::::oss:::::::::::::s  e::::::eeeee:::::ee
  R:::::::::::::RR  u::::u    u::::u   b:::::bbbbb:::::::b   y:::::y     y:::::y          R:::::::::::::RR  o:::::ooooo:::::os::::::ssss:::::se::::::e     e:::::e
  R::::RRRRRR:::::R u::::u    u::::u   b:::::b    b::::::b    y:::::y   y:::::y           R::::RRRRRR:::::R o::::o     o::::o s:::::s  ssssss e:::::::eeeee::::::e
  R::::R     R:::::Ru::::u    u::::u   b:::::b     b:::::b     y:::::y y:::::y            R::::R     R:::::Ro::::o     o::::o   s::::::s      e:::::::::::::::::e
  R::::R     R:::::Ru::::u    u::::u   b:::::b     b:::::b      y:::::y:::::y             R::::R     R:::::Ro::::o     o::::o      s::::::s   e::::::eeeeeeeeeee
  R::::R     R:::::Ru:::::uuuu:::::u   b:::::b     b:::::b       y:::::::::y              R::::R     R:::::Ro::::o     o::::ossssss   s:::::s e:::::::e
RR:::::R     R:::::Ru:::::::::::::::uu b:::::bbbbbb::::::b        y:::::::y             RR:::::R     R:::::Ro:::::ooooo:::::os:::::ssss::::::se::::::::e
R::::::R     R:::::R u:::::::::::::::u b::::::::::::::::b          y:::::y              R::::::R     R:::::Ro:::::::::::::::os::::::::::::::s  e::::::::eeeeeeee
R::::::R     R:::::R  uu::::::::uu:::u b:::::::::::::::b          y:::::y               R::::::R     R:::::R oo:::::::::::oo  s:::::::::::ss    ee:::::::::::::e
RRRRRRRR     RRRRRRR    uuuuuuuu  uuuu bbbbbbbbbbbbbbbb          y:::::y                RRRRRRRR     RRRRRRR   ooooooooooo     sssssssssss        eeeeeeeeeeeeee
                                                                y:::::y
                                                               y:::::y
                                                              y:::::y
                                                             y:::::y
                                                            yyyyyyy");

            Logger.Info("Loading configuration");
            _config = CoreConfig.ReadConfig();

            Logger.Info("Initiating DiscordClient");
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100
            });
            
            Logger.Info("Connecting to MongoDb");
            _mongo = new MongoClient(_config.MongoConnectionString);

            var provider = ConfigureProvider();
            
            _handler = new CommandHandler(provider);
            _client.Ready += StartHandler;

            await EventHandlerService.Install(provider);
            await EventHandlerService.StartHandlers();

            await Task.Delay(500);

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();
            Logger.Info("Started Bot");
            
            await Task.Delay(-1);
        }

        private Task StartHandler()
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await _handler.StartServiceAsync();
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureProvider()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_mongo)
                .AddSingleton(_config)
                .AddSingleton(new CommandService(
                    new CommandServiceConfig {CaseSensitiveCommands = false, ThrowOnError = false}));

            return services.BuildServiceProvider();
        }

        public static Task Logging(LogMessage arg)
        {
            LogLevel level;

            switch (arg.Severity)
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

            if (arg.Exception == null)
                Logger.Log(level, arg.Message);
            else
                Logger.Log(level, arg.Exception, arg.Message);

            return Task.CompletedTask;
        }
    }
}