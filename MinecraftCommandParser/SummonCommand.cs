using System.Text;
using Sprache;

namespace MinecraftCommandParser
{
    public class SummonCommand : IFormattable
    {
        public SummonCommand(
            Asset entity,
            Location location,
            DataTag dataTag = null)
        {
            Entity = entity;
            Location = location;
            DataTag = dataTag;
        }

        public Asset Entity { get; private set; }
        public Location Location { get; private set; }
        public DataTag DataTag { get; private set; }

        public string PrettyPrinted
        {
            get
            {
                var sb = FormatBeginning();

                if (DataTag != null)
                {
                    sb.AppendLine().Append(DataTag.PrettyPrinted);
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

            sb.Append("/summon ").Append(Entity);

            if (Location != null)
            {
                sb.Append(" ").Append(Location);
            }

            return sb;
        }

        internal static Parser<SummonCommand> GetParser()
        {
            return
                from summon in Parse.String("summon")
                from w1 in Parse.WhiteSpace.AtLeastOnce()
                from entity in CommandParsers.EntityParser
                from location in
                    (from w2 in Parse.WhiteSpace.AtLeastOnce()
                        from l in CommandParsers.LocationParser
                        select l).Optional()
                from dataTag in
                    (from w2 in Parse.WhiteSpace.AtLeastOnce()
                        from dt in CommandParsers.DataTagParser
                        select dt).Optional()
                select new SummonCommand(entity, location.GetOrDefault(), dataTag.GetOrDefault());
        }
    }
}