using MongoDB.Bson;
using RubyRose.Database.Interfaces;

namespace RubyRose.RWBY.Entities.Player
{
    public class ActiveQuests : IIndexed
    {
        public ObjectId Id { get; set; }
        public ulong UserId { get; set; }

        //TODO: MAKE THIS A THING
    }
}