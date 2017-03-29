using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RubyRose.Database
{
    public class DatabaseModel
    {
        public ulong Id { get; set; }

        [BsonIgnoreIfNull]
        public List<Users> User { get; set; }

        [BsonIgnoreIfNull]
        public List<Tags> Tag { get; set; }

        [BsonIgnoreIfNull]
        public List<Commands> Command { get; set; }

        [BsonIgnoreIfNull]
        public List<Joinables> Joinable { get; set; }

        [BsonIgnoreIfNull]
        public Roles OneTruePair { get; set; }
    }

    public class Users
    {
        public ulong Id { get; set; }
        public bool IsSpecial { get; set; }
        public uint Money { get; set; }
    }

    public class Tags
    {
        public string Name { get; set; }
        public int Uses { get; set; }
        public ulong CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        public string Response { get; set; }
    }

    public class Commands
    {
        public Whitelists Whitelist { get; set; }
        public Blacklists Blacklist { get; set; }

        public class Whitelists
        {
            [BsonIgnoreIfNull]
            public List<Users> User { get; set; }

            [BsonIgnoreIfNull]
            public List<Channels> Channel { get; set; }

            public class Users
            {
                public ulong Id { get; set; }
            }

            public class Channels
            {
                public ulong Id { get; set; }
            }
        }

        public class Blacklists
        {
            [BsonIgnoreIfNull]
            public List<Users> User { get; set; }

            [BsonIgnoreIfNull]
            public List<Channels> Channel { get; set; }

            public class Users
            {
                public ulong Id { get; set; }
            }

            public class Channels
            {
                public ulong Id { get; set; }
            }
        }
    }

    public class Joinables
    {
        public string Keyword { get; set; }
        public Roles Role { get; set; }
        public int Level { get; set; }
    }

    public class Roles
    {
        public ulong Id { get; set; }
    }
}