using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Users : IGuildIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool IsSpecial { get; set; }
        public int Money { get; set; }
    }
}