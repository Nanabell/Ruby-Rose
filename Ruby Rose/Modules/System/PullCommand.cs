﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using NLog;

namespace RubyRose.Modules.System
{
    [Name("System")]
    public class PullCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

                if (error == null) return Regex.IsMatch(report, "Already up-to-date") ? "Already up-to-date." : report;
                if (!Regex.IsMatch(error, "Couldn't find remote ref"))
                    return Regex.IsMatch(report, "Already up-to-date") ? "Already up-to-date." : report;
                Logger.Warn(error);
                return error.Substring(7);
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