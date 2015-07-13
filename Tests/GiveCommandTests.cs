using System;
using MinecraftCommandParser;
using NUnit.Framework;
using Sprache;

namespace Tests
{
    public class GiveCommandTests
    {
        [Test]
        public void CanParseGiveCommands()
        {
            var commandText = @"/give John minecraft:planks 29 -1234567 {foo:bar,baz:[123,456]}";

            var command = (GiveCommand)CommandParsers.CommandParser.Parse(commandText);

            var player = (Player)command.Player;
            Assert.That(player.Name, Is.EqualTo("John"));

            Assert.That(command.Item.Type, Is.EqualTo(AssetType.Block));
            Assert.That(command.Item.Id, Is.EqualTo("planks"));

            Assert.That(command.Amount, Is.EqualTo(29));

            Assert.That(command.Data, Is.EqualTo(-1234567));

            Assert.That(command.DataTag.Attributes["foo"], Is.EqualTo("bar"));
            var baz = (object[])command.DataTag.Attributes["baz"];
            Assert.That(baz, Is.EqualTo(new object[] { 123, 456 }));

            Console.WriteLine(command.Minified);
            Console.WriteLine(command.PrettyPrinted);
        }
    }
}
