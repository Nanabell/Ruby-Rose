using Discord.Commands;
using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class PullCommand : ModuleBase
    {
        public static async Task<string> GitPull(string branch)
        {
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"pull https://github.com/Nanabell/Ruby-Rose.git {branch}",
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../"),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            if (proc.Start())
            {
                var error = await proc.StandardError.ReadToEndAsync();
                var report = await proc.StandardOutput.ReadToEndAsync();

                if (error != null)
                {
                    if (Regex.IsMatch(error, "Couldn't find remote ref"))
                        return error.Substring(7);
                    Logging.LogMessage("Error", "Dotnet", error);
                }

                if (Regex.IsMatch(report, "Already up-to-date"))
                {
                    return "Already up-to-date.";
                }
                else
                {
                    return report;
                }
            }
            else return "Failed to start git pull process.";
        }

        [Command("pull")]
        [RequireOwner]
        public async Task Pull(string branch = "master")
        {
            var result = await GitPull(branch);
            Console.WriteLine(result);
        }
    }
}