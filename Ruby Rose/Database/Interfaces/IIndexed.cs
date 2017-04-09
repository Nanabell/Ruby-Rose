using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Database
{
    public interface IIndexed
    {
        ObjectId _id { get; set; }
    }
}