using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using RubyRose.Database.Models;
using System.Text;
using System.IO;

namespace RubyRose.Modules.Logging
{
    [Name("Logging")]
    public class ChannelLogs : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ChannelLogs(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("ChannelLogs")]
        [MinPermission(AccessLevel.ServerAdmin)]
        public async Task Invoke(IMessageChannel channel = null, int count = 1000)
        {
            try { await Context.Message.DeleteAsync(); }
            catch
            {
                // ignored
            }
            channel = channel ?? Context.Channel;
            var allLogs = _mongo.GetCollection<MessageLoggings>(Context.Client);
            var channelLogs = await GetChannelLogs(allLogs, channel);
            var logs = channelLogs.OrderByDescending(x => x.Timestamp).Take(count);
            var sb = new StringBuilder();

            foreach (var channelLog in logs)
            {
                var user = await Context.Guild.GetUserAsync(channelLog.UserId);

                sb.Append($"[{channelLog.Timestamp}] {$"{(user != null ? $"{user}" : "Unknown User")} ({channelLog.UserId})".PadRight(53)} ");

                if (channelLog.IsDeleted)
                    sb.Append("[DELETED] ");

                sb.AppendLine(channelLog.Content);

                if (channelLog.IsEdited)
                {
                    foreach (var edit in channelLog.Edits)
                    {
                        sb.AppendLine($"[EDIT] {edit}");
                    }
                }

                if (!channelLog.AttachmentUrls.Any()) continue;
                foreach (var attachment in channelLog.AttachmentUrls)
                {
                    sb.AppendLine($"[ATTACHMENT] {attachment}");
                }
            }
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            var dmchannel = await Context.User.CreateDMChannelAsync();
            await dmchannel.SendFileAsync(stream, $"{channel.Name}-logs.log");
        }

        private static async Task<List<MessageLoggings>> GetChannelLogs(IMongoCollection<MessageLoggings> collection,
            IMessageChannel channel)
        {
            var cursor = await collection.FindAsync(f => f.ChannelId == channel.Id);
            return await cursor.ToListAsync();
        }
    }
}