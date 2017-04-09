using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public interface INameIndexed : IIndexed
    {
        string Name { get; set; }
    }
}