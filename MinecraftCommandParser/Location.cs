using System;
using Sprache;

namespace MinecraftCommandParser
{
    public class Location
    {
        public Location(Coordinate x, Coordinate y, Coordinate z)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            if (z == null) throw new ArgumentNullException("z");

            X = x;
            Y = y;
            Z = z;
        }

        public Coordinate X { get; private set; }
        public Coordinate Y { get; private set; }
        public Coordinate Z { get; private set; }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", X, Y, Z);
        }

        internal static Parser<Location> GetParser()
        {
            return
                from w1 in Parse.WhiteSpace.Many()
                from x in CommandParsers.CoordinateParser
                from w2 in Parse.WhiteSpace.AtLeastOnce()
                from y in CommandParsers.CoordinateParser
                from w3 in Parse.WhiteSpace.AtLeastOnce()
                from z in CommandParsers.CoordinateParser
                select new Location(x, y, z);
        }
    }
}