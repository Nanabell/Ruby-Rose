using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RubyRose.Modules.RoleSystem.Db
{
    public class JoinSystemSerializer
    {
        [BsonElement("_id")]
        public ObjectId Id { get; internal set; }
        public string Keyword { get; set; }
        public Role Role { get; set; }
    }

    public class Role
    {
        public string Name { get; set; }
        public ulong Id { get; set; }
    }
}