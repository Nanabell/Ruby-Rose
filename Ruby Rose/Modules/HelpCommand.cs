using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules
{
    [Name("Help"), Group]
    public class HelpCommand : ModuleBase
    {
        private readonly CommandService _service;
        private readonly IDependencyMap _map;
        private readonly Credentials _credentials;

        public HelpCommand(CommandService service, IDependencyMap map)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _credentials = map.Get<Credentials>();
            _map = map;
        }

        [Command("Help")]
        [Summary("Display what commands you can use"), Hidden]
        public async Task Help()
        {
            var cmdgrps = (await _service.Commands.CheckConditionsAsync(Context, _map))
                .Where(c => !c.Preconditions.Any(p => p is HiddenAttribute))
                .GroupBy(c => (c.Module.IsSubmodule ? c.Module.Parent.Name : c.Module.Name));

            var sb = new StringBuilder();
            sb.AppendLine("You can use the following commands:\n");

            foreach (var group in cmdgrps)
            {
                var commands = group.Select(commandInfo => commandInfo.Module.IsSubmodule
                        ? $"`{commandInfo.Module.Name}*`"
                        : $"`{commandInfo.Name}`")
                    .ToList();

                sb.AppendLine($"**{group.Key}**: {string.Join(" ", commands.Distinct())}");
            }
            sb.AppendLine(
                $"\nYou can use `{_credentials.Prefix}Help <command>` for more information on that command.");

            await ReplyAsync($"{sb}");
        }

        [Command("Help")]
        [Summary("Display how you can use a command"), Hidden]
        public async Task Help(string commandName)
        {
            var commands = (await _service.Commands.CheckConditionsAsync(Context, _map)).Where(
                c => (c.Aliases.FirstOrDefault().Equals(commandName, StringComparison.OrdinalIgnoreCase)) ||
                     (c.Module.IsSubmodule && c.Module.Aliases.FirstOrDefault()
                          .Equals(commandName, StringComparison.OrdinalIgnoreCase)) &&
                     !c.Preconditions.Any(p => p is HiddenAttribute));

            var sb = new StringBuilder();
            var commandInfos = commands as IList<CommandInfo> ?? commands.ToList();
            if (commandInfos.Any())
            {
                sb.AppendLine($"{commandInfos.Count} {(commandInfos.Count > 1 ? "entries" : "entry")} for `{commandName}`");
                sb.AppendLine("```cs");

                foreach (var command in commandInfos)
                {
                    sb.AppendLine("Usage");
                    sb.AppendLine(
                        $"\t{_credentials.Prefix}{(command.Module.IsSubmodule ? $"{command.Module.Name} " : "")}{command.Name} " +
                        string.Join(" ", command.Parameters.Select(FormatParam)).Replace("`", ""));
                    sb.AppendLine("Summary");
                    sb.AppendLine($"\t{command.Summary ?? "No Summary"}");
                }
                sb.AppendLine("```");
                await ReplyAsync($"{sb}");
            }
            else
            {
                await Context.Channel.SendEmbedAsync(
                    Embeds.NotFound($"Unable to find any comamnd matching `{commandName}`"));
            }
        }

        private static string FormatParam(ParameterInfo parameter)
        {
            var sb = new StringBuilder();
            if (parameter.IsMultiple)
            {
                sb.Append($"`[({parameter.Type.Name}): {parameter.Name}...]`");
            }
            else if (parameter.IsRemainder)
            {
                sb.Append($"`<({parameter.Type.Name}): {parameter.Name}...>`");
            }
            else if (parameter.IsOptional)
            {
                sb.Append($"`[({parameter.Type.Name}): {parameter.Name}]`");
            }
            else
            {
                sb.Append($"`<({parameter.Type.Name}): {parameter.Name}>`");
            }

            if (!string.IsNullOrWhiteSpace(parameter.Summary))
            {
                sb.Append($" ({parameter.Summary})");
            }
            return sb.ToString();
        }
    }
}