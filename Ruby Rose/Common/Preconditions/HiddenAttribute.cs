using System.Threading.Tasks;
using Discord.Commands;

namespace RubyRose.Common.Preconditions
{
    public class HiddenAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IDependencyMap map) => Task.FromResult(PreconditionResult.FromSuccess());
    }
}