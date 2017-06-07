using System;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using RubyRose.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Services.Logging
{
    public class MessageLoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly MongoClient _mongo;

        public MessageLoggingService(IServiceProvider provider)
        {
            _client = provider.GetService<DiscordSocketClient>();
            _mongo = provider.GetService<MongoClient>();
        }

        internal Task StartLogging()
        {
            _client.MessageReceived += Client_MessageReceived;
            _client.MessageUpdated += Client_MessageUpdated;
            _client.MessageDeleted += Client_MessageDeleted;
            
            return Task.CompletedTask;
        }
        
        private Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            Task.Run(async () =>
            {
                var removedMessage = await arg.GetOrDownloadAsync();
                if (removedMessage is SocketUserMessage message)
                {
                    if (message.Author is SocketGuildUser user)
                    {
                        var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(_client);
                        var messageLogging = await allMessageLoggings.GetByMessageIdAsyc(user.Guild, message.Id);

                        messageLogging.IsDeleted = true;

                        await allMessageLoggings.SaveAsync(messageLogging);
                    }
                }
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> oldMessage, IDeletable newMessage, ISocketMessageChannel channel)
        {
            Task.Run(async () =>
            {
                if (newMessage is SocketUserMessage message)
                {
                    if (message.Author is SocketGuildUser user)
                    {
                        var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(_client);
                        var messageLogging = await allMessageLoggings.GetByMessageIdAsyc(user.Guild, message.Id);

                        messageLogging.IsEdited = true;
                        messageLogging.Edits.Add(message.Content);

                        await allMessageLoggings.SaveAsync(messageLogging);
                    }
                }
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(IDeletable arg)
        {
            Task.Run(async () =>
            {
                if (arg is SocketUserMessage message)
                {
                    if (message.Author is SocketGuildUser user)
                    {
                        var allMessageLoggings = _mongo.GetCollection<MessageLoggings>(_client);

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
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }
    }
}