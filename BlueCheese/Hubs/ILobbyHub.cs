using BlueCheese.HostedServices.Bingo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public interface ILobbyHub
    {
        Task LobbyNewGameHasStarted(IGameData newGame);
        Task LobbyUserJoinedGame(IGameData newGame, string user, string message);
        Task LobbyUpdateGame(IGameData gameData, string message);
        Task LobbyPlayerMessage(IGameData gameData, string message);
        Task LobbyPlayerNumbers(IGameData gameData, IReadOnlyList<int> numbers);
    }
}