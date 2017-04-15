namespace RubyRose.Database.Interfaces
{
    public interface IGuildIdIndexed : IGuildIndexed
    {
        ulong MessageId { get; set; }
    }
}