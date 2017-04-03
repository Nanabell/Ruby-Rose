using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Handler;
using System.Threading.Tasks;

namespace RubyRose
{
    internal class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            Utils.LoadNLogSettings();

            logger.Debug("[Configuration] Loading configuration");
            var config = Utils.LoadConfig();

            logger.Debug("[Discord] Initiating DiscordClient");
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 10
            });

            logger.Debug("[Database] Connecting to MongoDb");
            var mongo = new MongoClient(config.Database.Mongo);
            var handler = new CommandHandler();

            logger.Debug("[Discord] Creating new DependencyMap");
            var map = new DependencyMap();
            map.Add(client);
            map.Add(mongo);
            map.Add(config);
            logger.Trace("[Discord] Added Client, Mongodb & config to DependencyMap");

            logger.Debug("[EventHandler] Installing Event Handler");
            var events = new EventHandlers(map);
            await events.Install();

            await SettingsManager.Install(map);

            logger.Trace($"[Gateway] Starting Login to Discord");
            await client.LoginAsync(TokenType.Bot, (config.IsTestBot ? config.TestBotToken : config.Token));
            logger.Info("[Gateway] Starting Bot");
            await client.StartAsync();

            logger.Info("[CommandService] Installing Command Handler");
            await handler.Install(map);

            await Task.Delay(-1);
        }
    }
}