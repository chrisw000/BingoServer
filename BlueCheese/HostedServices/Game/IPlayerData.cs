using System.Collections.Generic;

namespace BlueCheese.HostedServices.Game
{
    public interface IPlayerData
    {
        IReadOnlyList<int> Numbers {get;}
        string ConnectionId {get;}
        string User {get;}
        bool HasWon {get;}
    }
}
