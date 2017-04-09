using MongoDB.Bson;
using RubyRose.Common;

namespace RubyRose.Database
{
    public class Settings : IGuildOneIndexed
    {
        public ObjectId _id { get; set; }

        public ulong GuildId { get; set; }

        public bool ResultAnnounce { get; set; }

        public bool RwbyFight { get; set; }
    }
}