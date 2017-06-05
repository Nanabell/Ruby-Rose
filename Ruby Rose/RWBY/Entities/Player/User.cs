using System.Linq;
using System.Threading.Tasks;
using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using RubyRose.Database;
using RubyRose.Database.Interfaces;
using RubyRose.RWBY.Entities.Item;

namespace RubyRose.RWBY.Entities.Player
{
    public class User : GameBase, IGuildUserIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool IsSpecial { get; set; }
        public int Dust { get; set; }
        public int WeaponId { get; set; }

        /// <summary>
        /// Get the IGuild from the current User Object
        /// </summary>
        /// <returns>IGuild Guild</returns>
        internal IGuild GetGuild()
            => Client.GetGuild(GuildId);

        /// <summary>
        /// Get the IUser from the current User Object
        /// </summary>
        /// <returns>IUser User</returns>
        internal async Task<IGuildUser> GetGuildUserAsync()
            => await GetGuild().GetUserAsync(UserId);

        /// <summary>
        /// Get the Weapon the User currently has. Does not use Database! Weapons must be loaded into Memory first!
        /// </summary>
        /// <returns>The Weapon</returns>
        internal Weapon GetWeapon()
            => Weapons.FirstOrDefault(weapon => weapon.ItemId == WeaponId);

        /// <summary>
        /// Get a User from a UserId
        /// </summary>
        /// <param name="userId">UserId of the User</param>
        /// <returns>Database User</returns>
        public static async Task<User> GetUserAsync(ulong userId)
            => await MongoClient.GetCollection<User>(Client).FirstOrDefaultAsync(user => user.UserId == userId);

        /// <summary>
        /// Load all Users from the Database into local Memory.
        /// </summary>
        /// <returns></returns>
        public static async Task LoadUsersAsync()
        {
            var allUsers = await MongoClient.GetCollection<User>(Client).All();
            Users.AddRange(allUsers);
        }

        /// <summary>
        /// Save any Changes to the current User to the Database.
        /// </summary>
        /// <returns></returns>
        internal async Task SaveUserAsync()
        {
            if (Id != ObjectId.Empty)
            {
                var allUsers = MongoClient.GetCollection<User>(Client);
                await allUsers.SaveAsync(this);
            }
            else
            {
                await InsertUserAsync();
            }
        }

        /// <summary>
        /// Save all Users to the Database
        /// </summary>
        /// <returns></returns>
        public static async Task SaveUsersAsync()
        {
            var allUsers = MongoClient.GetCollection<User>(Client);

            foreach (var user in Users)
            {
                if (user.Id != ObjectId.Empty)
                {
                    await allUsers.SaveAsync(user);
                }
                else
                {
                    await InsertUserAsync(user);
                }
            }
        }

        private async Task InsertUserAsync()
        {
            var allUsers = MongoClient.GetCollection<User>(Client);
            await allUsers.InsertOneAsync(this);
        }

        private static async Task InsertUserAsync(User user)
        {
            var allUsers = MongoClient.GetCollection<User>(Client);
            await allUsers.InsertOneAsync(user);
        }

    }
}