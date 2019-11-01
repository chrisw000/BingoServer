using BlueCheese.HostedServices.Bingo.Contracts;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public interface ILobbyHub
    {
        Task LobbyNewGameHasStarted(IGameData newGame);
        Task LobbyUserJoinedGame(IGameData newGame, string message);
        Task LobbyUpdateGame(IGameData gameData, string message);
        Task LobbyPlayerMessage(IGameData gameData, string message);
        Task LobbyPlayerNumbers(IGameData gameData, IPlayerData player);

        Task ReceiveChatMessage(IHoldUserIdentity user, string message);
    }
}