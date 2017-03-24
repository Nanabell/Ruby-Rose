using Discord;
using Discord.Commands;
using RubyRose.Common.Preconditions;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RubyRose.Modules.Rwby
{
    [Name("RWBY")]
    public class UncleCommand : ModuleBase
    {
        [Command("Uncle")]
        [RequireAllowed, Ratelimit(1, 30, Measure.Seconds)]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Uncle()
        {
            var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);
            do
            {
                direc = direc.Parent;
            }
            while (direc.Name != "Ruby Rose");
            await Context.Channel.SendFileAsync($"{direc.FullName}/Data/uncle.gif");
        }
    }
}