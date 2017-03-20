using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RubyRose.Custom_Reactions;

namespace RubyRose.Common
{
    public class EventHandlers
    {
        public static DiscordSocketClient Client;

        public EventHandlers(IDependencyMap map)
        {
            Client = map.Get<DiscordSocketClient>();
        }

        public void Install()
        {
            Client.Log += Logging.Log;
            Client.Ready += ReadyEvent.Ready;
            Client.MessageReceived += RwbyFight.MessageHandler;
        }
    }

    public static class ReadyEvent
    {
        public static async Task Ready()
        {
            await EventHandlers.Client.SetGameAsync(Config.Settings.NowPlaying);
        }
    }

    public static class Logging
    {
        public static Task Log(LogMessage msg)
        {
            LogMessage(msg.Severity.ToString(), msg.Source, msg.Message);
            return Task.CompletedTask;
        }
        private static void Append(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(text);
        }

        private static void EndLine(string text = "", ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(text + Environment.NewLine);
        }

        public static void LogMessage(string level, string source, string message)
        {
            Append($"{DateTime.Now:hh:mm:ss} ", ConsoleColor.DarkGray);
            Append($"[{level}] ", ConsoleColor.Red);
            Append($"{source}: ", source == "Command" ? ConsoleColor.Cyan : ConsoleColor.DarkGreen);
            EndLine(message, ConsoleColor.White);
        }
    }
}