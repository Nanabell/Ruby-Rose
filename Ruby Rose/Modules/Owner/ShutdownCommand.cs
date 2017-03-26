﻿using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("Owner")]
    public class ShutdownCommand : ModuleBase
    {
        [Command("Shutdown")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await ReplyAsync("***SHUTDWON***");
            Environment.Exit(0);
        }
    }
}