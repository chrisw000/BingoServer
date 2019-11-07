namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IPlayer : IPlayerData
    {
        new IEndPlayerInfo Info { get; }
        bool CheckNumber(int number, int gameRound);
    }
}
