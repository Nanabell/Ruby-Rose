using MongoDB.Bson;

namespace RubyRose.MongoDB
{
    public class LoginToken
    {
        public ObjectId Id { get; internal set; }
        public string Name { get; internal set; }
        public string Token { get; internal set; }
    }
}