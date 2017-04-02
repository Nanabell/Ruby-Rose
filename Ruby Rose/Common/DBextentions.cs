using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Common
{
    public interface IIndexed
    {
        ObjectId _id { get; set; }
        ulong GuildId { get; set; }
    }

    public interface IOneIndexed
    {
        ObjectId _id { get; }
        ulong GuildId { get; set; }
        string Name { get; set; }
    }

    public static class DBextentions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static IMongoCollection<T> GetCollection<T>(this MongoClient mongo, IDiscordClient client)
        {
            logger.Trace($"[Database] Connecting to Database {client.CurrentUser.Username.Replace(" ", "")}");
            var db = mongo.GetDatabase(client.CurrentUser.Username.Replace(" ", ""));
            logger.Trace($"[Database] Connected to {db.DatabaseNamespace}");
            logger.Trace($"[Database] Loading Collection {typeof(T).Name}");
            return db.GetCollection<T>(typeof(T).Name);
        }

        public static async Task<T> FirstAsync<T>(this IMongoCollection<T> collection) where T : IIndexed
        {
            logger.Trace($"[Database] Loading Documents for {collection.CollectionNamespace}");
            var cursor = await collection.FindAsync("{}");
            logger.Trace($"[Database] Finding First Document in Collection");
            return await cursor.FirstAsync();
        }

        public static async Task<ReplaceOneResult> SaveAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            logger.Debug($"[Database] Adding/Replaceing document with ID {entity._id}");
            return await collection.ReplaceOneAsync(
                i => i._id == entity._id,
                entity,
                new UpdateOptions { IsUpsert = true });
        }

        public static async Task<DeleteResult> DeleteAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            logger.Debug($"[Database] Deleting document with Id {entity._id}");
            return await collection.DeleteOneAsync(
                i => i._id == entity._id);
        }

        public static async Task<T> GetOneAsync<T>(this IMongoCollection<T> collection, IGuild guild, string name) where T : IOneIndexed
        {
            logger.Trace($"[Database] Loading Documents for {collection.CollectionNamespace}");
            var TCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            logger.Trace($"[Database] Filtered Documents by Guild Id {guild.Id} & Name {name}");
            return await TCursor.FirstOrDefaultAsync();
        }

        public static async Task<List<T>> GetListAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IIndexed
        {
            logger.Trace($"[Database] Loading Documents for {collection.CollectionNamespace}");
            var TCursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            logger.Trace($"[Database] Filtered Documents by Guild Id {guild.Id}");
            return await TCursor.ToListAsync();
        }
    }
}