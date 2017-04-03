using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Database;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubyRose.Custom_Reactions
{
    public static class RwbyFight
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ConcurrentDictionary<ulong, bool> WeissF = new ConcurrentDictionary<ulong, bool>();
        private static ConcurrentDictionary<ulong, bool> RubyF = new ConcurrentDictionary<ulong, bool>();

        public static async Task MessageHandler(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var user = message.Author as SocketGuildUser;
            if (user == null) return;
            var guild = user.Guild;
            if (guild == null) return;

            SettingsManager.CustomReactions.TryGetValue(guild.Id, out var temp);
            if (temp)
            {
                if (Regex.IsMatch(arg.Content, "<:Heated2:\\d+>"))
                {
                    if (WeissF.Contains(new KeyValuePair<ulong, bool>(arg.Channel.Id, true)))
                    {
                        var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);

                        do
                        {
                            direc = direc.Parent;
                        }
                        while (direc.Name != "Ruby Rose");
                        logger.Info("[CustomReactions] Triggered Rwby Fight Gif");
                        await arg.Channel.SendFileAsync($"{direc.FullName}/Data/rwby-fight.gif");
                        WeissF.TryRemove(arg.Channel.Id, out temp);
                        RubyF.TryRemove(arg.Channel.Id, out temp);
                    }
                    else
                    {
                        RubyF.GetOrAdd(arg.Channel.Id, true);
                    }
                }
                else if (Regex.IsMatch(arg.Content, "<:Heated1:\\d+>"))
                {
                    if (RubyF.Contains(new KeyValuePair<ulong, bool>(arg.Channel.Id, true)))
                    {
                        var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);

                        do
                        {
                            direc = direc.Parent;
                        }
                        while (direc.Name != "Ruby Rose");
                        logger.Info("[CustomReactions] Triggered Rwby Fight Gif");
                        await arg.Channel.SendFileAsync($"{direc.FullName}/Data/rwby-fight.gif");
                        WeissF.TryRemove(arg.Channel.Id, out temp);
                        RubyF.TryRemove(arg.Channel.Id, out temp);
                    }
                    else
                    {
                        WeissF.GetOrAdd(arg.Channel.Id, true);
                    }
                }
                else
                {
                    WeissF.TryRemove(arg.Channel.Id, out temp);
                    RubyF.TryRemove(arg.Channel.Id, out temp);
                }
            }
        }
    }
}