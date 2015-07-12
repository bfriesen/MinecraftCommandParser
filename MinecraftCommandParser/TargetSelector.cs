using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sprache;

namespace MinecraftCommandParser
{
    public class TargetSelector
    {
        private readonly IReadOnlyDictionary<TargetSelectorArgument, object> _arguments;

        public TargetSelector(TargetSelectorVariable variable, IDictionary<TargetSelectorArgument, object> arguments)
        {
            Variable = variable;
            _arguments = new ReadOnlyDictionary<TargetSelectorArgument, object>(arguments);
        }

        public TargetSelectorVariable Variable { get; private set; }
        public IReadOnlyDictionary<TargetSelectorArgument, object> Arguments { get { return _arguments; } }

        internal static Parser<TargetSelector> GetParser()
        {
            var targetSelectorVariableParser = CommandParsers.GetEnumParser<TargetSelectorVariable>();
            var argumentParser = GetArgumentParser();

            return
                from w1 in Parse.WhiteSpace.Many()
                from at in Parse.Char('@')
                from variable in targetSelectorVariableParser
                from arguments in
                    (from openBracket in Parse.Char('[')
                        from items in CommandParsers.OptionallyDelimitedBy(argumentParser, Parse.Char(',').Token()).Token()
                        from closeBracket in Parse.Char(']')
                        select items.ToDictionary(x => x.Key, x => x.Value)).Optional()
                select new TargetSelector(variable, arguments.GetOrElse(new Dictionary<TargetSelectorArgument, object>()));
        }

        private static Parser<KeyValuePair<TargetSelectorArgument, object>> GetArgumentParser()
        {
            Parser<KeyValuePair<TargetSelectorArgument, object>> argumentParser =
                from argument in Parse.Char('x')
                from eq in Parse.Char('=').Token()
                from value in CommandParsers.CoordinateParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.x, value);

            argumentParser = argumentParser.Or(
                from argument in Parse.Char('y')
                from eq in Parse.Char('=').Token()
                from value in CommandParsers.CoordinateParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.y, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.Char('z')
                from eq in Parse.Char('=').Token()
                from value in CommandParsers.CoordinateParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.z, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.Char('r')
                from eq in Parse.Char('=').Token()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.r, Int32.Parse(value)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("rm")
                from eq in Parse.Char('=').Token()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.rm, Int32.Parse(value)));

            var modeParser = CommandParsers.GetRangeParser(-1, 5);

            argumentParser = argumentParser.Or(
                from argument in Parse.String("m")
                from eq in Parse.Char('=').Token()
                from value in modeParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.m, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.Char('c')
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.c, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.Char('l')
                from eq in Parse.Char('=').Token()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.l, Int32.Parse(value)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("lm")
                from eq in Parse.Char('=').Token()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.lm, Int32.Parse(value)));

            var stringValueParser = Parse.CharExcept(c => c == ']' || c == ',' || Char.IsWhiteSpace(c), "string values").AtLeastOnce().Text();

            argumentParser = argumentParser.Or(
                from argument in Parse.String("team")
                from eq in Parse.Char('=').Token()
                from not in Parse.Char('!').Token()
                from value in stringValueParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.not_team, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("team")
                from eq in Parse.Char('=').Token()
                from value in stringValueParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.team, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("team")
                from eq in Parse.Char('=').Token()
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.not_any_team, null));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("name")
                from eq in Parse.Char('=').Token()
                from not in Parse.Char('!').Token()
                from value in stringValueParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.not_name, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("name")
                from eq in Parse.Char('=').Token()
                from value in stringValueParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.name, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("dx")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.dx, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("dy")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.dy, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("dz")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.dz, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("rx")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.rx, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("rxm")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.rxm, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("ry")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.ry, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("rym")
                from eq in Parse.Char('=').Token()
                from negative in Parse.Char('-').Optional()
                from value in Parse.Number
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.rym, Int32.Parse(value) * (negative.IsDefined ? -1 : 1)));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("type")
                from eq in Parse.Char('=').Token()
                from not in Parse.Char('!').Token()
                from value in CommandParsers.EntityParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.not_type, value));

            argumentParser = argumentParser.Or(
                from argument in Parse.String("type")
                from eq in Parse.Char('=').Token()
                from value in CommandParsers.EntityParser
                select new KeyValuePair<TargetSelectorArgument, object>(TargetSelectorArgument.type, value));

            return argumentParser;
        }
    }
}