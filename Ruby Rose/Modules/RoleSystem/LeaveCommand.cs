﻿using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Linq;
using System.Threading.Tasks;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class LeaveCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public LeaveCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
        }

        [Command("Leave")]
        [Summary("Leavea role marked as Joinable")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Leave([Remainder] string input)
        {
            var names = input.ToLower().Split(' ');
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);
            var user = Context.User as SocketGuildUser;
            if (user == null) throw new NullReferenceException(nameof(user));

            IUserMessage msg = null;

            var embed = new EmbedBuilder
            {
                Title = "Leaving Roles",
                Color = new Color(user.GetColorFromUser()),
                Description = "```diff\n"
            };

            var _ = Task.Run(async () =>
            {
                if (names.Contains("all"))
                {
                    foreach (var joinable in joinables)
                    {
                        var role = user.Guild.GetRole(joinable.RoleId);

                        if (user.Roles.Any(userRole => userRole.Id == role.Id))
                        {
                            if (msg == null)
                                msg = await Context.ReplyAsync("Removing requested roles. This may take a while please be patient.");
                            embed.Description += $"+ {role}\n";
                            await user.RemoveRoleAsync(role);
                            await Task.Delay(1000);
                        }
                        else
                        {
                            embed.Description += $"- {role} | you dont have the role\n";
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
                            var role = user.Guild.GetRole(first.RoleId);

                            if (role != null)
                            {
                                if (user.Roles.Any(userRole => userRole.Id == role.Id))
                                {
                                    if (msg == null)
                                        msg = await Context.ReplyAsync("Removing requested roles. This may take a while please be patient.");
                                    embed.Description += $"+ {role}\n";
                                    await user.AddRoleAsync(role);
                                    await Task.Delay(1000);
                                }
                                else
                                {
                                    embed.Description += $"- {role} | you dont have the role\n";
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