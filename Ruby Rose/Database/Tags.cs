using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace RubyRose.Database
{
    public class Tags : IIndexed, INameIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int Uses { get; set; }
        public DateTime CreatedAt { get; set; }
        public ulong CreatedBy { get; set; }
        public DateTime LastUsed { get; set; }
    }
}