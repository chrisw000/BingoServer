using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IGameManager : IHostedServiceProvider
    {
        IEnumerable<IGameData> GameData { get; }

        Task JoinGameAsync(JoinGame joinGame);

        Task<IGame> StartNewGameAsync(NewGameStarted newGame);

        Task ClientReconnectedAsync(IEndPlayerInfo endPlayerInfo);
        Task PushSelectionAsync(PushSelection pushSelection);
    }
}
