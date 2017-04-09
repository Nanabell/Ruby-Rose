using RubyRose.Common;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace RubyRose.Database
{
    internal class Services : IGuildNameIndexed
    {
        public ObjectId _id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}