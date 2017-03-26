using Discord;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Common
{
    public static class DBextentions
    {
        public static IMongoCollection<DatabaseModel> GetDiscordDb(this MongoClient mongo, IDiscordClient client)
        {
            var db = mongo.GetDatabase("Discord");
            var collection = db.GetCollection<DatabaseModel>(client.CurrentUser.Username);
            return collection;
        }
    }
}