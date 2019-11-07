using System;
using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IPlayerData
    {
        Guid PlayerId { get; }
        string User { get; }
        bool HasWon { get; }
        IEnumerable<IDrawData> Draws { get; }
    }
}
