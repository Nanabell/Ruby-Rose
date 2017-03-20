using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.TypeReaders;
using RubyRose.Config;

namespace RubyRose
{
    internal class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 10
            });

            var mongo = new MongoClient(Credentials.MongoStrong);
            //Quartz later here

            var handler = new CommandHandler();
            var service = new CommandService(new CommandServiceConfig() {DefaultRunMode = RunMode.Sync});

            service.AddTypeReader<IAttachment>(new AttachmentsTypeReader());

            var map = new DependencyMap();
            map.Add(client);
            map.Add(service);
            map.Add(mongo);

            var events = new EventHandlers(map);
            events.Install();

            await client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);
            await client.StartAsync();

            await handler.Install(map);

            await Task.Delay(-1);

        }
    }
}