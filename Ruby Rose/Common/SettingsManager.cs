using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Common
{
    public static class SettingsManager
    {
        private static DiscordSocketClient _client;
        private static MongoClient _mongo;

        public static ConcurrentDictionary<ulong, bool> CustomReactions = new ConcurrentDictionary<ulong, bool>();
        public static ConcurrentDictionary<ulong, bool> ResultAnnounce = new ConcurrentDictionary<ulong, bool>();

        public static Task Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _mongo = map.Get<MongoClient>();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Load settings from the database to local
        /// </summary>
        public async static Task LoadSettings()
        {
            var allSettings = _mongo.GetCollection<Settings>(_client);
            var settingsList = await allSettings.Find("{}").ToListAsync();

            foreach (var settings in settingsList)
            {
                CustomReactions.AddOrUpdate(settings.GuildId, settings.CustomReactions, (key, oldvalue) => settings.CustomReactions);
                ResultAnnounce.AddOrUpdate(settings.GuildId, settings.ExecutionErrorAnnounce, (key, oldvalue) => settings.ExecutionErrorAnnounce);
            }
        }

        /// <summary>
        /// Save local settings to the Database
        /// </summary>
        public async static Task SaveSettings()
        {
            var allSettings = _mongo.GetCollection<Settings>(_client);
            var settingsList = await allSettings.Find("{}").ToListAsync();

            foreach (var settings in settingsList)
            {
                var cr = CustomReactions.GetOrAdd(settings.GuildId, settings.CustomReactions);
                var ra = ResultAnnounce.GetOrAdd(settings.GuildId, settings.ExecutionErrorAnnounce);

                if (ra != settings.ExecutionErrorAnnounce || cr != settings.CustomReactions)
                {
                    settings.CustomReactions = cr;
                    settings.ExecutionErrorAnnounce = ra;

                    await allSettings.SaveAsync(settings);
                }
            }
        }
    }
}