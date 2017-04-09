using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace RubyRose.Database
{
    public class OneTruePairs : IGuildOneIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}