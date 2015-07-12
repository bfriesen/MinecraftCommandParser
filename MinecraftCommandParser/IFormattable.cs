namespace MinecraftCommandParser
{
    public interface IFormattable
    {
        string PrettyPrinted { get; }
        string Minified { get; }
    }
}