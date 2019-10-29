namespace BlueCheese.HostedServices.Bingo
{
    public interface IDrawData
    {
        int Number {get;}
        string Name {get;}
        bool Matched {get;}
    }
}
