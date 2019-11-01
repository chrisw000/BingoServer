namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IDraw : IDrawData
    {
        bool IsMatched(int number, int drawnOrder);
    }
}
