namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IDrawData
    {
        int Number { get; }
        string Name { get; }
        bool Matched { get; }
        int? GameRound { get; }
    }
}
