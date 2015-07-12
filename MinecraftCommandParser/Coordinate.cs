using System;
using Sprache;

namespace MinecraftCommandParser
{
    public class Coordinate
    {
        public Coordinate(double value, bool isRelative)
        {
            Value = value;
            IsRelative = isRelative;
        }

        public double Value { get; private set; }
        public bool IsRelative { get; private set; }

        public override string ToString()
        {
            return IsRelative
                ? "~" + (Value == 0 ? "" : Value.ToString())
                : Value.ToString();
        }

        internal static Parser<Coordinate> GetParser()
        {
            var relativeParser =
                from tilde in Parse.Char('~')
                from negative in Parse.Char('-').Optional()
                from value in Parse.DecimalInvariant
                select new Coordinate(Double.Parse(value) * (negative.IsDefined ? -1 : 1), true);

            var tildeParser =
                from tilde in Parse.Char('~')
                select new Coordinate(0, true);

            var absoluteParser =
                from negative in Parse.Char('-').Optional()
                from value in Parse.DecimalInvariant
                select new Coordinate(Double.Parse(value) * (negative.IsDefined ? -1 : 1), false);


            return relativeParser.Or(tildeParser).Or(absoluteParser);
        }
    }
}