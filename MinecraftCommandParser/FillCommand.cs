using System.Text;
using Sprache;

namespace MinecraftCommandParser
{
    public class FillCommand : IFormattable
    {
        public FillCommand(
            Location location1,
            Location location2,
            Asset block,
            int? dataValue = null,
            FillCommandOldBlockHandling? oldBlockHandling = null,
            DataTag dataTag = null)
        {
            Location1 = location1;
            Location2 = location2;
            Block = block;
            DataValue = dataValue;
            OldBlockHandling = oldBlockHandling;
            DataTag = dataTag;
        }

        public Location Location1 { get; private set; }
        public Location Location2 { get; private set; }
        public Asset Block { get; private set; }
        public int? DataValue { get; private set; }
        public FillCommandOldBlockHandling? OldBlockHandling { get; private set; }
        public DataTag DataTag { get; private set; }

        public string PrettyPrinted
        {
            get
            {
                var sb = FormatBeginning();

                if (DataTag != null)
                {
                    if (DataTag.IsSimple(DataTag))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.AppendLine();
                    }

                    sb.Append(DataTag.PrettyPrinted);
                }

                return sb.ToString();
            }
        }

        public string Minified
        {
            get
            {
                var sb = FormatBeginning();

                if (DataTag != null)
                {
                    sb.Append(" ").Append(DataTag.Minified);
                }

                return sb.ToString();
            }
        }

        private StringBuilder FormatBeginning()
        {
            var sb = new StringBuilder();

            sb.Append("/fill ").Append(Location1).Append(" ").Append(Location2).Append(" ").Append(Block);

            if (DataValue.HasValue)
            {
                sb.Append(" ").Append(DataValue.Value);
            }

            if (OldBlockHandling.HasValue)
            {
                sb.Append(" ").Append(OldBlockHandling.Value);
            }

            return sb;
        }

        internal static Parser<FillCommand> GetParser()
        {
            var dataValueParser = CommandParsers.GetRangeParser(0, 16);
            var oldBlockHandlingParser = CommandParsers.GetEnumParser<FillCommandOldBlockHandling>();
            var dataTagParser = DataTag.GetParser();

            return
                from fill in Parse.String("fill")
                from w1 in Parse.WhiteSpace.AtLeastOnce()
                from location1 in CommandParsers.LocationParser
                from w2 in Parse.WhiteSpace.AtLeastOnce()
                from location2 in CommandParsers.LocationParser
                from w3 in Parse.WhiteSpace.AtLeastOnce()
                from block in CommandParsers.BlockParser
                from dataValue in
                    (from w4 in Parse.WhiteSpace.AtLeastOnce()
                        from dv in dataValueParser
                        select dv).Optional()
                from oldBlockHandling in
                    (from w5 in Parse.WhiteSpace.AtLeastOnce()
                        from obh in oldBlockHandlingParser
                        select obh).Optional()
                from dataTag in
                    (from w6 in Parse.WhiteSpace.AtLeastOnce()
                        from dt in dataTagParser
                        select dt).Optional()
                select new FillCommand(
                    location1,
                    location2,
                    block,
                    (dataValue.IsDefined ? (int?)dataValue.Get() : null),
                    (oldBlockHandling.IsDefined ? (FillCommandOldBlockHandling?)oldBlockHandling.Get() : null),
                    dataTag.GetOrDefault());
        }
    }
}