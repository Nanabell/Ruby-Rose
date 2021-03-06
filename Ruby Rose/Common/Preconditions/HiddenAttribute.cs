﻿using System.Threading.Tasks;
using Discord.Commands;
using System;

namespace RubyRose.Common.Preconditions
{
    public class HiddenAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider map) => Task.FromResult(PreconditionResult.FromSuccess());
    }
}