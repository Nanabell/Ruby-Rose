using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RubyRose.Common.TypeReaders
{
    public class AttachmentsTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            return Task.FromResult(context.Message.Attachments.Any() ? TypeReaderResult.FromSuccess(context.Message.Attachments.First()) : TypeReaderResult.FromError(CommandError.ParseFailed, "Message contains no attachments."));
        }
    }
}