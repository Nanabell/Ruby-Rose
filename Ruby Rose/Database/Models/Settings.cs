using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Settings : IGuildFirstIndexed
    {
        public ObjectId Id { get; set; }

        public ulong GuildId { get; set; }

        public bool ResultAnnounce { get; set; } = true;

        public bool RwbyFight { get; set; } = true;

        public List<string> BadWords { get; set; } = new List<string>(0);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine($"GuildId:         {GuildId}");
            sb.AppendLine($"Result Announce: {ResultAnnounce}");
            sb.AppendLine($"Rwby Fight:      {RwbyFight}");
            sb.AppendLine($"Bad Word Filter:");
            foreach (var word in BadWords)
                sb.AppendLine($"    {word}");
            sb.AppendLine("````");
            return sb.ToString();
        }
    }
}