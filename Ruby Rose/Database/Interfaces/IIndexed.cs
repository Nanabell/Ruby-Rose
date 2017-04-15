using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RubyRose.Database.Interfaces
{
    public interface IIndexed
    {
        [BsonElement("_id")]
        ObjectId Id { get; set; }
    }
}