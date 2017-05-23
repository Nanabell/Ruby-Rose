using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Database;
using System.Threading.Tasks;
using RubyRose.Database.Models;
using RubyRose.RWBY.Entities.Player;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RubyRose.Common.Preconditions
{
    public class RequireMoneyAttribute : PreconditionAttribute
    {
        private readonly int _dust;

        /// <summary>
        /// Require a Set Ammount of Currency to Execute this Command.
        /// </summary>
        /// <param name="money">Required Currency</param>
        public RequireMoneyAttribute(int dust)
        {
            _dust = dust;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var mongo = provider.GetService<MongoClient>();
            var dust = GetUserDust(context, mongo);

            if (dust >= _dust)
            {
                var newMoney = dust - _dust;
                Task.Run(async () => await SetUserDust(context, mongo, newMoney));
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else return Task.FromResult(PreconditionResult.FromError($"Missing {_dust - dust} Dust to perform this Command"));
        }

        /// <summary>
        /// Return the Currency of the current User
        /// </summary>
        /// <returns>Currency as int</returns>
        private static int GetUserDust(ICommandContext context, MongoClient mongo)
        {
            var allUsers = mongo.GetCollection<User>(context.Client);
            var user = allUsers.Find(g => g.GuildId == context.Guild.Id && g.UserId == context.User.Id).FirstOrDefault();

            return user?.Dust ?? 0;
        }

        /// <summary>
        /// Save the new Currency count to the Database.
        /// </summary>
        private static async Task SetUserDust(ICommandContext context, MongoClient mongo, int newMoney)
        {
            var allUsers = mongo.GetCollection<User>(context.Client);
            var user = allUsers.Find(g => g.GuildId == context.Guild.Id && g.UserId == context.User.Id).FirstOrDefault();

            user.Dust = newMoney;

            await allUsers.SaveAsync(user);
        }
    }
}