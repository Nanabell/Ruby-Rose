﻿using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation")]
    public class ConfigCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ConfigCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Config")]
        [MinPermission(AccessLevel.ServerModerator)]
        public async Task Config()
        {
            var settings = await _mongo.GetCollection<Settings>(Context.Client).GetByGuildAsync(Context.Guild);

            await Context.ReplyAsync($"Current Settings for {Context.Guild.Name}\n{settings.ToString()}");
        }
    }
}