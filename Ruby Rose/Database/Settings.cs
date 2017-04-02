using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RubyRose.Database
{
    public class Settings : IIndexed
    {
        public ObjectId _id { get; set; }

        public ulong GuildId { get; set; }

        [BsonDefaultValue(true)]
        public bool ExecutionErrorAnnounce { get; set; }

        [BsonDefaultValue(true)]
        public bool CustomReactions { get; set; }
    }
}