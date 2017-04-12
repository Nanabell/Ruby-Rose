using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public interface IGuildIdIndexed : IGuildIndexed
    {
        ulong MessageId { get; set; }
    }
}