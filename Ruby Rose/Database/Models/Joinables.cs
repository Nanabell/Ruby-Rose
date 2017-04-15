using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Joinables : IGuildNameIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public ulong RoleId { get; set; }
        public int Level { get; set; }
    }
}