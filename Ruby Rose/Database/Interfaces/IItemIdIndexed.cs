namespace RubyRose.Database.Interfaces
{
    public interface IItemIdIndexed : IIndexed
    {
        int ItemId { get; set; }
    }
}