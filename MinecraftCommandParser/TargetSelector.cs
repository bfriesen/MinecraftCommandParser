using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Sprache;

namespace MinecraftCommandParser
{
    public abstract class TargetSelector : IFormattable
    {
        private static readonly char[] _legalVariables = { 'p', 'r', 'a', 'e' };

        private readonly char _variable;
        private readonly IReadOnlyDictionary<TargetSelectorArgument, object> _arguments;

        protected TargetSelector(char variable, IDictionary<TargetSelectorArgument, object> arguments)
        {
            if (!_legalVariables.Contains(variable)) throw new ArgumentException("variable must be 'p', 'r', 'a', or 'e'.", "variable");

            _variable = variable;
            _arguments = new ReadOnlyDictionary<TargetSelectorArgument, object>(arguments);
        }

        private static TargetSelector GetTargetSelector(
            char variable,
            IDictionary<TargetSelectorArgument, object> arguments)
        {
            switch (variable)
            {
                case 'p':
                    return new PlayerSelector(arguments);
                case 'r':
                    return new RandomPlayerSelector(arguments);
                case 'a':
                    return new AllPlayersSelector(arguments);
                case 'e':
                    return new EntitySelector(arguments);
                default:
                    throw new ArgumentException("variable must be 'p', 'r', 'a', or 'e'.", "variable");
            }
        }

        public IReadOnlyDictionary<TargetSelectorArgument, object> Arguments { get { return _arguments; } }

        internal static Parser<TargetSelector> GetParser()
        {
            return GetParser<TargetSelector>(_legalVariables);
        }

        internal static Parser<TTargetSelector> GetParser<TTargetSelector>(params char[] variables)
            where TTargetSelector : TargetSelector
        {
            if (variables == null) throw new ArgumentNullException("variables");

            if (variables.Length == 0)
            {
                variables = _legalVariables;
            }
            else if (variables.Any(v => !_legalVariables.Contains(v)))
            {
                throw new ArgumentException("variables may only contain 'p', 'r', 'a', or 'e'.", "variables");
            }
            else if (variables.Distinct().Count() != variables.Length)
            {
                throw new ArgumentException("variables may not contain duplicates.", "variables");
            }

            foreach (var variable in variables)
            {
                switch (variable)
                {
                    case 'p':
                        if (!typeof(TTargetSelector).IsAssignableFrom(typeof(PlayerSelector)))
                        {
                            throw new ArgumentException(string.Format("TTargetSelector type, {0}, is not assignable to PlayerSelector type.", typeof(TTargetSelector)), "variables");
                        }
                        break;
                    case 'r':
                        if (!typeof(TTargetSelector).IsAssignableFrom(typeof(RandomPlayerSelector)))
                        {
                            throw new ArgumentException(string.Format("TTargetSelector type, {0}, is not assignable to RandomPlayerSelector type.", typeof(TTargetSelector)), "variables");
                        }
                        break;
                    case 'a':
                        if (!typeof(TTargetSelector).IsAssignableFrom(typeof(AllPlayersSelector)))
                        {
                            throw new ArgumentException(string.Format("TTargetSelector type, {0}, is not assignable to AllPlayersSelector type.", typeof(TTargetSelector)), "variables");
                        }
                        break;
                    case 'e':
                        if (!typeof(TTargetSelector).IsAssignableFrom(typeof(EntitySelector)))
                        {
                            throw new ArgumentException(string.Format("TTargetSelector type, {0}, is not assignable to EntitySelector type.", typeof(TTargetSelector)), "variables");
                        }
                        break;
                }
            }

            var variableParser = Parse.Chars(variables);
            var argumentParser = GetArgumentParser();

            return
                from w1 in Parse.WhiteSpace.Many()
                from at in Parse.Char('@')
                from variable in variableParser
                from arguments in
                    (from openBracket in Parse.Char('[')
                     from items in argumentParser.OptionallyDelimitedBy(Parse.Char(',').Token()).Token()
                     from closeBracket in Parse.Char(']')
                     select items.ToDictionary(x => x.Key, x => x.Value)).Optional()
                select (TTargetSelector)GetTargetSelector(variable, arguments.GetOrElse(new Dictionary<TargetSelectorArgument, object>()));
        }

        private static Parser<KeyValuePair<TargetSelectorArgument, object>> GetArgumentParser()
        {
            var argumentParser =
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

        public string PrettyPrinted
        {
            get { return GetString(", "); }
        }

        public string Minified
        {
            get { return GetString(","); }
        }

        private string GetString(string separator)
        {
            var sb = new StringBuilder();

            sb.Append("@").Append(_variable);

            var arguments = Arguments.ToList();

            if (arguments.Any())
            {
                sb.Append("[");

                sb.Append(arguments[0].Key).Append("=").Append(arguments[0].Value);

                for (int i = 1; i < arguments.Count; i++)
                {
                    sb.Append(separator).Append(arguments[i].Key).Append("=").Append(arguments[i].Value);
                }

                sb.Append("]");
            }

            return sb.ToString();
        }
    }
}