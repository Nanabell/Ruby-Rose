using Discord;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RubyRose.Database.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using RubyRose.Database;

namespace RubyRose.RWBY.Entities.Item
{
    [BsonIgnoreExtraElements]
    public class Weapon : GameBase, IItemIdIndexed
    {
        //TODO: Implement Multiplier Calculation => "Multiplier = Math.Pow(Math.Log(BaseDamage + 5), 4) / (2 * Math.Pow(Math.Log(10), 2));"

        public ObjectId Id { get; set; }
        public int ItemId { get; set; }
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Rarity Rarity { get; set; }
        public string ImageUrl { get; set; }
        public double Worth { get; set; }
        public int Level { get; set; }
        public int UpgradeLevel { get; set; }
        public double BaseDamage { get; set; }
        public double Multiplier { get; set; }


        /// <summary>
        /// Get the Weapon from the Database
        /// </summary>
        /// <param name="weaponId">The Weapon Id</param>
        /// <returns>Weapon Object</returns>
        public static async Task<Weapon> GetWeaponAsync(int weaponId)
            => await MongoClient.GetCollection<Weapon>(Client).FirstOrDefaultAsync(weapon => weapon.ItemId == weaponId);

        /// <summary>
        /// Load all Weapons from the Database
        /// </summary>
        /// <returns></returns>
        public static async Task LoadWeaponsAsync()
        {
            var allWeapons = await MongoClient.GetCollection<Weapon>(Client).All();
            Weapons.AddRange(allWeapons);
        }

        /// <summary>
        /// Save all Weapons to the Database
        /// </summary>
        /// <returns></returns>
        public static async Task SaveWeaponsAsync()
        {
            var allWeapons = MongoClient.GetCollection<Weapon>(Client);

            foreach (var weapon in Weapons)
            {
                if (weapon.Id != ObjectId.Empty)
                {
                    await allWeapons.SaveAsync(weapon);
                }
                else
                {
                    await InsertWeaponAsync(weapon);
                }
            }
        }

        /// <summary>
        /// Save the Weapon to the Database
        /// </summary>
        /// <returns></returns>
        internal async Task SaveWeaponAsync()
        {
            if (Id != ObjectId.Empty)
            {
                var allWeapons = MongoClient.GetCollection<Weapon>(Client);
                await allWeapons.SaveAsync(this);
            }
            else
            {
                await InsertWeaponAsync();
            }
        }

        /// <summary>
        /// Get a Count with how many Users have this exact Weapon
        /// </summary>
        /// <returns>int ExistingCount</returns>
        internal int GetExistingCount()
            => Weapons.Count(weapon => weapon.ItemId == ItemId);

        /// <summary>
        /// Get a Random Damage value between Min and Max
        /// </summary>
        /// <returns>Damage value</returns>
        internal double GetDamage()
            => BaseDamage + Multiplier + GetInternalMultiplier();

        /// <summary>
        /// Get the Damage value for a Critial-Hit
        /// </summary>
        /// <returns>Critial Damage value</returns>
        internal double GetCritialDamage()
        {
            var dmg = GetDamage();
            return dmg + dmg * 0.5;
        }

        /// <summary>
        /// Calculate the Multiplier for the BaseDamage and return it.
        /// </summary>
        /// <returns>Multiplier value</returns>
        internal double GetMultiplier()
            => Math.Pow(Math.Log(BaseDamage + 5), 4) / (2 * Math.Pow(Math.Log(10), 2));

        /// <summary>
        /// Get the Maximum Damage possible
        /// </summary>
        /// <returns>Max Damage</returns>
        internal double GetMaxDamage()
            => BaseDamage + Multiplier * 1.75;

        /// <summary>
        /// Get the Minimum Damage possible
        /// </summary>
        /// <returns>Min Damage</returns>
        internal double GetMinDamage()
            => BaseDamage;

        /// <summary>
        /// Get as Embed to send to Discord
        /// </summary>
        /// <returns>EmbedBuilder</returns>
        internal EmbedBuilder ToEmbed()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"{Name}  + {UpgradeLevel}");
            embed.WithDescription(Description);

            embed.AddField("Required Level", Level);
            embed.AddField("Damage", $"{GetMinDamage()} - {GetMaxDamage()}");

            embed.WithThumbnailUrl(ImageUrl);

            embed.WithFooter(builder => { builder.Text = $"Sellable for {Worth}"; });

            return embed;
        }

        private async Task InsertWeaponAsync()
        {
            var allWeapons = MongoClient.GetCollection<Weapon>();
            await allWeapons.InsertOneAsync(this);
        }

        private static async Task InsertWeaponAsync(Weapon weapon)
        {
            var allWeapons = MongoClient.GetCollection<Weapon>();
            await allWeapons.InsertOneAsync(weapon);
        }

        private static double GetInternalMultiplier()
        {
            var rnd = new Random();
            return (double)rnd.Next(50, 176) / 100;
        }
    }
}