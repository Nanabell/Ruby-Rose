using System;
using System.Collections.Generic;
using System.Text;

namespace RubyRose.Modules.Money
{
    public enum CurrencyType
    {
        /// <summary>
        /// Just a small Vial of powdered Dust.
        /// </summary>
        VialOfDust = 1,

        /// <summary>
        /// A normal sized tube of powdered Dust.
        /// </summary>
        TubeOfDust = 10,

        /// <summary>
        /// A large sized tube of powdered Dust.
        /// </summary>
        LargeTubeOfDust = 50,

        /// <summary>
        /// A small Dust Crystal.
        /// </summary>
        SmallCrystal = 100,

        /// <summary>
        /// A normal Dust Crystal.
        /// </summary>
        NormalCrystal = 250,

        /// <summary>
        /// A large Dust Crystal.
        /// </summary>
        LargeCrystal = 500,

        /// <summary>
        /// A large Pure and Burned Crystal. This is the purest form of Dust.
        /// </summary>
        BurnedCrystal = 1000
    }
}