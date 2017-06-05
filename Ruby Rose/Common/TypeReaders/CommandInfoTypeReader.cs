using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Common.TypeReaders
{
    internal class CommandInfoTypeReader : TypeReader
    {
        private readonly CommandService _service;

        public CommandInfoTypeReader(CommandService service)
        {
            _service = service;
        }

        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var result = _service.Search(context, input);

            return Task.FromResult(result.IsSuccess
                ? TypeReaderResult.FromSuccess(result.Commands.FirstOrDefault().Command)
                : TypeReaderResult.FromError(CommandError.ParseFailed, $"Command {input} not found"));
        }
    }
}