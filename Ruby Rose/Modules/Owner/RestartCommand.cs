using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class RestartCommand : ModuleBase
    {
        [Command("Restart")]
        [RequireOwner]
        public async Task Restart()
        {
            await ReplyAsync("*Restarting...*");
            Environment.Exit(1);
        }
    }
}