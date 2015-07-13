using System.Collections.Generic;

namespace MinecraftCommandParser
{
    public class PlayerSelector : TargetSelector
    {
        public PlayerSelector(IDictionary<TargetSelectorArgument, object> arguments)
            : base('p', arguments)
        {
        }

        protected PlayerSelector(char variable, IDictionary<TargetSelectorArgument, object> arguments)
            : base(variable, arguments)
        {
        }
    }
}