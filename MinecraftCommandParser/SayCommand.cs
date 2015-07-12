using System;
using Sprache;

namespace MinecraftCommandParser
{
    public class SayCommand : IFormattable
    {
        public SayCommand(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            Message = message;
        }

        public string Message { get; private set; }

        public string PrettyPrinted
        {
            get { return String.Format("/say {0}", DataTag.GetStringValue(Message)); }
        }

        public string Minified
        {
            get { return PrettyPrinted; }
        }

        internal static Parser<SayCommand> GetParser()
        {
            return
                from say in Parse.String("say")
                from w1 in Parse.WhiteSpace.AtLeastOnce()
                from message in CommandParsers.StringParser
                select new SayCommand(message);
        }
    }
}