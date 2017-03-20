using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class RestartCommand : ModuleBase
    {
        [Command("Restart")]
        [RequireOwner]
        public async Task Restart()
        {
            await ReplyAsync("*Restarting...*");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"dotnet",
                    Arguments = "run",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardError = true
                }
            };

            proc.Start();
            Environment.Exit(0);
        }

        public static void restart()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"dotnet",
                    Arguments = "run",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardError = true
                }
            };

            proc.Start();
            Environment.Exit(0);
        }
    }
}