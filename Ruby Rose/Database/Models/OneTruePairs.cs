using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class OneTruePairs : IGuildOneIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}