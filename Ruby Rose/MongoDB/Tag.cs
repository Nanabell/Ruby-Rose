using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RubyRose.MongoDB
{
    public class Tag
    {
        [BsonElement("_id")]
        public ObjectId Id { get; internal set; }
        public string TagName { get; set; }
        public int Uses { get; set; }
        public ulong Creator { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Response { get; set; }
    }
}