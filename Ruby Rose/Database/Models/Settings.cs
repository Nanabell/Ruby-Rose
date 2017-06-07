using System.Text;
using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.Database.Models
{
    public class Settings : IGuildFirstIndexed
    {
        public Settings(ulong guildId)
        {
            GuildId = guildId;
        }

        public ObjectId Id { get; set; }

        public ulong GuildId { get; set; }

        public bool IsErrorReporting { get; set; } = true;

        public bool RwbyFight { get; set; } = true;

        public ulong OtpRoleId { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine($"GuildId:         {GuildId}");
            sb.AppendLine($"Result Announce: {IsErrorReporting}");
            sb.AppendLine($"Rwby Fight:      {RwbyFight}");
            sb.AppendLine("```");
            return sb.ToString();
        }
    }
}