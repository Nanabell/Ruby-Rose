using MongoDB.Bson;
using RubyRose.Common;
using System.Text;

namespace RubyRose.Database
{
    public class Settings : IGuildOneIndexed
    {
        public ObjectId _id { get; set; }

        public ulong GuildId { get; set; }

        public bool ResultAnnounce { get; set; } = true;

        public bool RwbyFight { get; set; } = true;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine($"GuildId:         {GuildId}");
            sb.AppendLine($"Result Announce: {ResultAnnounce}");
            sb.AppendLine($"Rwby Fight:      {RwbyFight}");
            sb.AppendLine("````");
            return sb.ToString();
        }
    }
}