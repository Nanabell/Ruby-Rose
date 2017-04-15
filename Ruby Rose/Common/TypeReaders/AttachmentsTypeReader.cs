using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RubyRose.Common.TypeReaders
{
    public class AttachmentsTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var attachments = context.Message.Attachments;

            if (attachments.Count == 1)
            {
                var attachment = attachments.First();
                return Task.FromResult(TypeReaderResult.FromSuccess(attachment));
            }
            else if (attachments.Count > 1)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Message contained to many Attachments"));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Message contains no attachments."));
            }
        }
    }
}