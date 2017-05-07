using System.Collections.Generic;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.RWBY.Entities.Item;
using RubyRose.RWBY.Entities.Player;

namespace RubyRose.RWBY
{
    public abstract class GameBase
    {
        public static MongoClient MongoClient;
        public static DiscordSocketClient Client;

        internal static readonly List<Weapon> Weapons = new List<Weapon>();
        internal static readonly List<User> Users = new List<User>();
        //TODO: Decide if you really want to keep this
    }
}