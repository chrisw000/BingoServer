using System;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IEndPlayerManager
    {
        IEndPlayerInfo SpawnEndPlayer(string username);
        bool CheckUserAgainstId(IHoldUserIdentity userIdentity);
        IEndPlayerInfo StoreConnection(IEndPlayerInfo endPlayerInfo);
        IEndPlayerInfo GetBy(Guid playerId);
        IEndPlayerInfo GetBy(IHoldUserIdentity userIdentity);
        IEndPlayerInfo GetBy(string connectionId);
    }
}
