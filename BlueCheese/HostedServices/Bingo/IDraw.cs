namespace BlueCheese.HostedServices.Bingo
{
    public interface IDraw : IDrawData
    {
        bool IsMatched(int number);
    }
}
