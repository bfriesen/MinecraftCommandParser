using System.Collections.Generic;

namespace MinecraftCommandParser
{
    public class EntitySelector : TargetSelector
    {
        public EntitySelector(IDictionary<TargetSelectorArgument, object> arguments)
            : base('e', arguments)
        {
        }
    }
}