using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace RubyRose.Database
{
    public class Users : IIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool IsSpecial { get; set; }
        public int Money { get; set; }
    }
}