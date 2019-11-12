using System;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IEndPlayerManager
    {
        IHoldUserIdentity SpawnEndPlayer(string username);
        bool CheckUserAgainstId(IHoldUserIdentity userIdentity);
        IEndPlayerInfo StoreConnection(IEndPlayerInfo endPlayerInfo);
        EndPlayerInfo GetBy(Guid playerId);
        IEndPlayerInfo GetBy(IHoldUserIdentity userIdentity);
        IEndPlayerInfo GetByConnection(string connectionId);
        IEndPlayerInfo GetByUser(string user);
    }
}
