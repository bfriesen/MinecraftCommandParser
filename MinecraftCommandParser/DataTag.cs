using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Sprache;

namespace MinecraftCommandParser
{
    public class DataTag : IFormattable
    {
        private const int IndentSize = 4;

        public DataTag(IEnumerable<DataTagItem> attributes)
        {
            if (attributes == null) throw new ArgumentNullException("attributes");
            Attributes = new ReadOnlyDictionary<string, object>(attributes.ToDictionary(x => x.Name, x => x.Value));
        }

        public IReadOnlyDictionary<string, object> Attributes { get; private set; }

        public string PrettyPrinted
        {
            get
            {
                var sb = new StringBuilder();
                PrettyPrint(sb, 0);
                return sb.ToString();
            }
        }

        private void PrettyPrint(StringBuilder sb, int indentLevel, bool skip = false)
        {
            var attributes = Attributes.ToArray();

            if (attributes.Length == 1 && IsSimple(attributes[0].Value))
            {
                sb.Append("{");

                sb.Append(" ")
                    .Append(attributes[0].Key).Append(": ").Append(MinifyObject(attributes[0].Value))
                    .Append(" ");
            }
            else
            {
                if (indentLevel > 0 && attributes.Length > 0 && !skip)
                {
                    sb.AppendLine().Append(Indent(indentLevel));
                }

                sb.Append("{");

                if (attributes.Length > 0)
                {
                    var separator = IsSimple(attributes[0].Value as dynamic) ? ": " : ":";
                    sb.AppendLine().Append(Indent(indentLevel + 1))
                        .Append(attributes[0].Key).Append(separator).Append(PrettyPrintObject(attributes[0].Value, indentLevel + 1, false));
                }

                for (int i = 1; i < attributes.Length; i++)
                {
                    var separator = IsSimple(attributes[i].Value as dynamic) ? ": " : ":";

                    sb.AppendLine(",").Append(Indent(indentLevel + 1))
                        .Append(attributes[i].Key).Append(separator).Append(PrettyPrintObject(attributes[i].Value, indentLevel + 1, false));
                }

                if (attributes.Length > 0)
                {
                    sb.AppendLine().Append(Indent(indentLevel));
                }
            }

            sb.Append("}");
        }

        private static string Indent(int indentLevel)
        {
            return new string(' ', indentLevel * IndentSize);
        }

        private string PrettyPrintObject(object obj, int indentLevel, bool skip)
        {
            var sb = new StringBuilder();

            var dataTag = obj as DataTag;
            if (dataTag != null)
            {
                dataTag.PrettyPrint(sb, indentLevel, skip);
            }
            else
            {
                var array = obj as object[];
                if (array != null)
                {
                    if (IsSimple(array))
                    {
                        sb.Append("[ ");

                        if (array.Length > 0)
                        {
                            sb.Append(MinifyObject(array[0]));
                        }

                        for (int i = 1; i < array.Length; i++)
                        {
                            sb.Append(", ").Append(MinifyObject(array[i]));
                        }

                        sb.Append(" ]");
                    }
                    else
                    {
                        if (array.Length > 1 && !skip)
                        {
                            sb.AppendLine().Append(Indent(indentLevel));
                        }

                        sb.Append("[");

                        if (array.Length == 1)
                        {
                            sb.Append(PrettyPrintObject(array[0], indentLevel, false));
                        }
                        else if (array.Length > 1)
                        {
                            sb.AppendLine().Append(Indent(indentLevel + 1)).Append(PrettyPrintObject(array[0], indentLevel + 1, true));

                            for (int i = 1; i < array.Length; i++)
                            {
                                sb.Append(",").AppendLine().Append(Indent(indentLevel + 1)).Append(PrettyPrintObject(array[i], indentLevel + 1, true));
                            }

                            sb.AppendLine().Append(Indent(indentLevel));
                        }

                        sb.Append("]");
                    }
                }
                else
                {
                    var s = obj as string;
                    if (s != null)
                    {
                        sb.Append(GetStringValue(s));
                    }
                    else
                    {
                        sb.Append(obj);
                    }
                }
            }

            return sb.ToString();
        }

        internal static string GetStringValue(string value)
        {
            var problemChars = new[] { ',', ']', '}', '"' };

            if (value.Any(c => problemChars.Contains(c)))
            {
                return '"' + value.Replace("\"", "\\\"") + '"';
            }
            else
            {
                return value;
            }
        }

        internal static bool IsSimple(object[] array)
        {
            return array.Length == 0 || array.All(IsSimple);
        }

        internal static bool IsSimple(DataTag dataTag)
        {
            return dataTag.Attributes.Count == 0
                   || (dataTag.Attributes.Count == 1 && IsSimple((dynamic)dataTag.Attributes.First().Value));
        }

        internal static bool IsSimple(object obj)
        {
            if (obj is double)
            {
                return true;
            }

            var s = obj as string;
            if (s != null)
            {
                return s.Length <= 25;
            }

            var dt = obj as DataTag;
            if (dt != null)
            {
                return dt.Attributes.Count == 0;
            }

            var array = (object[])obj;
            return array.Length == 0;
        }

        public string Minified
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("{");

                var items = Attributes.ToArray();

                if (items.Length > 0)
                {
                    sb.Append(items[0].Key).Append(":").Append(MinifyObject(items[0].Value));
                }

                for (int i = 1; i < items.Length; i++)
                {
                    sb.Append(",").Append(items[i].Key).Append(":").Append(MinifyObject(items[i].Value));
                }

                sb.Append("}");

                return sb.ToString();
            }
        }

        private string MinifyObject(object obj)
        {
            var sb = new StringBuilder();

            var dataTag = obj as DataTag;
            if (dataTag != null)
            {
                sb.Append(dataTag.Minified);
            }
            else
            {
                var array = obj as object[];
                if (array != null)
                {
                    sb.Append("[");

                    if (array.Length > 0)
                    {
                        sb.Append(MinifyObject(array[0]));
                    }

                    for (int i = 1; i < array.Length; i++)
                    {
                        sb.Append(",").Append(MinifyObject(array[i]));
                    }

                    sb.Append("]");
                }
                else
                {
                    var s = obj as string;
                    if (s != null)
                    {
                        sb.Append(GetStringValue(s));
                    }
                    else
                    {
                        sb.Append(obj);
                    }
                }
            }

            return sb.ToString();
        }

        internal static Parser<DataTag> GetParser()
        {
            var nameParser =
                Parse.Char(c =>
                {
                    switch (c)
                    {
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                        case '}':
                        case ']':
                        case ':':
                            return false;
                        default:
                            return true;
                    }
                }, "valid characters for data tage names").AtLeastOnce().Text();

            var numberParser =
                from negative in Parse.Char('-').Optional()
                from value in Parse.DecimalInvariant
                select (object)(Double.Parse(value) * (negative.IsDefined ? -1 : 1));

            Parser<DataTag> dataTagParser = null;
            Parser<object[]> listParser = null;

            // ReSharper disable AccessToModifiedClosure
            var valueParser =
                ((Parser<object>)Parse.Ref(() => dataTagParser))
                    .Or(Parse.Ref(() => listParser))
                    .Or(numberParser)
                    .Or(CommandParsers.StringParser);
            // ReSharper restore AccessToModifiedClosure

            var itemParser =
                from name in nameParser
                from colon in Parse.Char(':').Token()
                from value in valueParser
                select new DataTagItem(name, value);

            listParser =
                from openBracket in Parse.Char('[')
                from items in valueParser.OptionallyDelimitedBy(Parse.Char(',').Token()).Token<IEnumerable<object>>()
                from closeBracket in Parse.Char(']')
                select items.ToArray();

            dataTagParser =
                from openBrace in Parse.Char('{').Token()
                from items in itemParser.OptionallyDelimitedBy(Parse.Char(',').Token()).Token<IEnumerable<DataTagItem>>()
                from closeBrace in Parse.Char('}').Token()
                select new DataTag(items);

            return dataTagParser;
        }
    }
}