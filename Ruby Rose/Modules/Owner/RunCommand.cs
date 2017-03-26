using Discord.Commands;
using RubyRose.Common;
using Serilog;
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
    public class BuildCommand : ModuleBase
    {
        public static async Task<string> dotnetBuild(string configuration, string verbosity)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build -c {configuration} -v {verbosity}",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            if (proc.Start())
            {
                var report = await proc.StandardOutput.ReadToEndAsync();
                var error = await proc.StandardError.ReadToEndAsync();

                if (error != "")
                {
                    Log.Fatal(error);
                }

                if (Regex.IsMatch(report, @"Build \w+\."))
                {
                    Match m = Regex.Match(report, @"(Build \w+\.)");

                    return $"{report.Substring(m.Groups[1].Index)}";
                }
                return report;
            }
            else return "Failed to start dotnet build process.";
        }

        [Command("Build")]
        [RequireOwner]
        public async Task Build(string configuration = "debug", string verbosity = "q")
        {
            var report = await dotnetBuild(configuration, verbosity);
            Console.WriteLine(report);
        }
    }
}