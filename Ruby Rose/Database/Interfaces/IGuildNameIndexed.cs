namespace RubyRose.Database.Interfaces
{
    public interface IGuildNameIndexed : IGuildIndexed
    {
        string Name { get; set; }
    }
}