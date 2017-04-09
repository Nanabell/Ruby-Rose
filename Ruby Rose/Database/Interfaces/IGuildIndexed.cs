using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public interface IGuildIndexed : IIndexed
    {
        ulong GuildId { get; set; }
    }
}