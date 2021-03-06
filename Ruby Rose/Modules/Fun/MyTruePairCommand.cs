﻿using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Fun
{
    [Name("Fun"), Group]
    public class MyTruePairCommand : ModuleBase
    {
        [Command("MyTruePair"), Alias("MTP")]
        [Summary("Let me guess your one true partner")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        public async Task MyTruePair()
        {
            await ReplyAsync("wip");
        }
    }
}