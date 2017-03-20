using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("Owner")]
    public class PullCommand : ModuleBase
    {
        private async Task<string> GitPull(string branch)
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
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                var error = await proc.StandardError.ReadToEndAsync();
                var report = await proc.StandardOutput.ReadToEndAsync();

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