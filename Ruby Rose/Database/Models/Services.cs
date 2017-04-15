using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    internal class Services : IGuildNameIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}