using Discord.Commands;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class BuildCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<string> DotnetBuild(string configuration, string verbosity)
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
                    Logger.Error(error);
                }

                if (!Regex.IsMatch(report, @"Build \w+\.")) return report;
                var m = Regex.Match(report, @"(Build \w+\.)");

                return $"{report.Substring(m.Groups[1].Index)}";
            }
            else return "Failed to start dotnet build process.";
        }

        [Command("Build")]
        [RequireOwner]
        public async Task Build(string configuration = "debug", string verbosity = "q")
        {
            var report = await DotnetBuild(configuration, verbosity);
            Console.WriteLine(report);
        }
    }
}