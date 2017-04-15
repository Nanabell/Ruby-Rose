using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using RubyRose.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Services.Logging
{
    public class LoggingService : ServiceBase
    {
        private static MongoClient _mongo;

        protected override Task PreDisable()
        {
            Client.MessageReceived -= Client_MessageReceived;
            Client.MessageUpdated -= Client_MessageUpdated;
            Client.MessageDeleted -= Client_MessageDeleted;

            return Task.CompletedTask;
        }

        protected override Task PreEnable()
        {
            _mongo = Map.Get<MongoClient>();

            Client.MessageReceived += Client_MessageReceived;
            Client.MessageUpdated += Client_MessageUpdated;
            Client.MessageDeleted += Client_MessageDeleted;

            return Task.CompletedTask;
        }

        private static async Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            var removedMessage = await arg.GetOrDownloadAsync();
            if (removedMessage is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(Client);
                    var messageLogging = await allMessageLoggings.GetByMessageIdAsyc(user.Guild, message.Id);

                    messageLogging.IsDeleted = true;

                    await allMessageLoggings.SaveAsync(messageLogging);
                }
            }
        }

        private static async Task Client_MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (newMessage is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(Client);
                    var messageLogging = await allMessageLoggings.GetByMessageIdAsyc(user.Guild, message.Id);

                    messageLogging.IsEdited = true;
                    messageLogging.Edits.Add(message.Content);

                    await allMessageLoggings.SaveAsync(messageLogging);
                }
            }
        }

        private static async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(Client);

                    var newMessage = new MessageLoggings
                    {
                        GuildId = user.Guild.Id,
                        ChannelId = message.Channel.Id,
                        UserId = user.Id,
                        MessageId = message.Id,
                        Timestamp = message.Timestamp.UtcDateTime,
                        Content = message.Content,
                        IsEdited = false,
                        Edits = new List<string>(),
                        IsDeleted = false,
                        AttachmentUrls = new List<string>(message.Attachments.Select(x => x.Url))
                    };

                    await allMessageLoggings.InsertOneAsync(newMessage);
                }
            }
        }

        protected override bool WaitForReady()
            => true;
    }
}