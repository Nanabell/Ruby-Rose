using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.EmojiTools;
using Discord;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using MoreLinq;
using System.Collections.Concurrent;
using Serilog;

namespace RubyRose.Custom_Reactions
{
    public static class RwbyFight
    {
        private static bool temp;
        private static ConcurrentDictionary<ulong, bool> WeissF = new ConcurrentDictionary<ulong, bool>();
        private static ConcurrentDictionary<ulong, bool> RubyF = new ConcurrentDictionary<ulong, bool>();

        public static async Task MessageHandler(SocketMessage arg)
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
                    Log.Information("Triggered Rwby Fight Gif");
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
                    Log.Information("Triggered Rwby Fight Gif");
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