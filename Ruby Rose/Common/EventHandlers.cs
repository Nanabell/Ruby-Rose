using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RubyRose.Custom_Reactions;
using RubyRose.Database;
using System;
using System.Threading.Tasks;

namespace RubyRose.Common
{
    public class EventHandlers
    {
        private readonly DiscordSocketClient _client;
        private readonly Credentials _credentials;

        public EventHandlers(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _credentials = map.Get<Credentials>();
        }

        public void Install()
        {
            _client.Log += Logging.Log;
            _client.Ready += Ready;
            _client.MessageReceived += RwbyFight.MessageHandler;
        }

        private async Task Ready()
        {
            await _client.SetGameAsync(_credentials.NowPlaying);
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