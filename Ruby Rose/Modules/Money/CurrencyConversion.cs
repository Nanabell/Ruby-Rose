using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Modules.Money
{
    public static class CurrencyConversion
    {
        public static decimal Convert(CurrencyType from, CurrencyType to, decimal ammount)
        {
            var multiplier = (int)from;
            var dividor = (int)to;

            return ammount * multiplier / dividor;
        }
    }
}