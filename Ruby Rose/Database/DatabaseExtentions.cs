using Discord;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database
{
    public static class DatabaseExtentions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static string _fallbackName;

        public static void LoadFallbackName(string name)
        {
            _fallbackName = name;
        }

        public static IMongoCollection<T> GetCollection<T>(this MongoClient mongo, IDiscordClient client = null)
        {
            string dbName;
            if (client?.CurrentUser == null)
                dbName = _fallbackName ?? throw new ArgumentNullException(nameof(mongo));
            else
                dbName = client.CurrentUser.Username.Replace(" ", "");

            Logger.Trace($"Connecting to Database {dbName}");
            var db = mongo.GetDatabase(dbName);
            Logger.Trace($"Connected to Database {dbName}");

            Logger.Trace($"Loading Collection {typeof(T).Name}");
            return db.GetCollection<T>(typeof(T).Name);
        }

        public static async Task<T> GetFirstAsync<T>(this IMongoCollection<T> collection) where T : IIndexed
        {
            var cursor = await collection.FindAsync("{}");
            Logger.Debug($"Returning first document in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<DeleteResult> DeleteAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            Logger.Info($"Deleting document with Id {entity.Id} in collection {collection.CollectionNamespace}");
            return await collection.DeleteOneAsync(i => i.Id == entity.Id);
        }

        public static async Task<ReplaceOneResult> SaveAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            Logger.Info($"Updating document with Id {entity.Id} in collection {collection.CollectionNamespace}");
            return await collection.ReplaceOneAsync(i => i.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, IGuild guild, string name) where T : IGuildNameIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            Logger.Debug($"Returning first document where name={name} & GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<List<T>> GetListAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IGuildIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            Logger.Debug($"Returning list of document where GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.ToListAsync();
        }

        public static async Task<T> GetByGuildAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IGuildOneIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            Logger.Debug($"Returning first document where GuildId={guild.Id} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, string name) where T : INameIndexed
        {
            var cursor = await collection.FindAsync(f => f.Name == name);
            Logger.Debug($"Returning first document where name={name} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByMessageIdAsyc<T>(this IMongoCollection<T> collection, IGuild guild, ulong messageId) where T : IGuildIdIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.MessageId == messageId);
            Logger.Debug($"Returning first document where messageId={messageId} in collection {collection.CollectionNamespace}");
            return await cursor.FirstOrDefaultAsync();
        }
    }
}