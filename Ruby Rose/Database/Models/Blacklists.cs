using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Blacklists : IGuildIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public ulong ChannelId { get; set; }
    }
}