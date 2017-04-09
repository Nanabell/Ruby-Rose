using Discord;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Database
{
    public static class DatabaseExtentions
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static string FallbackName;

        public static void LoadFallbackName(string name)
        {
            FallbackName = name;
        }

        public static IMongoCollection<T> GetCollection<T>(this MongoClient mongo, IDiscordClient client = null)
        {
            string dbName;
            if (client == null || client.CurrentUser == null)
                dbName = FallbackName ?? throw new ArgumentNullException("Not Connected to Discord and no Fallback name entered in config");
            else
                dbName = client.CurrentUser.Username.Replace(" ", "");

            _logger.Trace($"Connecting to Database {dbName}");
            var db = mongo.GetDatabase(dbName);
            _logger.Trace($"Connected to Database {dbName}");

            _logger.Trace($"Loading Collection {typeof(T).Name}");
            return db.GetCollection<T>(typeof(T).Name);
        }

        public static async Task<T> GetFirstAsync<T>(this IMongoCollection<T> collection) where T : IIndexed
        {
            var cursor = await collection.FindAsync("{}");
            _logger.Debug($"Returning first document in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<DeleteResult> DeleteAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            _logger.Info($"Deleting document with Id {entity._id} in collection {collection.CollectionNamespace}");
            return await collection.DeleteOneAsync(i => i._id == entity._id);
        }

        public static async Task<ReplaceOneResult> SaveAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            _logger.Info($"Updating document with Id {entity._id} in collection {collection.CollectionNamespace}");
            return await collection.ReplaceOneAsync(i => i._id == entity._id, entity, new UpdateOptions { IsUpsert = true });
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, IGuild guild, string name) where T : IGuildNameIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            _logger.Debug($"Returning first document where name={name} & GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<List<T>> GetListAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IGuildIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            _logger.Debug($"Returning list of document where GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.ToListAsync();
        }

        public static async Task<T> GetByGuildAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IGuildOneIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            _logger.Debug($"Returning first document where GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, string name) where T : INameIndexed
        {
            var cursor = await collection.FindAsync(f => f.Name == name);
            _logger.Debug($"Returning first document where name={name} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }
    }
}