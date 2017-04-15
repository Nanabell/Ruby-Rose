namespace RubyRose.Database.Interfaces
{
    public interface IGuildIndexed : IIndexed
    {
        ulong GuildId { get; set; }
    }
}