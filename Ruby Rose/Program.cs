using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace RubyRose
{
    internal class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            var config = Utils.LoadConfig();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile("Logs/debug-{Date}.log", outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", retainedFileCountLimit: 7)
                .WriteTo.RollingFile("Logs/RubyRose-{Date}.log", restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", retainedFileCountLimit: 7)
                .WriteTo.RollingFile("Logs/error-{Date}.log", restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", retainedFileCountLimit: 7)
                .CreateLogger();

            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 10
            });

            var mongo = new MongoClient(config.Database.Mongo);
            //Quartz later here

            var handler = new CommandHandler();

            var map = new DependencyMap();
            map.Add(client);
            map.Add(mongo);
            map.Add(config);

            var events = new EventHandlers(map);
            events.Install();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            await handler.Install(map);

            await Task.Delay(-1);
        }
    }
}