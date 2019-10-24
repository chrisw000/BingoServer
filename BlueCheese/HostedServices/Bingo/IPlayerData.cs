using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IPlayerData
    {
        IReadOnlyList<int> Numbers {get;}
        string User {get;}
        bool HasWon {get;}
    }
}
