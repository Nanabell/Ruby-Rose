using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using NLog;

namespace RubyRose.Modules.System
{
    [Name("System")]
    public class RestoreCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<string> DotnetRestore(string verbosity)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"restore --verbosity {verbosity}",
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../"),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            if (!proc.Start()) return "Failed to start restore process.";
            var report = await proc.StandardOutput.ReadToEndAsync();
            var error = await proc.StandardError.ReadToEndAsync();
            if (error != null)
            {
                Logger.Error(error);
            }

            if (!Regex.IsMatch(report, @"Restore completed in \d.+? sec")) return report;
            var rTime = Regex.Match(report, @"Restore completed in (\d.+?) sec").Groups[1].Value;

            return Regex.IsMatch(report, @"Lock file has not changed. Skipping lock file write.") ? $"Lock file has not changed. Skipping lock file write.\nRestore completed in {rTime} sec" : report;
        }

        [Command("Restore")]
        [RequireOwner]
        public async Task Restore(string verbosity = "m")
        {
            var report = await DotnetRestore(verbosity);
            Console.WriteLine(report);
        }
    }
}