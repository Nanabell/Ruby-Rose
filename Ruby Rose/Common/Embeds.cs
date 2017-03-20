using Discord;

namespace RubyRose.Common
{
    public static class Embeds
    {
        public static EmbedBuilder NotFound(string description)
        {
            var embed = new EmbedBuilder
            {
                Title = "Not Found!",
                Description = description,
                Color = new Color(0xE010B0)
            };
            return embed;
        }

        public static EmbedBuilder Exeption(string description)
        {
            var embed = new EmbedBuilder
            {
                Title = "Exeption!",
                Description = description,
                Color = new Color(0xE010B0)
            };
            return embed;
        }

        public static EmbedBuilder Success(string title, string description)
        {
            var embed = new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = new Color(0xE010B0)
            };
            return embed;
        }

        public static EmbedBuilder Invalid(string description)
        {
            var embed = new EmbedBuilder
            {
                Title = "Invalid!",
                Description = description,
                Color = new Color(0xE010B0)
            };
            return embed;
        }

        public static EmbedBuilder UnmetPrecondition(string description)
        {
            var embed = new EmbedBuilder
            {
                Title = "Unmet Precondition!",
                Description = description,
                Color = new Color(0xE010B0)
            };
            return embed;
        }
    }
}