using MongoDB.Bson;
using RubyRose.Common;

namespace RubyRose.Database
{
    public class Whitelists : IIndexed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}