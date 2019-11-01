using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IEndPlayerManager
    {
        IEndPlayerInfo SpawnEndPlayer(string username);
        bool CheckUserAgainstId(IHoldUserIdentity userIdentity);
        IEndPlayerInfo StoreConnection(IEndPlayerInfo endPlayerInfo);
        IEndPlayerInfo GetByPlayerId(Guid playerId);
    }
}
