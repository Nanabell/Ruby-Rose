﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using NLog;
using Discord;
using System.Collections.Generic;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class TagCommand : ModuleBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public TagCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Tag"), Alias("Tags")]
        [Summary("Display all Tags that exist")]
        [MinPermission(AccessLevel.User), RequireAllowed]
        public async Task Tag()
        {
            var sb = new StringBuilder();
            var alltags = _mongo.GetCollection<Tags>(Context.Client);
            var tags = await alltags.GetListAsync(Context.Guild);
            logger.Debug($"[TagSystem] Loaded {tags.Count} Tags");

            sb.AppendLine("**Tags**:");
            sb.AppendLine(string.Join(" ", tags.OrderBy(x => x.Name).Select(x => $"`{x.Name}`")));
            await ReplyAsync($"{sb}");
        }

        [Command("Tag")]
        [Summary("Use simply keywords to display a custom set response")]
        [MinPermission(AccessLevel.User), RequireAllowed]
        public async Task Tag([Remainder] string name)
        {
            name = name.ToLower();
            var alltags = _mongo.GetCollection<Tags>(Context.Client);
            var tag = await alltags.GetByNameAsync(Context.Guild, name);

            if (tag != null)
            {
                tag.LastUsed = DateTime.UtcNow;
                tag.Uses++;
                await alltags.SaveAsync(tag);

                await ReplyAsync(tag.Content);
            }
            else
            {
                await ReplyAsync($"Tag {name} not existing");
                logger.Warn($"[TagSystem] Tag {name} on {Context.Guild} not existent");
            }
        }
    }
}