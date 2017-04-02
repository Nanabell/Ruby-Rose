using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using System.Threading.Tasks;

namespace RubyRose
{
    internal class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool isTestBot = false;

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            Utils.LoadNLogSettings();

            logger.Debug("[Configuration] Loading configuration");
            var config = Utils.LoadConfig();

            logger.Trace("[Discord] Initiating DiscordClient");
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 10
            });

            logger.Debug("[Database] Connecting to MongoDb");
            var mongo = new MongoClient(config.Database.Mongo);
            //Quartz later here
            var handler = new CommandHandler();

            logger.Debug("[Discord] Creating new DependencyMap");
            var map = new DependencyMap();
            map.Add(client);
            map.Add(mongo);
            map.Add(config);
            logger.Trace("[Discord] Added Client, Mongodb & config to DependencyMap");

            var events = new EventHandlers(map);
            events.Install();

            logger.Trace($"[Gateway] Starting Login to Discord");
            if (!isTestBot)
                await client.LoginAsync(TokenType.Bot, config.Token);
            else
                await client.LoginAsync(TokenType.Bot, config.TestBotToken);
            logger.Info("[Gateway] Starting Bot");
            await client.StartAsync();

            logger.Debug("[CommandService] Installing Command Handler");
            await handler.Install(map);

            await Task.Delay(-1);
        }
    }
}