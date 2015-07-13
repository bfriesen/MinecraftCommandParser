using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace MinecraftCommandParser
{
    internal static class CommandParsers
    {
        public static readonly Parser<string> StringParser;

        public static readonly Parser<Asset> EntityParser;
        public static readonly Parser<Asset> BlockParser;
        public static readonly Parser<Asset> ItemParser;
        public static readonly Parser<Player> PlayerParser;
        public static readonly Parser<DataTag> DataTagParser;
        public static readonly Parser<Coordinate> CoordinateParser;
        public static readonly Parser<Location> LocationParser;
        public static readonly Parser<TargetSelector> TargetSelectorParser;

        public static readonly Parser<FillCommand> FillCommandParser;
        public static readonly Parser<GiveCommand> GiveCommandParser;
        public static readonly Parser<SayCommand> SayCommandParser;
        public static readonly Parser<SummonCommand> SummonCommandParser;

        public static readonly Parser<IFormattable> CommandParser;

        static CommandParsers()
        {
            StringParser = GetStringParser();

            EntityParser = Asset.GetEntityParser();
            BlockParser = Asset.GetBlockParser();
            ItemParser = Asset.GetItemParser();
            PlayerParser = Player.GetParser();
            DataTagParser = DataTag.GetParser();
            CoordinateParser = Coordinate.GetParser();
            LocationParser = Location.GetParser();
            TargetSelectorParser = TargetSelector.GetParser();

            FillCommandParser = FillCommand.GetParser();
            GiveCommandParser = GiveCommand.GetParser();
            SummonCommandParser = SummonCommand.GetParser();
            SayCommandParser = SayCommand.GetParser();

            CommandParser =
                from w1 in Parse.WhiteSpace.Many()
                from slash in Parse.Char('/').Optional()
                from command in
                    FillCommandParser.Select(c => (IFormattable)c)
                        .Or(GiveCommandParser)
                        .Or(SummonCommandParser)
                        .Or(SayCommandParser)
                from w2 in Parse.WhiteSpace.Many().End()
                select command;
        }

        private static Parser<string> GetStringParser()
        {
            Predicate<char> isUnquotedStringCharacter = c =>
            {
                switch (c)
                {
                    case ',':
                    case ']':
                    case '}':
                        return false;
                    default:
                        return true;
                }
            };

            var unquotedStringParser =
                from value in Parse.Char(isUnquotedStringCharacter, "unquoted string characters").AtLeastOnce().Text()
                select value.TrimEnd();

            var escapedQuote =
                from backslash in Parse.Char('\\')
                from q in Parse.Char('"')
                select q;

            var unescapedChar = Parse.CharExcept('"');

            var quotedStringParser =
                from q1 in Parse.Char('"')
                from value in escapedQuote.Or(unescapedChar).Many().Text()
                from q2 in Parse.Char('"')
                select value;

            return quotedStringParser.Or(unquotedStringParser);
        }

        public static Parser<int> GetRangeParser(int start, int count)
        {
            if (count < 1) throw new ArgumentException("count must be greater than zero", "count");

            Parser<int> parser = null;

            var unorderedRange = Enumerable.Range(start, count).ToArray();

            var range =
                unorderedRange.Where(x => x < 0).OrderBy(x => x)
                .Concat(unorderedRange.Where(x => x >= 0).OrderByDescending(x => x));

            foreach (var i in range)
            {
                var localValue = i;
                var localStringValue = i.ToString();
                var p =
                    from s in Parse.String(localStringValue)
                    select localValue;

                if (parser == null)
                {
                    parser = p;
                }
                else
                {
                    parser = parser.Or(p);
                }
            }

            return parser;
        }

        public static Parser<T> GetEnumParser<T>(params T[] except)
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must enum type");

            Parser<T> parser = null;

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (except.Contains(value))
                {
                    continue;
                }

                var localValue = value;
                var stringValue = value.ToString();

                var p =
                    from s in Parse.String(stringValue)
                    select localValue;

                parser = parser == null ? p : parser.Or(p);
            }

            return parser;
        }

        public static Parser<IEnumerable<T>> OptionallyDelimitedBy<T, U>(this Parser<T> parser, Parser<U> delimiter)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            if (delimiter == null) throw new ArgumentNullException("delimiter");

            return from head in parser.Once().Optional()
                   from tail in
                       (from separator in delimiter
                        from item in parser
                        select item).Many()
                   select head.IsDefined ? head.Get().Concat(tail) : Enumerable.Empty<T>();
        }
    }
}