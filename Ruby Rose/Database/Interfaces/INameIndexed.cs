namespace RubyRose.Database.Interfaces
{
    public interface INameIndexed : IIndexed
    {
        string Name { get; set; }
    }
}