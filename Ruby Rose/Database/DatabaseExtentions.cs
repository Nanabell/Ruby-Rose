using Discord;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RubyRose.Database.Interfaces;
using RubyRose.Database.Models;

namespace RubyRose.Database
{
    public static class DatabaseExtentions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IMongoCollection<T> GetCollection<T>(this MongoClient mongo, IDiscordClient client)
        {
            var dbName = client.CurrentUser.Username.Replace(" ", "");
            Logger.Trace($"Connecting to Database {dbName}");
            var db = mongo.GetDatabase(dbName);

            Logger.Trace($"Loading Collection {typeof(T).Name}");
            return db.GetCollection<T>(typeof(T).Name);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IMongoCollection<T> collection) where T : IIndexed
        {
            var cursor = await collection.FindAsync("{}");
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IIndexed
        {
            var cursor = await collection.FindAsync(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<DeleteResult> DeleteAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            return await collection.DeleteOneAsync(i => i.Id == entity.Id);
        }

        public static async Task<ReplaceOneResult> SaveAsync<T>(this IMongoCollection<T> collection, T entity) where T : IIndexed
        {
            return await collection.ReplaceOneAsync(i => i.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, IGuild guild, string name) where T : IGuildNameIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<List<T>> GetListAsync<T>(this IMongoCollection<T> collection, IGuild guild) where T : IGuildIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            return await cursor.ToListAsync();
        }

        public static async Task<T> GetByNameAsync<T>(this IMongoCollection<T> collection, string name) where T : INameIndexed
        {
            var cursor = await collection.FindAsync(f => f.Name == name);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByMessageIdAsyc<T>(this IMongoCollection<T> collection, IGuild guild,
            ulong messageId) where T : IGuildMessageIdIndexed
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.MessageId == messageId);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByIdAsync<T>(this IMongoCollection<T> collection, int id)
            where T : IItemIdIndexed
        {
            var cursor = await collection.FindAsync(f => f.ItemId == id);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<T> GetByUserIdAsync<T>(this IMongoCollection<T> collection, ulong userId)
            where T : IGuildUserIndexed
        {
            var cursor = await collection.FindAsync(f => f.UserId == userId);
            return await cursor.FirstOrDefaultAsync();
        }

        public static async Task<Settings> GetByGuildAsync(this IMongoCollection<Settings> collection, ulong guildId)
        {
            var cursor = await collection.FindAsync(f => f.GuildId == guildId);
            var result = await cursor.SingleOrDefaultAsync();

            if (result != null)
                return result;
            var newSettings = new Settings(guildId);
            await collection.InsertOneAsync(newSettings);

            var secondCursor = await collection.FindAsync(f => f.GuildId == guildId);
            return await secondCursor.SingleOrDefaultAsync();
        }

        public static async Task<List<Settings>> GetGuildsAsync(this IMongoCollection<Settings> collection)
        {
            var cursor = await collection.FindAsync("{}");
            return await cursor.ToListAsync();
        }


        public static async Task<List<T>> All<T>(this IMongoCollection<T> collection) where T : IIndexed
        {
            var cursor = await collection.FindAsync("{}");
            return await cursor.ToListAsync();
        }
    }
}