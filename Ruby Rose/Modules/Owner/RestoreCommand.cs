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
    public class RestoreCommand : ModuleBase
    {
        public static async Task<string> dotnetRestore(string verbosity)
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

            if (proc.Start())
            {
                var report = await proc.StandardOutput.ReadToEndAsync();
                var error = await proc.StandardError.ReadToEndAsync();
                if (error != null)
                {
                    Logging.LogMessage("Error", "Dotnet", error);
                }

                if (Regex.IsMatch(report, @"Restore completed in \d.+? sec"))
                {
                    string rTime = Regex.Match(report, @"Restore completed in (\d.+?) sec").Groups[1].Value;

                    if (Regex.IsMatch(report, @"Lock file has not changed. Skipping lock file write."))
                    {
                        return $"Lock file has not changed. Skipping lock file write.\nRestore completed in {rTime} sec";
                    }
                    else return report;
                }
                else return report;
            }
            else return "Failed to start restore process.";
        }

        [Command("Restore")]
        [RequireOwner]
        public async Task Restore(string verbosity = "m")
        {
            var report = await dotnetRestore(verbosity);
            Console.WriteLine(report);
        }
    }
}