using System;
using System.Collections.Generic;

namespace BlueCheese.HostedServices.Game
{
    public interface IGameData
    {
        Guid GameId {get;}
        DateTime StartedUtc {get;}
        string StartedByUser {get;}
        int CheeseCount {get;}
        int GameSize {get; }
        GameStatus Status {get;}
        int GameRound {get;}
        IReadOnlyList<int> NumbersDrawn {get;}
        IReadOnlyList<IPlayerData> Players {get;}
    }
}
