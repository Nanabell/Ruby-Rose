using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Services;
using System.Threading.Tasks;

namespace RubyRose
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

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
            var config = CoreConfig.ReadConfig();

            Logger.Info("Initiating DiscordClient");
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100
            });

            client.Log += Logging;

            Logger.Info("Connecting to MongoDb");
            var mongo = new MongoClient(config.Database.Mongo);

            var map = new DependencyMap();
            map.Add(client);
            map.Add(mongo);
            map.Add(config);

            Logger.Info("Initializing Service Handler");
            new ServiceHandler(map);

            Logger.Info("Starting Login to Discord");
            await client.LoginAsync(TokenType.Bot, (config.IsMainBot ? config.Token.Main : config.Token.Dev));

            Logger.Info("Starting Bot");
            await client.StartAsync();

            Logger.Info("Initializing Command Handler");
            var handler = new CommandHandler();
            await handler.Install(map);

            await Task.Delay(-1);
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