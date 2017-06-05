using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RubyRose.Modules.System
{
    [Name("System")]
    public class ShutdownCommand : ModuleBase
    {
        [Command("Shutdown")]
        [RequireOwner]
        public async Task Shutdown()
        {
            await ReplyAsync("***SHUTDOWN***");
            Environment.Exit(0);
        }
    }
}