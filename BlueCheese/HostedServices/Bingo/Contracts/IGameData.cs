using System;
using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IGameData
    {
        Guid GameId { get; }
        DateTime StartedUtc { get; }
        string StartedByUser { get; }
        int CheeseCount { get; }
        int Size { get; }
        GameStatus Status { get; }
        GameMode Mode { get; }
        string Name { get; }
        int GameRound { get; }
        IEnumerable<IDrawData> Numbers { get; }
        IEnumerable<IPlayerData> Players { get; }
    }
}
