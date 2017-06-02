using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class JoinCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public JoinCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
        }

        [Command("Join")]
        [Summary("Join roles that are marked as joinable.")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task Join([Remainder] string input)
        {
            var names = input.ToLower().Split(' ');
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);
            var user = Context.User as SocketGuildUser;
            if (user == null) throw new NullReferenceException(nameof(user));

            IUserMessage msg = null;

            var embed = new EmbedBuilder
            {
                Title = "Joining Roles",
                Color = new Color(user.GetColorFromUser()),
                Description = "```diff\n"
            };

            var _ = Task.Run(async () =>
            {
                if (names.Contains("all"))
                {
                    foreach (var joinable in joinables.Where(joinable => joinable.Level == 0))
                    {
                        var role = user.Guild.GetRole(joinable.RoleId);

                        if (user.Roles.All(userRole => userRole.Id != role.Id))
                        {
                            if (msg == null)
                                msg = await Context.ReplyAsync($"Adding requested roles. This may take a while please be patient.");
                            embed.Description += $"+ {role}\n";
                            await user.AddRoleAsync(role);
                            await Task.Delay(1000);
                        }
                        else
                        {
                            embed.Description += $"- {role} | you already have the role\n";
                        }
                    }
                }
                else
                {
                    foreach (var name in names)
                    {
                        if (joinables.Any(joinable => joinable.Name == name))
                        {
                            var first = joinables.First(joinable => joinable.Name == name);
                            var equalLevel = joinables.Where(joinable => joinable.Level != 0 && joinable.Level == first.Level);
                            var role = user.Guild.GetRole(first.RoleId);

                            if (role != null)
                            {
                                if (user.Roles.All(userRole => userRole.Id != role.Id))
                                {
                                    if (!user.Roles.Any(userRole => equalLevel.Any(joinable => userRole.Id == joinable.RoleId)))
                                    {
                                        if (msg == null)
                                            msg = await Context.ReplyAsync($"Adding requested roles. This may take a while please be patient.");
                                        embed.Description += $"+ {role}\n";
                                        await user.AddRoleAsync(role);
                                        await Task.Delay(1000);
                                    }
                                    else
                                    {
                                        var sameLvlRole =
                                            user.Roles.First(
                                                socketRole => equalLevel.Any(joinable => joinable.RoleId == socketRole.Id));
                                        embed.Description += $"- {role} | you already have {sameLvlRole}\n";
                                    }
                                }
                                else
                                {
                                    embed.Description += $"- {role} | you already have the role\n";
                                }
                            }
                            else
                            {
                                embed.Description += $"- {name} | Role no longer existent in guild";
                            }

                        }
                        else
                        {
                            embed.Description += $"- {name} | Role not found\n";
                        }
                    }
                }
                embed.Description += "```";
                if (msg != null)
                    await msg.DeleteAsync();
                await Context.ReplyAsync(embed);
            });
        }
    }
}