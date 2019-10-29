using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IPlayerData
    {
        string User {get;}
        bool HasWon {get;}
        IEnumerable<IDrawData> Draws {get;}
    }
}
