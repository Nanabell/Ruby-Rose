using System;
using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Tags : IGuildNameIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int Uses { get; set; }
        public DateTime CreatedAt { get; set; }
        public ulong CreatedBy { get; set; }
        public DateTime LastUsed { get; set; }
    }
}