using MongoDB.Bson;
using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public class Blacklists : IIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public ulong ChannelId { get; set; }
    }
}