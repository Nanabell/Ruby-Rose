using Discord;
using Discord.Commands;
using RubyRose.Common.Preconditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Fun
{
    [Name("Fun")]
    public class SleepCommand : ModuleBase
    {
        [Command("Sleep")]
        [RequireAllowed, Ratelimit(1, 30, Measure.Seconds)]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Sleep()
        {
            var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);
            do
            {
                direc = direc.Parent;
            }
            while (direc.Name != "Ruby Rose");
            await Context.Channel.SendFileAsync($"{direc.FullName}/Data/sleep.png");
        }
    }
}