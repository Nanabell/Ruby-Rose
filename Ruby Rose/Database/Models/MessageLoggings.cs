using System;
using System.Collections.Generic;
using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    internal class MessageLoggings : IGuildMessageIdIndexed
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public List<string> Edits { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> AttachmentUrls { get; set; }
    }
}