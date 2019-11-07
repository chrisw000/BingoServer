using System.Collections.Generic;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IPlayerData
    {
        PlayerStatus Status {get; }
        IHoldUserIdentity Info { get; }
        IEnumerable<IDrawData> Draws { get; }
    }
}
