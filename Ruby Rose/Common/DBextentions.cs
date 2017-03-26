using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Common
{
    public static class DBextentions
    {
        public static IMongoCollection<T> GetDiscordDb<T>(this MongoClient mongo, string name)
        {
            var db = mongo.GetDatabase("Discord");
            var collection = db.GetCollection<T>(name);
            return collection;
        }
    }
}