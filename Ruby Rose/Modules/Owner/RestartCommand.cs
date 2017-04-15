using Discord.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class RestartCommand : ModuleBase
    {
        public static void Restart()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                }
            };

            if (proc.Start())
            {
                Environment.Exit(1);
            }
        }

        [Command("Restart")]
        [RequireOwner]
        public async Task RestartCmd()
        {
            await ReplyAsync("*Restarting...*");
            Restart();
        }
    }
}