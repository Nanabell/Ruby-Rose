using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using System;
using MoreLinq;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation"), Group]
    public class PruneCommand : ModuleBase
    {
        [Command("Prune")]
        [Summary("Prune the Chat of some dirty danglers")]
        [MinPermission(AccessLevel.ServerModerator), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Prune(int count, IGuildUser user = null)
        {
            var internalCount = count;
            var deletedmsgs = 0;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            int runs = count / 100;
            if (runs == 0) runs = 1;
            IMessage lastmsg = null;
            do
            {
                IEnumerable<IMessage> messages;
                if (lastmsg != null)
                {
                    messages = await Context.Channel.GetMessagesAsync(lastmsg, Direction.After, (internalCount > 100 ? 100 : internalCount))
                        .Flatten();
                }
                else
                {
                    messages = await Context.Channel.GetMessagesAsync((internalCount > 100 ? 100 : internalCount)).Flatten();
                }
                var msgs = messages as IList<IMessage> ?? messages.ToList();

                lastmsg = msgs.Last();

                if (user != null)
                {
                    deletedmsgs += msgs.Where(x => x.Author.Id == user.Id).Count();
                    await Context.Channel.DeleteMessagesAsync(msgs.Where(x => x.Author.Id == user.Id));
                }
                else
                {
                    deletedmsgs += msgs.Count;
                    await Context.Channel.DeleteMessagesAsync(msgs);
                }
                runs--;
                internalCount = internalCount - msgs.Count;
            }
            while (runs > 0);
            await Context.Channel.SendEmbedAsync(Embeds.Success("Success", $"{deletedmsgs} Messages deleted!"));
        }
    }
}