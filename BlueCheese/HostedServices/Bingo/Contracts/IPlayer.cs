namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IPlayer : IPlayerData
    {
        bool CheckNumber(int number, int gameRound);
    }
}
