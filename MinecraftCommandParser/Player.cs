using System;
using System.Text.RegularExpressions;
using Sprache;

namespace MinecraftCommandParser
{
    public class Player : IFormattable
    {
        public Player(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
        }

        public string Name { get; private set; }

        public string PrettyPrinted { get { return Name; } }
        public string Minified { get { return Name; } }

        internal static Parser<Player> GetParser()
        {
            var nameRegex = new Regex("[a-zA-Z0-9_]+", RegexOptions.Compiled);

            return
                from name in Parse.Regex(nameRegex)
                select new Player(name);
        }
    }
}