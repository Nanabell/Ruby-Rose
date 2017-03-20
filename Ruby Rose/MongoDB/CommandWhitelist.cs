using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RubyRose.MongoDB
{
    public class CommandWhitelist
    {
        [BsonElement("_id")]
        public ObjectId Id { get; internal set; }
        public string Name { get; set; }
        public List<ulong> WhitelistedChannelIds { get; set; }
    }
}