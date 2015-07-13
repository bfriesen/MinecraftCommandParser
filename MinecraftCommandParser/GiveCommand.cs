using System;
using System.Text;
using System.Text.RegularExpressions;
using Sprache;

namespace MinecraftCommandParser
{
    public class GiveCommand : IFormattable
    {
        public GiveCommand(IFormattable player, Asset item, int? amount = null, int? data = null, DataTag dataTag = null)
        {
            if (player == null) throw new ArgumentNullException("player");
            if (!(player is PlayerSelector || player is Player)) throw new ArgumentException("player must be a PlayerSelector or Player.", "player");
            if (item == null) throw new ArgumentNullException("item");
            if (item.Type == AssetType.Entity) throw new ArgumentException("item must have a type of Item or Block.", "item");
            if (amount < 1 || amount > 64) throw new ArgumentException("amount must be between 1 and 64, inclusive.", "amount");

            Player = player;
            Item = item;
            Amount = amount;
            Data = data;
            DataTag = dataTag;
        }

        public IFormattable Player { get; private set; }
        public Asset Item { get; private set; }
        public int? Amount { get; private set; }
        public int? Data { get; private set; }
        public DataTag DataTag { get; private set; }

        public string PrettyPrinted
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append("/give ")
                    .Append(Player.PrettyPrinted)
                    .Append(" ")
                    .Append(Item);

                if (Amount.HasValue)
                {
                    sb.Append(" ").Append(Amount.Value);
                }

                if (Data.HasValue)
                {
                    sb.Append(" ").Append(Data.Value);
                }

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
                var sb = new StringBuilder();

                sb.Append("/give ")
                    .Append(Player.Minified)
                    .Append(" ")
                    .Append(Item);

                if (Amount.HasValue)
                {
                    sb.Append(" ").Append(Amount.Value);
                }

                if (Data.HasValue)
                {
                    sb.Append(" ").Append(Data.Value);
                }

                if (DataTag != null)
                {
                    sb.Append(" ").Append(DataTag.Minified);
                }

                return sb.ToString();
            }
        }

        internal static Parser<GiveCommand> GetParser()
        {
            // give <player> <item> [amount] [data] [dataTag]

            /*Arguments
                player
                Specifies the target to give item(s) to. Must be a player name or target selector.
                item
                Specifies the item to give. Must be a valid item id (for example, minecraft:iron_shovel),
                or block id for which items exist. Numerical ids are unsupported.
                amount (optional)
                Specifies the number of items to give. Must be between 1 and 64 (inclusive), but can be 64 even when that's more than one stack. If not specified, defaults to 1.
                data (optional)
                Specifies the item data of the given item(s). Must be an integer between -2,147,483,648 and 2,147,483,647 (inclusive, without the commas), but values which are invalid for the specified item id revert to 0. If not specified, defaults to 0.
                dataTag (optional)
                Specifies the data tag of the given item(s). Must be a compound NBT tag (for example, {display:{Name:Fred}}).*/

            var playerParser =
                ((Parser<IFormattable>)TargetSelector.GetParser<PlayerSelector>('p', 'r', 'a'))
                    .Or(CommandParsers.PlayerParser);

            var itemParser = CommandParsers.ItemParser.Or(CommandParsers.BlockParser);

            var amountRegex = new Regex(@"(?:[1-9]|[1-5][0-9]|6[0-4])\b", RegexOptions.Compiled);

            return
                from give in Parse.String("give")
                from w1 in Parse.WhiteSpace.AtLeastOnce()
                from player in playerParser
                from w2 in Parse.WhiteSpace.AtLeastOnce()
                from item in itemParser
                from amount in
                   (from w3 in Parse.WhiteSpace.AtLeastOnce()
                    from a in Parse.Regex(amountRegex)
                    select int.Parse(a)).Optional()
                from data in
                   (from w4 in Parse.WhiteSpace.AtLeastOnce()
                    from negative in Parse.Char('-').Optional()
                    from d in Parse.Number
                    select (int.Parse(d) * (negative.IsDefined ? -1 : 1))).Optional()
                from dataTag in
                    (from w5 in Parse.WhiteSpace.AtLeastOnce()
                     from dt in CommandParsers.DataTagParser
                     select dt).Optional()
                select new GiveCommand(
                    player,
                    item,
                    amount.IsDefined ? (int?)amount.Get() : null,
                    data.IsDefined ? (int?)data.Get() : null,
                    dataTag.GetOrDefault());
        }
    }
}