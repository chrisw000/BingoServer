using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IEndPlayerManager
    {
        IEndPlayerInfo SpawnEndPlayer(string username);
        bool CheckUserAgainstId(IHoldUserIdentity userIdentity);
        void StoreConnection(JoinGame joinGame);
        IEndPlayerInfo GetByPlayerId(Guid playerId);
    }
}
