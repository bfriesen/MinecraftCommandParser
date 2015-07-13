using System.Collections.Generic;

namespace MinecraftCommandParser
{
    public class AllPlayersSelector : PlayerSelector
    {
        public AllPlayersSelector(IDictionary<TargetSelectorArgument, object> arguments)
            : base('a', arguments)
        {
        }
    }
}