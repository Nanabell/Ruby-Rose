using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Common.Preconditions
{
    public class RequireMoneyAttribute : PreconditionAttribute
    {
        private readonly int _money;

        /// <summary>
        /// Require a Set Ammount of Currency to Execute this Command.
        /// </summary>
        /// <param name="money">Required Currency</param>
        public RequireMoneyAttribute(int money)
        {
            _money = money;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var mongo = map.Get<MongoClient>();
            var money = GetUserMoney(context, mongo);

            if (money >= _money)
            {
                var newMoney = money - _money;
                Task.Run(async () => await SetUserMoneyAsync(context, mongo, newMoney));
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else return Task.FromResult(PreconditionResult.FromError($"Missing {_money - money} Dust to perform this Command"));
        }

        /// <summary>
        /// Return the Currency of the current User
        /// </summary>
        /// <returns>Currency as int</returns>
        private int GetUserMoney(ICommandContext context, MongoClient mongo)
        {
            var allUsers = mongo.GetCollection<Users>(context.Client);
            var user = allUsers.Find(g => g.GuildId == context.Guild.Id && g.UserId == context.User.Id).FirstOrDefault();

            if (user != null)
                return user.Money;
            else return 0;
        }

        /// <summary>
        /// Save the new Currency count to the Database.
        /// </summary>
        private async Task SetUserMoneyAsync(ICommandContext context, MongoClient mongo, int newMoney)
        {
            var allUsers = mongo.GetCollection<Users>(context.Client);
            var user = allUsers.Find(g => g.GuildId == context.Guild.Id && g.UserId == context.User.Id).FirstOrDefault();

            user.Money = newMoney;

            await allUsers.SaveAsync(user);
        }
    }
}