using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Game
{
    public interface IGameManager : IHostedServiceProvider
    {
        IReadOnlyCollection<IGameData> GameData {get;}

        Task JoinGameAsync(string connectionId, string user, System.Guid gameId);
        Task StartNewGameAsync(string connectionId, string user, int cheeseCount);
    }
}
