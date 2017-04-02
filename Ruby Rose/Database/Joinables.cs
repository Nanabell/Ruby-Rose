using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace RubyRose.Database
{
    public class Joinables : IIndexed, IOneIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public ulong RoleId { get; set; }
        public int Level { get; set; }
    }
}