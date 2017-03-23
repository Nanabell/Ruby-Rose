using Discord;
using Discord.Commands;
using RubyRose.Common.Preconditions;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RubyRose.Modules.Rwby
{
    [Name("RWBY")]
    public class WShrugCommand : ModuleBase
    {
        [Command("WShrug")]
        [RequireAllowed, Ratelimit(5, 30, Measure.Seconds)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task WSchrug()
        {
            var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);
            do
            {
                direc = direc.Parent;
            }
            while (direc.Name != "Ruby Rose");
            await Context.Channel.SendFileAsync($"{direc.FullName}/Data/weiss-shrug.png");
        }
    }
}