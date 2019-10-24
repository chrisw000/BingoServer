namespace BlueCheese.HostedServices.Bingo
{
    public interface IPlayer : IPlayerData
    {
        bool CheckNumber(int number);
    }
}
