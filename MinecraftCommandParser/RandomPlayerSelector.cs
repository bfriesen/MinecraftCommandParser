using System.Collections.Generic;

namespace MinecraftCommandParser
{
    public class RandomPlayerSelector : PlayerSelector
    {
        public RandomPlayerSelector(IDictionary<TargetSelectorArgument, object> arguments)
            : base('r', arguments)
        {
        }
    }
}