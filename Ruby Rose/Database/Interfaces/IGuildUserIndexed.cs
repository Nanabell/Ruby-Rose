namespace RubyRose.Database.Interfaces
{
    public interface IGuildUserIndexed : IGuildIndexed
    {
        ulong UserId { get; set; }
    }
}