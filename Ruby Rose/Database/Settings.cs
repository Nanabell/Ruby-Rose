using MongoDB.Bson;
using RubyRose.Common;

namespace RubyRose.Database
{
    public class Settings : IIndexed
    {
        public ObjectId _id { get; set; }

        public ulong GuildId { get; set; }

        public bool ExecutionErrorAnnounce { get; set; }

        public bool CustomReactions { get; set; }
    }
}