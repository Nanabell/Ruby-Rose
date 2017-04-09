﻿using Discord.Commands;
using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Owner
{
    [Name("System")]
    public class UpdateCommand : ModuleBase
    {
        [Command("Update")]
        [RequireOwner]
        public async Task Update(string branch = "master", string config = "debug", string verbosity = "q")
        {
            var msg = await Context.Channel.SendEmbedAsync(Embeds.Success("***Updating...***", "Running: Git Pull"));
            var pull = await PullCommand.GitPull(branch);
            if (pull.Contains("Couldn't find remote ref"))
                await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Invalid(pull)));
            else if (pull != "Already up-to-date.")
            {
                await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Success("***Updating...***", pull)));

                var restore = await RestoreCommand.dotnetRestore(verbosity);
                await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Success("***Updating...***", restore)));

                var build = await BuildCommand.dotnetBuild(config, verbosity);
                await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Success("***Updating...***", build)));
                await Task.Delay(500);

                await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Success("***Updating...***", "***RESTARTING***")));
                await Task.Delay(200);
                RestartCommand.Restart();
            }
            else await msg.ModifyAsync(modi => modi.Embed = new Discord.Optional<Discord.Embed>(Embeds.Invalid("Already up-to-date")));
        }
    }
}