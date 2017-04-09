using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public interface IGuildNameIndexed : IGuildIndexed
    {
        string Name { get; set; }
    }
}