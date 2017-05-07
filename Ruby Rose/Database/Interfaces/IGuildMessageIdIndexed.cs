namespace RubyRose.Database.Interfaces
{
    public interface IGuildMessageIdIndexed : IGuildIndexed
    {
        ulong MessageId { get; set; }
    }
}