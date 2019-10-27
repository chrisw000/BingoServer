using System;
using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGameData
    {
        Guid GameId {get;}
        DateTime StartedUtc {get;}
        string StartedByUser {get;}
        int CheeseCount {get;}
        int Size {get; }
        GameStatus Status {get;}
        GameMode Mode {get;}
        int GameRound {get;}
        IReadOnlyList<int> NumbersDrawn {get;}
        IReadOnlyList<string> NumberNames {get;}
        IReadOnlyList<IPlayerData> Players {get;}
    }
}
