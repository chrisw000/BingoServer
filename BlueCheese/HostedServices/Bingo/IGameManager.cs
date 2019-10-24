using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGameManager : IHostedServiceProvider
    {
        IEnumerable<IGameData> GameData {get;}

        Task JoinGameAsync(string connectionId, string user, System.Guid gameId);
        Task StartNewGameAsync(string connectionId, NewGameStarted newGame);
    }
}
